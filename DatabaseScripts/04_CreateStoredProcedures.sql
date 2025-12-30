USE GameLibraryDB;
GO

CREATE PROCEDURE sp_TestConnection
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 1 AS Result;
END
GO

CREATE PROCEDURE sp_CheckEmailExists
    @Email NVARCHAR(255),
    @Exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        SET @Exists = 1;
    ELSE
        SET @Exists = 0;
END
GO

CREATE PROCEDURE sp_AuthenticateUser
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(255),
    @UserID INT OUTPUT,
    @Username NVARCHAR(100) OUTPUT,
    @Role NVARCHAR(20) OUTPUT,
    @ProfileImageURL NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        SELECT 
            @UserID = UserID,
            @Username = Username,
            @Role = Role,
            @ProfileImageURL = ProfileImageURL
        FROM Users
        WHERE Email = @Email AND PasswordHash = @PasswordHash;
        
        IF @UserID IS NULL
        BEGIN
            SET @UserID = 0;
            SET @Username = NULL;
            SET @Role = NULL;
            SET @ProfileImageURL = NULL;
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_RegisterUser
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(255),
    @Username NVARCHAR(100),
    @UserID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        BEGIN
            SET @UserID = 0;
            RAISERROR('Email already exists', 16, 1)
            RETURN -1
        END
        
        INSERT INTO Users (Email, PasswordHash, Username, Role)
        VALUES (@Email, @PasswordHash, @Username, 'User')
        
        SET @UserID = SCOPE_IDENTITY()
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_UpdateLastLogin
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        UPDATE Users
        SET LastLogin = GETDATE()
        WHERE UserID = @UserID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('User not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            UserID,
            Email,
            Username,
            ProfileImageURL,
            Role,
            CreatedAt,
            LastLogin
        FROM Users
        ORDER BY CreatedAt DESC
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_UpdateUserRole
    @UserID INT,
    @Role NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF @Role NOT IN ('User', 'Admin')
        BEGIN
            RAISERROR('Invalid role. Must be User or Admin', 16, 1)
            RETURN -1
        END
        
        UPDATE Users
        SET Role = @Role
        WHERE UserID = @UserID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('User not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_DeleteUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM AuditLog WHERE UserID = @UserID;

        DELETE FROM UserGames WHERE UserID = @UserID;

        DELETE FROM AuditLog WHERE UserID = @UserID;

        DELETE FROM Users
        WHERE UserID = @UserID;
        
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('User not found', 16, 1);
            RETURN -1;
        END
        
        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetAllGames
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            g.GameID,
            g.Title,
            g.Developer,
            g.Publisher,
            g.ReleaseYear,
            g.Description,
            g.CoverImageURL,
            g.CreatedAt,
            g.CreatedBy,
            u.Username AS CreatedByUsername,
            STUFF((
                SELECT ', ' + gen.GenreName
                FROM GameGenres gg
                INNER JOIN Genres gen ON gg.GenreID = gen.GenreID
                WHERE gg.GameID = g.GameID
                FOR XML PATH('')
            ), 1, 2, '') AS Genres,
            STUFF((
                SELECT ', ' + p.PlatformName
                FROM GamePlatforms gp
                INNER JOIN Platforms p ON gp.PlatformID = p.PlatformID
                WHERE gp.GameID = g.GameID
                FOR XML PATH('')
            ), 1, 2, '') AS Platforms
        FROM Games g
        LEFT JOIN Users u ON g.CreatedBy = u.UserID
        ORDER BY g.Title
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetGameByID
    @GameID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            g.GameID,
            g.Title,
            g.Developer,
            g.Publisher,
            g.ReleaseYear,
            g.Description,
            g.CoverImageURL,
            g.CreatedAt,
            g.CreatedBy,
            u.Username AS CreatedByUsername
        FROM Games g
        LEFT JOIN Users u ON g.CreatedBy = u.UserID
        WHERE g.GameID = @GameID

        SELECT gen.GenreID, gen.GenreName
        FROM GameGenres gg
        INNER JOIN Genres gen ON gg.GenreID = gen.GenreID
        WHERE gg.GameID = @GameID

        SELECT p.PlatformID, p.PlatformName
        FROM GamePlatforms gp
        INNER JOIN Platforms p ON gp.PlatformID = p.PlatformID
        WHERE gp.GameID = @GameID
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_InsertGame
    @Title NVARCHAR(255),
    @Developer NVARCHAR(255) = NULL,
    @Publisher NVARCHAR(255) = NULL,
    @ReleaseYear INT = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @CoverImageURL NVARCHAR(500) = NULL,
    @CreatedBy INT,
    @GenreIDs NVARCHAR(MAX) = NULL,
    @PlatformIDs NVARCHAR(MAX) = NULL,
    @GameID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        INSERT INTO Games (Title, Developer, Publisher, ReleaseYear, Description, CoverImageURL, CreatedBy)
        VALUES (@Title, @Developer, @Publisher, @ReleaseYear, @Description, @CoverImageURL, @CreatedBy)
        
        SET @GameID = SCOPE_IDENTITY()

        IF @GenreIDs IS NOT NULL AND LEN(@GenreIDs) > 0
        BEGIN
            INSERT INTO GameGenres (GameID, GenreID)
            SELECT @GameID, CAST(value AS INT)
            FROM STRING_SPLIT(@GenreIDs, ',')
            WHERE value IS NOT NULL AND value != ''
        END

        IF @PlatformIDs IS NOT NULL AND LEN(@PlatformIDs) > 0
        BEGIN
            INSERT INTO GamePlatforms (GameID, PlatformID)
            SELECT @GameID, CAST(value AS INT)
            FROM STRING_SPLIT(@PlatformIDs, ',')
            WHERE value IS NOT NULL AND value != ''
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_UpdateGame
    @GameID INT,
    @Title NVARCHAR(255),
    @Developer NVARCHAR(255) = NULL,
    @Publisher NVARCHAR(255) = NULL,
    @ReleaseYear INT = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @CoverImageURL NVARCHAR(500) = NULL,
    @GenreIDs NVARCHAR(MAX) = NULL,
    @PlatformIDs NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        UPDATE Games
        SET 
            Title = @Title,
            Developer = @Developer,
            Publisher = @Publisher,
            ReleaseYear = @ReleaseYear,
            Description = @Description,
            CoverImageURL = @CoverImageURL
        WHERE GameID = @GameID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Game not found', 16, 1)
            RETURN -1
        END

        DELETE FROM GameGenres WHERE GameID = @GameID

        IF @GenreIDs IS NOT NULL AND LEN(@GenreIDs) > 0
        BEGIN
            INSERT INTO GameGenres (GameID, GenreID)
            SELECT @GameID, CAST(value AS INT)
            FROM STRING_SPLIT(@GenreIDs, ',')
            WHERE value IS NOT NULL AND value != ''
        END

        DELETE FROM GamePlatforms WHERE GameID = @GameID

        IF @PlatformIDs IS NOT NULL AND LEN(@PlatformIDs) > 0
        BEGIN
            INSERT INTO GamePlatforms (GameID, PlatformID)
            SELECT @GameID, CAST(value AS INT)
            FROM STRING_SPLIT(@PlatformIDs, ',')
            WHERE value IS NOT NULL AND value != ''
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_DeleteGame
    @GameID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DELETE FROM Games
        WHERE GameID = @GameID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Game not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetUserLibrary
    @UserID INT,
    @Status NVARCHAR(50) = NULL,
    @GenreID INT = NULL,
    @PlatformID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT DISTINCT
            ug.UserGameID,
            ug.UserID,
            ug.GameID,
            ug.Status,
            ug.PersonalRating,
            ug.PersonalNotes,
            ug.HoursPlayed,
            ug.DateAdded,
            ug.LastModified,
            g.Title,
            g.Developer,
            g.Publisher,
            g.ReleaseYear,
            g.Description,
            g.CoverImageURL
        FROM UserGames ug
        INNER JOIN Games g ON ug.GameID = g.GameID
        LEFT JOIN GameGenres gg ON g.GameID = gg.GameID
        LEFT JOIN GamePlatforms gp ON g.GameID = gp.GameID
        WHERE ug.UserID = @UserID
            AND (@Status IS NULL OR ug.Status = @Status)
            AND (@GenreID IS NULL OR gg.GenreID = @GenreID)
            AND (@PlatformID IS NULL OR gp.PlatformID = @PlatformID)
        ORDER BY ug.DateAdded DESC
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_AddGameToLibrary
    @UserID INT,
    @GameID INT,
    @Status NVARCHAR(50) = 'Backlog'
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF EXISTS (SELECT 1 FROM UserGames WHERE UserID = @UserID AND GameID = @GameID)
        BEGIN
            RAISERROR('Game already in library', 16, 1)
            RETURN -1
        END

        INSERT INTO UserGames (UserID, GameID, Status, HoursPlayed)
        VALUES (@UserID, @GameID, @Status, 0)
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_UpdateUserGame
    @UserGameID INT,
    @Status NVARCHAR(50) = NULL,
    @PersonalRating DECIMAL(3,1) = NULL,
    @PersonalNotes NVARCHAR(MAX) = NULL,
    @HoursPlayed DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        UPDATE UserGames
        SET 
            Status = ISNULL(@Status, Status),
            PersonalRating = CASE WHEN @PersonalRating IS NULL THEN PersonalRating ELSE @PersonalRating END,
            PersonalNotes = CASE WHEN @PersonalNotes IS NULL THEN PersonalNotes ELSE @PersonalNotes END,
            HoursPlayed = CASE WHEN @HoursPlayed IS NULL THEN HoursPlayed ELSE @HoursPlayed END,
            LastModified = GETDATE()
        WHERE UserGameID = @UserGameID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('User game entry not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_RemoveGameFromLibrary
    @UserID INT,
    @GameID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DELETE FROM UserGames
        WHERE UserID = @UserID AND GameID = @GameID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Game not found in library', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetUserGameDetails
    @UserID INT,
    @GameID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            ug.UserGameID,
            ug.UserID,
            ug.GameID,
            ug.Status,
            ug.PersonalRating,
            ug.PersonalNotes,
            ug.HoursPlayed,
            ug.DateAdded,
            ug.LastModified,
            g.Title,
            g.Developer,
            g.Publisher,
            g.ReleaseYear,
            g.Description,
            g.CoverImageURL
        FROM UserGames ug
        INNER JOIN Games g ON ug.GameID = g.GameID
        WHERE ug.UserID = @UserID AND ug.GameID = @GameID

        SELECT gen.GenreID, gen.GenreName
        FROM GameGenres gg
        INNER JOIN Genres gen ON gg.GenreID = gen.GenreID
        WHERE gg.GameID = @GameID

        SELECT p.PlatformID, p.PlatformName
        FROM GamePlatforms gp
        INNER JOIN Platforms p ON gp.PlatformID = p.PlatformID
        WHERE gp.GameID = @GameID
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetAllGenres
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT GenreID, GenreName
        FROM Genres
        ORDER BY GenreName
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetAllPlatforms
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT PlatformID, PlatformName
        FROM Platforms
        ORDER BY PlatformName
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_InsertGenre
    @GenreName NVARCHAR(100),
    @GenreID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF EXISTS (SELECT 1 FROM Genres WHERE GenreName = @GenreName)
        BEGIN
            SET @GenreID = 0;
            RAISERROR('Genre already exists', 16, 1)
            RETURN -1
        END
        
        INSERT INTO Genres (GenreName)
        VALUES (@GenreName)
        
        SET @GenreID = SCOPE_IDENTITY()
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_InsertPlatform
    @PlatformName NVARCHAR(100),
    @PlatformID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF EXISTS (SELECT 1 FROM Platforms WHERE PlatformName = @PlatformName)
        BEGIN
            SET @PlatformID = 0;
            RAISERROR('Platform already exists', 16, 1)
            RETURN -1
        END
        
        INSERT INTO Platforms (PlatformName)
        VALUES (@PlatformName)
        
        SET @PlatformID = SCOPE_IDENTITY()
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_UpdateGenre
    @GenreID INT,
    @GenreName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF EXISTS (SELECT 1 FROM Genres WHERE GenreName = @GenreName AND GenreID != @GenreID)
        BEGIN
            RAISERROR('Genre name already exists', 16, 1)
            RETURN -1
        END
        
        UPDATE Genres
        SET GenreName = @GenreName
        WHERE GenreID = @GenreID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Genre not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_UpdatePlatform
    @PlatformID INT,
    @PlatformName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF EXISTS (SELECT 1 FROM Platforms WHERE PlatformName = @PlatformName AND PlatformID != @PlatformID)
        BEGIN
            RAISERROR('Platform name already exists', 16, 1)
            RETURN -1
        END
        
        UPDATE Platforms
        SET PlatformName = @PlatformName
        WHERE PlatformID = @PlatformID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Platform not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_DeleteGenre
    @GenreID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF EXISTS (SELECT 1 FROM GameGenres WHERE GenreID = @GenreID)
        BEGIN
            RAISERROR('Cannot delete genre: it is associated with games', 16, 1)
            RETURN -1
        END
        
        DELETE FROM Genres
        WHERE GenreID = @GenreID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Genre not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_DeletePlatform
    @PlatformID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF EXISTS (SELECT 1 FROM GamePlatforms WHERE PlatformID = @PlatformID)
        BEGIN
            RAISERROR('Cannot delete platform: it is associated with games', 16, 1)
            RETURN -1
        END
        
        DELETE FROM Platforms
        WHERE PlatformID = @PlatformID
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Platform not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_SearchGames
    @SearchTerm NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            g.GameID,
            g.Title,
            g.Developer,
            g.Publisher,
            g.ReleaseYear,
            g.Description,
            g.CoverImageURL
        FROM Games g
        WHERE g.Title LIKE '%' + @SearchTerm + '%'
            OR g.Developer LIKE '%' + @SearchTerm + '%'
            OR g.Publisher LIKE '%' + @SearchTerm + '%'
        ORDER BY g.Title
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetUserStatistics
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            COUNT(*) AS TotalGames,
            SUM(CASE WHEN Status = 'Playing' THEN 1 ELSE 0 END) AS PlayingCount,
            SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedCount,
            SUM(CASE WHEN Status = 'Backlog' THEN 1 ELSE 0 END) AS BacklogCount,
            SUM(CASE WHEN Status = 'Wishlist' THEN 1 ELSE 0 END) AS WishlistCount,
            SUM(CASE WHEN Status = 'Dropped' THEN 1 ELSE 0 END) AS DroppedCount,
            AVG(PersonalRating) AS AverageRating,
            SUM(HoursPlayed) AS TotalHoursPlayed
        FROM UserGames
        WHERE UserID = @UserID
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_UpdateUserProfile
    @UserID INT,
    @Username NVARCHAR(100),
    @ProfileImageURL NVARCHAR(500) = NULL,
    @NewPasswordHash NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        IF @NewPasswordHash IS NOT NULL
        BEGIN
            UPDATE Users
            SET 
                Username = @Username,
                ProfileImageURL = @ProfileImageURL,
                PasswordHash = @NewPasswordHash
            WHERE UserID = @UserID
        END
        ELSE
        BEGIN
            UPDATE Users
            SET 
                Username = @Username,
                ProfileImageURL = @ProfileImageURL
            WHERE UserID = @UserID
        END
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('User not found', 16, 1)
            RETURN -1
        END
        
        COMMIT TRANSACTION
        RETURN 0
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetUserById
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            UserID,
            Email,
            Username,
            ProfileImageURL,
            Role,
            CreatedAt,
            LastLogin
        FROM Users
        WHERE UserID = @UserID
        
        RETURN 0
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
        RETURN -1
    END CATCH
END
GO

CREATE PROCEDURE sp_GetAuditLog
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @TableName NVARCHAR(100) = NULL,
    @Action NVARCHAR(50) = NULL,
    @UserID INT = NULL,
    @Limit INT = 1000
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Limit)
        al.LogID,
        al.UserID,
        u.Username,
        u.Email,
        al.TableName,
        al.Action,
        al.RecordID,
        al.FieldName,
        al.OldValue,
        al.NewValue,
        al.Timestamp
    FROM AuditLog al
    LEFT JOIN Users u ON al.UserID = u.UserID
    WHERE 
        (@StartDate IS NULL OR al.Timestamp >= @StartDate)
        AND (@EndDate IS NULL OR al.Timestamp <= @EndDate)
        AND (@TableName IS NULL OR al.TableName = @TableName)
        AND (@Action IS NULL OR al.Action = @Action)
        AND (@UserID IS NULL OR al.UserID = @UserID)
    ORDER BY al.Timestamp DESC;
END
GO

CREATE PROCEDURE sp_CreateReport
    @UserID INT,
    @Type NVARCHAR(20),
    @Title NVARCHAR(255),
    @Content NVARCHAR(MAX),
    @ReportID INT OUTPUT,
    @ErrorMessage NVARCHAR(MAX) = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        INSERT INTO Reports (UserID, Type, Title, Content, Status)
        VALUES (@UserID, @Type, @Title, @Content, 'Unreviewed');
        
        SET @ReportID = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        SET @ReportID = 0
        SET @ErrorMessage = ERROR_MESSAGE()
        RAISERROR(@ErrorMessage, 16, 1)
    END CATCH
END
GO

CREATE PROCEDURE sp_GetReports
    @Status NVARCHAR(20) = NULL,
    @Type NVARCHAR(20) = NULL,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.ReportID,
        r.UserID,
        r.Type,
        r.Title,
        r.Content,
        r.Status,
        r.ReviewedBy,
        r.AdminNotes,
        r.CreatedAt,
        r.ReviewedAt,
        u.Username AS CreatedByUsername,
        u.Email AS CreatedByEmail,
        ISNULL(u.ProfileImageURL, '') AS CreatedByProfileImageURL,
        reviewer.Username AS ReviewedByUsername,
        ISNULL(reviewer.ProfileImageURL, '') AS ReviewedByProfileImageURL
    FROM Reports r
    INNER JOIN Users u ON r.UserID = u.UserID
    LEFT JOIN Users reviewer ON r.ReviewedBy = reviewer.UserID
    WHERE 
        (@Status IS NULL OR r.Status = @Status)
        AND (@Type IS NULL OR r.Type = @Type)
        AND (@UserID IS NULL OR r.UserID = @UserID)
    ORDER BY 
        CASE WHEN r.Status = 'Unreviewed' THEN 0 ELSE 1 END,
        r.CreatedAt DESC;
END
GO

CREATE PROCEDURE sp_GetUnreviewedReportsCount
    @Count INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT @Count = COUNT(*)
    FROM Reports
    WHERE Status = 'Unreviewed';
END
GO

CREATE PROCEDURE sp_ReviewReport
    @ReportID INT,
    @ReviewedBy INT,
    @Status NVARCHAR(20),
    @AdminNotes NVARCHAR(MAX) = NULL,
    @Success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        UPDATE Reports
        SET Status = @Status,
            ReviewedBy = @ReviewedBy,
            AdminNotes = @AdminNotes,
            ReviewedAt = GETDATE()
        WHERE ReportID = @ReportID;
        
        IF @@ROWCOUNT > 0
            SET @Success = 1
        ELSE
            SET @Success = 0
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        SET @Success = 0
    END CATCH
END
GO

CREATE PROCEDURE sp_DeleteReport
    @ReportID INT,
    @Success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION
        
        DELETE FROM Reports
        WHERE ReportID = @ReportID;
        
        IF @@ROWCOUNT > 0
            SET @Success = 1
        ELSE
            SET @Success = 0
        
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        SET @Success = 0
    END CATCH
END
GO

PRINT 'All stored procedures created successfully!';
GO