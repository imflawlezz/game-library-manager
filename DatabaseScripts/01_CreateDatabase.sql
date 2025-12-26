USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'GameLibraryDB')
BEGIN
    ALTER DATABASE GameLibraryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE GameLibraryDB;
END
GO

CREATE DATABASE GameLibraryDB;
GO

USE GameLibraryDB;
GO

PRINT 'Database GameLibraryDB created successfully!';
GO

USE master;
GO