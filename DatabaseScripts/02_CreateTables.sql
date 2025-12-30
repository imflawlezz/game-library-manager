USE GameLibraryDB;
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    ProfileImageURL NVARCHAR(500) NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User' 
        CHECK (Role IN ('User', 'Admin')),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);
GO

CREATE TABLE Platforms (
    PlatformID INT IDENTITY(1,1) PRIMARY KEY,
    PlatformName NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE Genres (
    GenreID INT IDENTITY(1,1) PRIMARY KEY,
    GenreName NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE Games (
    GameID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Developer NVARCHAR(255) NULL,
    Publisher NVARCHAR(255) NULL,
    ReleaseYear INT NULL,
    Description NVARCHAR(MAX) NULL,
    CoverImageURL NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy INT NOT NULL,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserID) ON DELETE NO ACTION
);
GO

CREATE TABLE GameGenres (
    GameID INT NOT NULL,
    GenreID INT NOT NULL,
    PRIMARY KEY (GameID, GenreID),
    FOREIGN KEY (GameID) REFERENCES Games(GameID) ON DELETE CASCADE,
    FOREIGN KEY (GenreID) REFERENCES Genres(GenreID) ON DELETE CASCADE
);
GO

CREATE TABLE GamePlatforms (
    GameID INT NOT NULL,
    PlatformID INT NOT NULL,
    PRIMARY KEY (GameID, PlatformID),
    FOREIGN KEY (GameID) REFERENCES Games(GameID) ON DELETE CASCADE,
    FOREIGN KEY (PlatformID) REFERENCES Platforms(PlatformID) ON DELETE CASCADE
);
GO

CREATE TABLE UserGames (
    UserGameID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    GameID INT NOT NULL,
    Status NVARCHAR(50) NOT NULL 
        CHECK (Status IN ('Playing', 'Completed', 'Backlog', 'Wishlist', 'Dropped')),
    PersonalRating DECIMAL(3,1) NULL 
        CHECK (PersonalRating >= 0 AND PersonalRating <= 10),
    PersonalNotes NVARCHAR(MAX) NULL,
    HoursPlayed DECIMAL(10,2) NULL,
    DateAdded DATETIME NOT NULL DEFAULT GETDATE(),
    LastModified DATETIME NOT NULL DEFAULT GETDATE(),
    UNIQUE(UserID, GameID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (GameID) REFERENCES Games(GameID) ON DELETE CASCADE
);
GO

CREATE TABLE AuditLog (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL,
    TableName NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    RecordID INT NOT NULL,
    FieldName NVARCHAR(100) NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL
);
GO

CREATE TABLE Reports (
    ReportID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Type NVARCHAR(20) NOT NULL 
        CHECK (Type IN ('Report', 'Suggestion')),
    Title NVARCHAR(255) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Unreviewed'
        CHECK (Status IN ('Unreviewed', 'Reviewed', 'Resolved', 'Dismissed')),
    ReviewedBy INT NULL,
    AdminNotes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    ReviewedAt DATETIME NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (ReviewedBy) REFERENCES Users(UserID) ON DELETE NO ACTION
);
GO

PRINT 'All tables created successfully!';
GO