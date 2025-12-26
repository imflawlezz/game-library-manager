USE GameLibraryDB;
GO

CREATE TRIGGER trg_UserGames_AuditLog
ON UserGames
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, NewValue)
        SELECT i.UserID, 'UserGames', 'INSERT', i.UserGameID, 'GameID', CAST(i.GameID AS NVARCHAR(10)) FROM inserted i
        UNION ALL
        SELECT i.UserID, 'UserGames', 'INSERT', i.UserGameID, 'Status', i.Status FROM inserted i
        UNION ALL
        SELECT i.UserID, 'UserGames', 'INSERT', i.UserGameID, 'PersonalRating', CAST(i.PersonalRating AS NVARCHAR(10)) FROM inserted i WHERE i.PersonalRating IS NOT NULL
        UNION ALL
        SELECT i.UserID, 'UserGames', 'INSERT', i.UserGameID, 'PersonalNotes', i.PersonalNotes FROM inserted i WHERE i.PersonalNotes IS NOT NULL
        UNION ALL
        SELECT i.UserID, 'UserGames', 'INSERT', i.UserGameID, 'HoursPlayed', CAST(i.HoursPlayed AS NVARCHAR(20)) FROM inserted i WHERE i.HoursPlayed IS NOT NULL;
    END

    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, OldValue, NewValue)
        SELECT i.UserID, 'UserGames', 'UPDATE', i.UserGameID, 'GameID', CAST(d.GameID AS NVARCHAR(10)), CAST(i.GameID AS NVARCHAR(10))
        FROM inserted i
        INNER JOIN deleted d ON i.UserGameID = d.UserGameID
        WHERE i.GameID != d.GameID

        UNION ALL

        SELECT i.UserID, 'UserGames', 'UPDATE', i.UserGameID, 'Status', d.Status, i.Status
        FROM inserted i
        INNER JOIN deleted d ON i.UserGameID = d.UserGameID
        WHERE i.Status != d.Status

        UNION ALL

        SELECT i.UserID, 'UserGames', 'UPDATE', i.UserGameID, 'PersonalRating', 
               CAST(d.PersonalRating AS NVARCHAR(10)), CAST(i.PersonalRating AS NVARCHAR(10))
        FROM inserted i
        INNER JOIN deleted d ON i.UserGameID = d.UserGameID
        WHERE (i.PersonalRating IS NULL AND d.PersonalRating IS NOT NULL)
           OR (i.PersonalRating IS NOT NULL AND d.PersonalRating IS NULL)
           OR (i.PersonalRating != d.PersonalRating)

        UNION ALL

        SELECT i.UserID, 'UserGames', 'UPDATE', i.UserGameID, 'PersonalNotes', d.PersonalNotes, i.PersonalNotes
        FROM inserted i
        INNER JOIN deleted d ON i.UserGameID = d.UserGameID
        WHERE ISNULL(i.PersonalNotes, '') != ISNULL(d.PersonalNotes, '')

        UNION ALL

        SELECT i.UserID, 'UserGames', 'UPDATE', i.UserGameID, 'HoursPlayed',
               CAST(d.HoursPlayed AS NVARCHAR(20)), CAST(i.HoursPlayed AS NVARCHAR(20))
        FROM inserted i
        INNER JOIN deleted d ON i.UserGameID = d.UserGameID
        WHERE (i.HoursPlayed IS NULL AND d.HoursPlayed IS NOT NULL)
           OR (i.HoursPlayed IS NOT NULL AND d.HoursPlayed IS NULL)
           OR (i.HoursPlayed != d.HoursPlayed);
    END

    IF EXISTS (SELECT * FROM deleted) AND NOT EXISTS (SELECT * FROM inserted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, OldValue)
        SELECT d.UserID, 'UserGames', 'DELETE', d.UserGameID, 'GameID', CAST(d.GameID AS NVARCHAR(10)) FROM deleted d
        UNION ALL
        SELECT d.UserID, 'UserGames', 'DELETE', d.UserGameID, 'Status', d.Status FROM deleted d;
    END
END
GO

CREATE TRIGGER trg_Games_AuditLog
ON Games
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, NewValue)
        SELECT i.CreatedBy, 'Games', 'INSERT', i.GameID, 'Title', i.Title FROM inserted i
        UNION ALL
        SELECT i.CreatedBy, 'Games', 'INSERT', i.GameID, 'Developer', i.Developer FROM inserted i WHERE i.Developer IS NOT NULL
        UNION ALL
        SELECT i.CreatedBy, 'Games', 'INSERT', i.GameID, 'Publisher', i.Publisher FROM inserted i WHERE i.Publisher IS NOT NULL
        UNION ALL
        SELECT i.CreatedBy, 'Games', 'INSERT', i.GameID, 'ReleaseYear', CAST(i.ReleaseYear AS NVARCHAR(10)) FROM inserted i WHERE i.ReleaseYear IS NOT NULL
        UNION ALL
        SELECT i.CreatedBy, 'Games', 'INSERT', i.GameID, 'Description', i.Description FROM inserted i WHERE i.Description IS NOT NULL
        UNION ALL
        SELECT i.CreatedBy, 'Games', 'INSERT', i.GameID, 'CoverImageURL', i.CoverImageURL FROM inserted i WHERE i.CoverImageURL IS NOT NULL;
    END

    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, OldValue, NewValue)
        SELECT i.CreatedBy, 'Games', 'UPDATE', i.GameID, 'Title', d.Title, i.Title
        FROM inserted i
        INNER JOIN deleted d ON i.GameID = d.GameID
        WHERE i.Title != d.Title

        UNION ALL

        SELECT i.CreatedBy, 'Games', 'UPDATE', i.GameID, 'Developer', d.Developer, i.Developer
        FROM inserted i
        INNER JOIN deleted d ON i.GameID = d.GameID
        WHERE ISNULL(i.Developer, '') != ISNULL(d.Developer, '')

        UNION ALL

        SELECT i.CreatedBy, 'Games', 'UPDATE', i.GameID, 'Publisher', d.Publisher, i.Publisher
        FROM inserted i
        INNER JOIN deleted d ON i.GameID = d.GameID
        WHERE ISNULL(i.Publisher, '') != ISNULL(d.Publisher, '')

        UNION ALL

        SELECT i.CreatedBy, 'Games', 'UPDATE', i.GameID, 'ReleaseYear',
               CAST(d.ReleaseYear AS NVARCHAR(10)), CAST(i.ReleaseYear AS NVARCHAR(10))
        FROM inserted i
        INNER JOIN deleted d ON i.GameID = d.GameID
        WHERE ISNULL(i.ReleaseYear, -1) != ISNULL(d.ReleaseYear, -1)

        UNION ALL

        SELECT i.CreatedBy, 'Games', 'UPDATE', i.GameID, 'Description', d.Description, i.Description
        FROM inserted i
        INNER JOIN deleted d ON i.GameID = d.GameID
        WHERE ISNULL(i.Description, '') != ISNULL(d.Description, '')

        UNION ALL

        SELECT i.CreatedBy, 'Games', 'UPDATE', i.GameID, 'CoverImageURL', d.CoverImageURL, i.CoverImageURL
        FROM inserted i
        INNER JOIN deleted d ON i.GameID = d.GameID
        WHERE ISNULL(i.CoverImageURL, '') != ISNULL(d.CoverImageURL, '');
    END

    IF EXISTS (SELECT * FROM deleted) AND NOT EXISTS (SELECT * FROM inserted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, OldValue)
        SELECT d.CreatedBy, 'Games', 'DELETE', d.GameID, 'Title', d.Title FROM deleted d;
    END
END
GO

CREATE TRIGGER trg_Users_AuditLog
ON Users
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, NewValue)
        SELECT i.UserID, 'Users', 'INSERT', i.UserID, 'Username', i.Username FROM inserted i
        UNION ALL
        SELECT i.UserID, 'Users', 'INSERT', i.UserID, 'Email', i.Email FROM inserted i
        UNION ALL
        SELECT i.UserID, 'Users', 'INSERT', i.UserID, 'Role', i.Role FROM inserted i
        UNION ALL
        SELECT i.UserID, 'Users', 'INSERT', i.UserID, 'ProfileImageURL', i.ProfileImageURL FROM inserted i WHERE i.ProfileImageURL IS NOT NULL;
    END

    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, OldValue, NewValue)
        SELECT i.UserID, 'Users', 'UPDATE', i.UserID, 'Username', d.Username, i.Username
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        WHERE i.Username != d.Username

        UNION ALL

        SELECT i.UserID, 'Users', 'UPDATE', i.UserID, 'Email', d.Email, i.Email
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        WHERE i.Email != d.Email

        UNION ALL

        SELECT i.UserID, 'Users', 'UPDATE', i.UserID, 'Role', d.Role, i.Role
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        WHERE i.Role != d.Role

        UNION ALL

        SELECT i.UserID, 'Users', 'UPDATE', i.UserID, 'ProfileImageURL', d.ProfileImageURL, i.ProfileImageURL
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        WHERE ISNULL(i.ProfileImageURL, '') != ISNULL(d.ProfileImageURL, '');
    END
    
    IF EXISTS (SELECT * FROM deleted) AND NOT EXISTS (SELECT * FROM inserted)
    BEGIN
        INSERT INTO AuditLog (UserID, TableName, Action, RecordID, FieldName, OldValue)
        SELECT NULL, 'Users', 'DELETE', d.UserID, 'Username', d.Username FROM deleted d;
    END
END
GO

CREATE TRIGGER trg_UpdateLastModified
ON UserGames
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE UserGames
    SET LastModified = GETDATE()
    FROM UserGames ug
    INNER JOIN inserted i ON ug.UserGameID = i.UserGameID
    WHERE ug.LastModified = i.LastModified
END
GO

PRINT 'All triggers created successfully!';
GO

