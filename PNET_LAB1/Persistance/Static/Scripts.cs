namespace Persistance.Static;

public static class Scripts
{
    public const string GetUserPassRate = @"
        CREATE OR ALTER FUNCTION GetUserPassRate(@userId UNIQUEIDENTIFIER)
        RETURNS DECIMAL(5,2)
        AS
        BEGIN
            IF @userId IS NULL
                RETURN -1.00

            IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @userId)
                RETURN -1.00

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
        IF OBJECT_ID('dbo.GetTestLeaderboard', 'IF') IS NOT NULL
            DROP FUNCTION dbo.GetTestLeaderboard
        IF OBJECT_ID('dbo.GetTestLeaderboard', 'TF') IS NOT NULL
            DROP FUNCTION dbo.GetTestLeaderboard";

    public const string CreateGetTestLeaderboard = @"
        CREATE FUNCTION GetTestLeaderboard(@testId UNIQUEIDENTIFIER)
        RETURNS @Result TABLE (
            Rank          BIGINT,
            UserId        UNIQUEIDENTIFIER,
            FullName      NVARCHAR(255),
            Email         NVARCHAR(255),
            Score         DECIMAL(5,2),
            AttemptNumber INT,
            StartedAt     DATETIME,
            FinishedAt    DATETIME,
            Passed        BIT
        )
        AS
        BEGIN
            IF @testId IS NULL
                RETURN

            IF NOT EXISTS (SELECT 1 FROM Tests WHERE Id = @testId)
                RETURN

            IF NOT EXISTS (
                SELECT 1 FROM TestAttempts
                WHERE TestId = @testId
                AND Status = 'Completed'
            )
                RETURN

            INSERT INTO @Result
            SELECT
                ROW_NUMBER() OVER (ORDER BY ta.Score DESC),
                u.Id,
                u.FirstName + ' ' + u.LastName,
                u.Email,
                ta.Score,
                ta.AttemptNumber,
                ta.StartedAt,
                ta.FinishedAt,
                CAST(CASE
                    WHEN ta.Score >= t.PassingScore THEN 1
                    ELSE 0
                END AS BIT)
            FROM TestAttempts ta
            JOIN Users u ON ta.UserId = u.Id
            JOIN Tests t ON ta.TestId = t.Id
            WHERE ta.TestId = @testId
            AND ta.Status = 'Completed'

            RETURN
        END";


    public const string RecalculateScoreAfterAnswer = @"
        CREATE OR ALTER TRIGGER RecalculateScoreAfterAnswer
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
                DECLARE @testId UNIQUEIDENTIFIER

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

    public const string CleanupOldAttempts = @"
        CREATE OR ALTER PROCEDURE CleanupOldAttempts(@hours INT)
        AS
        BEGIN
            SET NOCOUNT ON;

            IF @hours IS NULL
            BEGIN
                RAISERROR('Parameter @hours cannot be null.', 16, 1)
                RETURN
            END

            IF @hours <= 0
            BEGIN
                RAISERROR('Parameter @hours must be greater than 0.', 16, 1)
                RETURN
            END

            DECLARE @cutoff  DATETIME = DATEADD(HOUR, -@hours, GETUTCDATE())
            DECLARE @cleaned INT

            IF NOT EXISTS (
                SELECT 1 FROM TestAttempts
                WHERE Status = 'InProgress'
                AND StartedAt < @cutoff
            )
            BEGIN
                SELECT 0 AS CleanedAttempts,
                       'No stale attempts found.' AS Message
                RETURN
            END

            UPDATE TestAttempts
            SET
                Status     = 'Abandoned',
                FinishedAt = GETUTCDATE(),
                Score      = 0
            WHERE Status = 'InProgress'
            AND StartedAt < @cutoff

            SET @cleaned = @@ROWCOUNT
        END";
}