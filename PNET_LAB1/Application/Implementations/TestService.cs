using Application.Contacts;
using Application.Models;
using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Application.Implementations;

public class TestService : ITestService
{
    private readonly ITestRepository _testRepo;
    private readonly ICategoryRepository _categoryRepo;

    public TestService(ITestRepository testRepo, ICategoryRepository categoryRepo)
    {
        _testRepo = testRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<TestResponse> CreateTestAsync(Guid userId, CreateTestRequest request)
    {
        var category = await _categoryRepo.GetByIdAsync(request.CategoryId)
            ?? throw new KeyNotFoundException("Category not found.");

        var test = new Test
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = request.MaxAttempts,
            ShuffleQuestions = request.ShuffleQuestions,
            ShowCorrectAnswers = request.ShowCorrectAnswers,
            PassingScore = request.PassingScore,
            CreatedAt = DateTime.UtcNow,
            Questions = request.Questions.Select(q => new Question
            {
                Id = Guid.NewGuid(),
                Text = q.Text,
                QuestionType = q.QuestionType,
                Points = q.Points,
                Order = q.Order,
                Explanation = q.Explanation,
                Answers = q.Answers.Select(a => new Answer
                {
                    Id = Guid.NewGuid(),
                    Text = a.Text,
                    IsCorrect = a.IsCorrect,
                    Order = a.Order
                }).ToList()
            }).ToList()
        };

        await _testRepo.AddAsync(test);

        return new TestResponse(test.Id, test.Title, test.Description,
            category.Name, test.TimeLimitMinutes, test.MaxAttempts,
            test.PassingScore, test.Questions.Count, test.CreatedAt);
    }

    public async Task<TestDetailResponse?> GetTestByIdAsync(Guid id, bool isOwnerOrAdmin = false)
    {
        var test = await _testRepo.GetByIdAsync(id, includeQuestions: true);
        if (test is null) return null;

        var questions = (test.ShuffleQuestions && !isOwnerOrAdmin
            ? test.Questions.OrderBy(_ => Guid.NewGuid())
            : test.Questions.OrderBy(q => q.Order))
            .Select(q => new QuestionResponse(
                q.Id, q.Text, q.QuestionType, q.Points, q.Order, q.Explanation,
                q.Answers.OrderBy(a => a.Order)
                    .Select(a => new AnswerResponse(a.Id, a.Text, a.Order,
                        isOwnerOrAdmin ? a.IsCorrect : null))
                    .ToList()))
            .ToList();

        return new TestDetailResponse(
            test.Id, test.Title, test.Description,
            test.Category?.Name ?? "", test.TimeLimitMinutes,
            test.MaxAttempts, test.ShuffleQuestions,
            test.ShowCorrectAnswers, test.PassingScore,
            test.CreatedAt, test.UserId, questions);
    }

    public async Task<IEnumerable<TestResponse>> GetAllTestsAsync()
    {
        var tests = await _testRepo.GetAllAsync();
        return tests.Select(t => new TestResponse(t.Id, t.Title, t.Description,
            t.Category?.Name ?? "", t.TimeLimitMinutes, t.MaxAttempts,
            t.PassingScore, t.Questions?.Count ?? 0, t.CreatedAt));
    }

    public async Task<IEnumerable<TestResponse>> GetTestsByUserAsync(Guid userId)
    {
        var tests = await _testRepo.GetByUserIdAsync(userId);
        return tests.Select(t => new TestResponse(t.Id, t.Title, t.Description,
            t.Category?.Name ?? "", t.TimeLimitMinutes, t.MaxAttempts,
            t.PassingScore, t.Questions?.Count ?? 0, t.CreatedAt));
    }

    public async Task UpdateTestAsync(Guid testId, Guid requestingUserId, CreateTestRequest request)
    {
        var test = await _testRepo.GetByIdAsync(testId, includeQuestions: true)
            ?? throw new KeyNotFoundException("Test not found.");

        if (test.UserId != requestingUserId)
            throw new UnauthorizedAccessException("You don't own this test.");

        test.Title = request.Title;
        test.Description = request.Description;
        test.CategoryId = request.CategoryId;
        test.TimeLimitMinutes = request.TimeLimitMinutes;
        test.MaxAttempts = request.MaxAttempts;
        test.ShuffleQuestions = request.ShuffleQuestions;
        test.ShowCorrectAnswers = request.ShowCorrectAnswers;
        test.PassingScore = request.PassingScore;

        test.Questions = request.Questions.Select(q => new Question
        {
            Id = Guid.NewGuid(),
            TestId = testId,
            Text = q.Text,
            QuestionType = q.QuestionType,
            Points = q.Points,
            Order = q.Order,
            Explanation = q.Explanation,
            Answers = q.Answers.Select(a => new Answer
            {
                Id = Guid.NewGuid(),
                Text = a.Text,
                IsCorrect = a.IsCorrect,
                Order = a.Order
            }).ToList()
        }).ToList();

        await _testRepo.UpdateAsync(test);
    }

    public async Task DeleteTestAsync(Guid testId, Guid requestingUserId)
    {
        var test = await _testRepo.GetByIdAsync(testId)
            ?? throw new KeyNotFoundException("Test not found.");

        if (test.UserId != requestingUserId)
            throw new UnauthorizedAccessException("You don't own this test.");

        try
        {
            await _testRepo.DeleteAsync(testId);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException?.Message.Contains("Cannot delete test") == true)
        {
            throw new InvalidOperationException(ex.InnerException.Message);
        }
    }
    
    public async Task<IEnumerable<TestResponse>> GetByCategoryIdAsync(Guid categoryId)
    {
        var tests = await _testRepo.GetByCategoryIdAsync(categoryId);
        return tests.Select(t => MapToTestResponse(t, t.Category?.Name ?? ""));
    }
    
    private static TestResponse MapToTestResponse(Test t, string categoryName) =>
        new(t.Id, t.Title, t.Description, categoryName,
            t.TimeLimitMinutes, t.MaxAttempts, t.PassingScore,
            t.Questions?.Count ?? 0, t.CreatedAt);
}