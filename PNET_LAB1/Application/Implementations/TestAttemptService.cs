using Application.Contacts;
using Application.Models;
using Domain.Enums;
using Domain.Model;

namespace Application.Implementations;

public class TestAttemptService : ITestAttemptService
{
    private readonly ITestAttemptRepository _attemptRepo;
    private readonly ITestRepository _testRepo;
    private readonly IUserAnswerRepository _userAnswerRepo;

    public TestAttemptService(
        ITestAttemptRepository attemptRepo,
        ITestRepository testRepo,
        IUserAnswerRepository userAnswerRepo)
    {
        _attemptRepo = attemptRepo;
        _testRepo = testRepo;
        _userAnswerRepo = userAnswerRepo;
    }

    public async Task<IEnumerable<AttemptSummaryResponse>> GetUserAttemptsAsync(Guid userId)
    {
        var attempts = await _attemptRepo.GetByUserIdAsync(userId);

        return attempts.Select(a => new AttemptSummaryResponse(
            a.Id,
            a.TestId,
            a.Test?.Title ?? "",
            a.Test?.PassingScore ?? 0,
            a.StartedAt,
            a.FinishedAt,
            a.Status.ToString(),
            a.Score,
            a.AttemptNumber
        ));
    }
    
    public async Task<StartAttemptResponse> StartAttemptAsync(Guid userId, Guid testId)
    {
        var test = await _testRepo.GetByIdAsync(testId, includeQuestions: true)
            ?? throw new KeyNotFoundException("Test not found.");

        var existing = await _attemptRepo.GetActiveAttemptAsync(userId, testId);
        if (existing is not null)
            throw new InvalidOperationException("You already have an active attempt for this test.");

        if (test.MaxAttempts.HasValue)
        {
            var completedCount = await _attemptRepo.CountAttemptsAsync(userId, testId);
            if (completedCount >= test.MaxAttempts.Value)
                throw new InvalidOperationException(
                    $"You have reached the maximum number of attempts ({test.MaxAttempts.Value}).");
        }

        var attemptNumber = await _attemptRepo.CountAttemptsAsync(userId, testId) + 1;

        var attempt = new TestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TestId = testId,
            StartedAt = DateTime.UtcNow,
            Status = AttemptStatus.InProgress,
            AttemptNumber = attemptNumber
        };

        await _attemptRepo.AddAsync(attempt);

        var questions = (test.ShuffleQuestions
                ? test.Questions.OrderBy(_ => Guid.NewGuid())
                : test.Questions.OrderBy(q => q.Order))
            .Select(q => new QuestionResponse(
                q.Id, q.Text, q.QuestionType, q.Points, q.Order,
                null,
                q.Answers.OrderBy(a => a.Order)
                    .Select(a => new AnswerResponse(a.Id, a.Text, a.Order, null))
                    .ToList()))
            .ToList();

        return new StartAttemptResponse(
            attempt.Id, testId, test.Title,
            test.TimeLimitMinutes, attempt.StartedAt, questions);
    }

    public async Task<AttemptResultResponse> SubmitAttemptAsync(Guid userId, Guid attemptId, SubmitAttemptRequest request)
    {
        var attempt = await _attemptRepo.GetByIdAsync(attemptId)
            ?? throw new KeyNotFoundException("Attempt not found.");

        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException();

        if (attempt.Status != AttemptStatus.InProgress)
            throw new InvalidOperationException("This attempt is already completed.");

        var test = await _testRepo.GetByIdAsync(attempt.TestId, includeQuestions: true)
            ?? throw new KeyNotFoundException("Test not found.");

        var isTimedOut = test.TimeLimitMinutes.HasValue &&
                         DateTime.UtcNow > attempt.StartedAt.AddMinutes(test.TimeLimitMinutes.Value);

        if (isTimedOut)
        {
            attempt.Status = AttemptStatus.TimedOut;
            attempt.FinishedAt = DateTime.UtcNow;
            attempt.Score = 0;
            await _attemptRepo.UpdateAsync(attempt);

            return new AttemptResultResponse(
                attemptId, 0, test.PassingScore, false,
                test.Questions.Count, 0,
                attempt.StartedAt, attempt.FinishedAt!.Value,
                AttemptStatus.TimedOut, null);
        }

        var userAnswers = new List<UserAnswer>();
        foreach (var question in test.Questions)
        {
            var submitted = request.Answers
                .FirstOrDefault(a => a.QuestionId == question.Id);
            if (submitted is null) continue;
            (_, _, var rows) = EvaluateQuestion(question, submitted, attemptId);
            userAnswers.AddRange(rows);
        }

        await _userAnswerRepo.AddRangeAsync(userAnswers);

        var updated = await _attemptRepo.GetByIdAsync(attemptId)
            ?? throw new KeyNotFoundException("Attempt not found after submit.");

        var passed = updated.Score >= test.PassingScore;

        return new AttemptResultResponse(
            attemptId,
            updated.Score ?? 0,
            test.PassingScore,
            passed,
            test.Questions.Count,
            0,
            updated.StartedAt,
            updated.FinishedAt!.Value,
            updated.Status,
            null);
    }

    public async Task<AttemptResultResponse?> GetAttemptResultAsync(Guid userId, Guid attemptId)
    {
        var attempt = await _attemptRepo.GetByIdAsync(attemptId, includeUserAnswers: true);
        if (attempt is null || attempt.UserId != userId) return null;
        if (attempt.Status == AttemptStatus.InProgress) return null;

        var test = await _testRepo.GetByIdAsync(attempt.TestId, includeQuestions: true)!
            ?? throw new KeyNotFoundException("Test not found.");

        var questionResults = test.Questions.Select(q =>
        {
            var userAnswersForQuestion = attempt.UserAnswers
                .Where(ua => ua.QuestionId == q.Id).ToList();

            bool isCorrect = userAnswersForQuestion.Any(ua => ua.IsCorrect);
            decimal pointsEarned = isCorrect ? q.Points : 0;

            if (q.QuestionType == QuestionType.MultipleChoice)
            {
                var totalCorrect = q.Answers.Count(a => a.IsCorrect);
                if (totalCorrect > 0)
                {
                    int correctSelected = userAnswersForQuestion.Count(ua => ua.IsCorrect);
                    int wrongSelected = userAnswersForQuestion.Count(ua => !ua.IsCorrect
                        && ua.SelectedAnswerId.HasValue);
                    decimal ratio = Math.Max(0,
                        (decimal)(correctSelected - wrongSelected) / totalCorrect);
                    pointsEarned = Math.Round(ratio * q.Points, 2);
                    isCorrect = ratio == 1m;
                }
            }

            return new QuestionResultResponse(
                q.Id, q.Text, isCorrect, q.Points, pointsEarned, q.Explanation);
        }).ToList();

        return new AttemptResultResponse(
            attemptId, attempt.Score ?? 0, test.PassingScore,
            attempt.Score >= test.PassingScore,
            test.Questions.Count,
            questionResults.Count(r => r.IsCorrect),
            attempt.StartedAt, attempt.FinishedAt!.Value,
            attempt.Status,
            test.ShowCorrectAnswers ? questionResults : null);
    }
    
    private static (bool isCorrect, decimal pointsEarned, List<UserAnswer> rows)
        EvaluateQuestion(Question question, SubmitAnswerRequest submitted, Guid attemptId)
    {
        return question.QuestionType switch
        {
            QuestionType.SingleChoice or QuestionType.TrueFalse =>
                EvaluateSingleChoice(question, submitted, attemptId),

            QuestionType.MultipleChoice =>
                EvaluateMultipleChoice(question, submitted, attemptId),

            QuestionType.Text =>
                EvaluateText(question, submitted, attemptId),

            _ => (false, 0, new List<UserAnswer>())
        };
    }

    private static (bool, decimal, List<UserAnswer>) EvaluateSingleChoice(
        Question question, SubmitAnswerRequest submitted, Guid attemptId)
    {
        var selectedId = submitted.SelectedAnswerIds?.FirstOrDefault();
        var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == selectedId);
        bool isCorrect = selectedAnswer?.IsCorrect == true;

        return (isCorrect, isCorrect ? question.Points : 0, new List<UserAnswer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AttemptId = attemptId,
                QuestionId = question.Id,
                SelectedAnswerId = selectedId,
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.UtcNow
            }
        });
    }

    private static (bool, decimal, List<UserAnswer>) EvaluateMultipleChoice(
        Question question, SubmitAnswerRequest submitted, Guid attemptId)
    {
        var selectedIds = submitted.SelectedAnswerIds ?? new List<Guid>();
        var totalCorrect = question.Answers.Count(a => a.IsCorrect);

        var rows = selectedIds.Select(sid =>
        {
            var answer = question.Answers.FirstOrDefault(a => a.Id == sid);
            return new UserAnswer
            {
                Id = Guid.NewGuid(),
                AttemptId = attemptId,
                QuestionId = question.Id,
                SelectedAnswerId = sid,
                IsCorrect = answer?.IsCorrect ?? false,
                AnsweredAt = DateTime.UtcNow
            };
        }).ToList();

        int correctSelected = rows.Count(r => r.IsCorrect);
        int wrongSelected = rows.Count(r => !r.IsCorrect);

        decimal ratio = totalCorrect > 0
            ? Math.Max(0, (decimal)(correctSelected - wrongSelected) / totalCorrect)
            : 0;

        decimal pointsEarned = Math.Round(ratio * question.Points, 2);
        bool fullCredit = ratio == 1m;

        return (fullCredit, pointsEarned, rows);
    }

    private static (bool, decimal, List<UserAnswer>) EvaluateText(
        Question question, SubmitAnswerRequest submitted, Guid attemptId)
    {
        var correctText = question.Answers.FirstOrDefault(a => a.IsCorrect)?.Text ?? "";
        bool isCorrect = string.Equals(
            submitted.TextAnswer?.Trim(), correctText.Trim(),
            StringComparison.OrdinalIgnoreCase);

        return (isCorrect, isCorrect ? question.Points : 0, new List<UserAnswer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AttemptId = attemptId,
                QuestionId = question.Id,
                TextAnswer = submitted.TextAnswer,
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.UtcNow
            }
        });
    }
    
    public async Task<IEnumerable<AttemptResultResponse>> GetAttemptsByTestIdAsync(Guid testId)
    {
        var attempts = await _attemptRepo.GetByTestIdAsync(testId);

        return attempts
            .Where(a => a.Status == AttemptStatus.Completed)
            .Select(a => new AttemptResultResponse(
                a.Id,
                a.Score ?? 0,
                a.Test.PassingScore,
                a.Score >= a.Test.PassingScore,
                0,
                0,
                a.StartedAt,
                a.FinishedAt!.Value,
                a.Status,
                null
            ));
    }
}