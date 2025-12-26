USE GameLibraryDB;
GO

CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Users_Role ON Users(Role);
GO

CREATE NONCLUSTERED INDEX IX_Games_Title ON Games(Title);
CREATE NONCLUSTERED INDEX IX_Games_CreatedBy ON Games(CreatedBy);
GO

CREATE NONCLUSTERED INDEX IX_UserGames_UserID ON UserGames(UserID);
CREATE NONCLUSTERED INDEX IX_UserGames_Status ON UserGames(Status);
CREATE NONCLUSTERED INDEX IX_UserGames_Rating ON UserGames(PersonalRating);
CREATE NONCLUSTERED INDEX IX_UserGames_GameID ON UserGames(GameID);
GO

CREATE NONCLUSTERED INDEX IX_AuditLog_UserID_Timestamp ON AuditLog(UserID, Timestamp);
CREATE NONCLUSTERED INDEX IX_AuditLog_TableName ON AuditLog(TableName);
CREATE NONCLUSTERED INDEX IX_AuditLog_FieldName ON AuditLog(FieldName);
GO

CREATE NONCLUSTERED INDEX IX_GameGenres_GenreID ON GameGenres(GenreID);
GO

CREATE NONCLUSTERED INDEX IX_GamePlatforms_PlatformID ON GamePlatforms(PlatformID);
GO

PRINT 'All indexes created successfully!';
GO

