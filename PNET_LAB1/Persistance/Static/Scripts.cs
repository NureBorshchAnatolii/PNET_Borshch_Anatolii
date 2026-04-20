namespace Persistance.Static;

public static class Scripts
{
    public const string GetUserPassRate = @"
        CREATE OR ALTER FUNCTION GetUserPassRate(@userId UNIQUEIDENTIFIER)
        RETURNS DECIMAL(5,2)
        AS
        BEGIN
            DECLARE @total INT
            DECLARE @passed INT
            DECLARE @rate DECIMAL(5,2)

            SELECT @total = COUNT(*)
            FROM TestAttempts
            WHERE UserId = @userId
            AND Status = 'Completed'

            IF @total = 0
                RETURN 0.00

            SELECT @passed = COUNT(*)
            FROM TestAttempts ta
            JOIN Tests t ON ta.TestId = t.Id
            WHERE ta.UserId = @userId
            AND ta.Status = 'Completed'
            AND ta.Score >= t.PassingScore

            SET @rate = (CAST(@passed AS DECIMAL(5,2)) / @total) * 100

            RETURN @rate
        END";

    
    public const string GetTestLeaderboard = @"
        CREATE OR ALTER FUNCTION GetTestLeaderboard(@testId UNIQUEIDENTIFIER)
        RETURNS TABLE
        AS
        RETURN
        (
            SELECT
                ROW_NUMBER() OVER (ORDER BY ta.Score DESC) AS Rank,
                u.Id AS UserId,
                u.FirstName + ' ' + u.LastName AS FullName,
                u.Email AS Email,
                ta.Score AS Score,
                ta.AttemptNumber AS AttemptNumber,
                ta.StartedAt AS StartedAt,
                ta.FinishedAt AS FinishedAt,
                CASE
                    WHEN ta.Score >= t.PassingScore THEN 1
                    ELSE 0
                END AS Passed
            FROM TestAttempts ta
            JOIN Users u ON ta.UserId = u.Id
            JOIN Tests t ON ta.TestId = t.Id
            WHERE ta.TestId = @testId
            AND ta.Status = 'Completed'
        )";

    
    public const string RecalculateScoreAfterAnswer = @"
        CREATE OR ALTER TRIGGER trg_RecalculateScoreAfterAnswer
        ON UserAnswers
        AFTER INSERT
        AS
        BEGIN
            SET NOCOUNT ON;

            DECLARE @attemptId UNIQUEIDENTIFIER

            DECLARE cur_attempts CURSOR FOR
                SELECT DISTINCT AttemptId FROM inserted

            OPEN cur_attempts
            FETCH NEXT FROM cur_attempts INTO @attemptId

            WHILE @@FETCH_STATUS = 0
            BEGIN
                DECLARE @totalPoints  DECIMAL(10,2)
                DECLARE @earnedPoints DECIMAL(10,2)
                DECLARE @scorePercent DECIMAL(5,2)
                DECLARE @testId       UNIQUEIDENTIFIER

                SELECT @testId = TestId
                FROM TestAttempts
                WHERE Id = @attemptId

                SELECT @totalPoints = SUM(Points)
                FROM Questions
                WHERE TestId = @testId

                SELECT @earnedPoints = ISNULL(SUM(q.Points), 0)
                FROM UserAnswers ua
                JOIN Questions q ON ua.QuestionId = q.Id
                WHERE ua.AttemptId = @attemptId
                AND ua.IsCorrect = 1

                IF @totalPoints > 0
                    SET @scorePercent = (@earnedPoints / @totalPoints) * 100
                ELSE
                    SET @scorePercent = 0

                UPDATE TestAttempts
                SET
                    Score      = @scorePercent,
                    Status     = 'Completed',
                    FinishedAt = GETUTCDATE()
                WHERE Id = @attemptId

                FETCH NEXT FROM cur_attempts INTO @attemptId
            END

            CLOSE cur_attempts
            DEALLOCATE cur_attempts
        END";

    public const string PreventTestDeleteWithAttempts = @"
        CREATE OR ALTER TRIGGER PreventTestDeleteWithAttempts
        ON Tests
        INSTEAD OF DELETE
        AS
        BEGIN
            SET NOCOUNT ON;

            DECLARE @testId UNIQUEIDENTIFIER
            DECLARE @testTitle NVARCHAR(255)
            DECLARE @attemptCount INT

            SELECT @testId = Id, @testTitle = Title
            FROM deleted

            SELECT @attemptCount = COUNT(*)
            FROM TestAttempts
            WHERE TestId = @testId
            AND Status = 'Completed'

            IF @attemptCount > 0
            BEGIN
                RAISERROR(
                    'Cannot delete test - it has %d completed attempt(s).',
                    16, 1,
                    @attemptCount
                )
                RETURN
            END

            DELETE FROM Tests WHERE Id = @testId
        END";

    public const string FlagUnderperformingTests = @"
        CREATE OR ALTER PROCEDURE FlagUnderperformingTests
        AS
        BEGIN
            SET NOCOUNT ON;

            CREATE TABLE #TestReport (
                TestId UNIQUEIDENTIFIER,
                Title NVARCHAR(255),
                Category NVARCHAR(255),
                PassingScore DECIMAL(5,2),
                AvgScore DECIMAL(5,2),
                AttemptCount INT,
                Status NVARCHAR(50)
            )

            DECLARE @testId UNIQUEIDENTIFIER
            DECLARE @title NVARCHAR(255)
            DECLARE @passingScore DECIMAL(5,2)
            DECLARE @categoryId UNIQUEIDENTIFIER

            DECLARE cur_tests CURSOR FOR
                SELECT Id, Title, PassingScore, CategoryId
                FROM Tests
                ORDER BY Title

            OPEN cur_tests
            FETCH NEXT FROM cur_tests INTO @testId, @title, @passingScore, @categoryId

            WHILE @@FETCH_STATUS = 0
            BEGIN
                DECLARE @avgScore DECIMAL(5,2)
                DECLARE @attemptCount INT
                DECLARE @categoryName NVARCHAR(255)
                DECLARE @flag NVARCHAR(50)

                SELECT @categoryName = Name
                FROM Categories
                WHERE Id = @categoryId

                SELECT
                    @attemptCount = COUNT(*),
                    @avgScore = ISNULL(AVG(Score), 0)
                FROM TestAttempts
                WHERE TestId = @testId
                AND Status = 'Completed'

                SET @flag = CASE
                    WHEN @attemptCount = 0 THEN 'No Attempts'
                    WHEN @avgScore < @passingScore THEN 'Underperforming'
                    ELSE 'Healthy'
                END

                INSERT INTO #TestReport
                VALUES (@testId, @title, @categoryName,
                        @passingScore, @avgScore, @attemptCount, @flag)

                FETCH NEXT FROM cur_tests INTO @testId, @title, @passingScore, @categoryId
            END

            CLOSE cur_tests
            DEALLOCATE cur_tests

            SELECT * FROM #TestReport ORDER BY Status, AvgScore ASC
            DROP TABLE #TestReport
        END";
}