USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'GameLibraryDB')
BEGIN
    PRINT 'GameLibraryDB found. Starting deletion process...';

    DECLARE @kill VARCHAR(MAX) = '';
    
    SELECT @kill = @kill + 'KILL ' + CAST(spid AS VARCHAR(10)) + '; '
    FROM master.sys.sysprocesses 
    WHERE dbid = DB_ID('GameLibraryDB') AND spid <> @@SPID;
    
    IF LEN(@kill) > 0
    BEGIN
        PRINT 'Terminating active connections...';
        EXEC(@kill);
    END

    PRINT 'Setting database to single user mode...';
    ALTER DATABASE GameLibraryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

    PRINT 'Dropping database...';
    DROP DATABASE GameLibraryDB;

    PRINT 'GameLibraryDB has been successfully deleted!';
END
ELSE
BEGIN
    PRINT 'Database GameLibraryDB does not exist. Nothing to delete.';
END
GO

