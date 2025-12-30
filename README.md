# Game Library Manager

A cross-platform desktop application for managing personal game collections. This is a university project built with Avalonia UI and .NET 8, allowing users to organize, track, and discover games across multiple platforms.

## Features

- User authentication and role-based access control
- Personal game library with status tracking, ratings, and notes
- Game catalog management with genres and platforms
- Administrative tools for user and game management
- Audit logging and search functionality

## Technology Stack

- **Framework**: .NET 8.0
- **UI Framework**: Avalonia 11.3.9
- **Architecture**: MVVM with ReactiveUI
- **Database**: Microsoft SQL Server with stored procedures and triggers

## Database

### Tables

#### Users
User accounts with authentication, roles, and profile information.

| Column | Type | Description |
|--------|------|-------------|
| UserID | INT | Primary key, auto-incrementing |
| Email | NVARCHAR(255) | Unique email address for login |
| PasswordHash | NVARCHAR(255) | SHA256 hashed password |
| Username | NVARCHAR(100) | Display name |
| ProfileImageURL | NVARCHAR(500) | URL to user's profile image |
| Role | NVARCHAR(20) | User role: 'User' or 'Admin' (default: 'User') |
| CreatedAt | DATETIME | Account creation timestamp |
| LastLogin | DATETIME | Last login timestamp |

#### Games
Game catalog with metadata and information.

| Column | Type | Description |
|--------|------|-------------|
| GameID | INT | Primary key, auto-incrementing |
| Title | NVARCHAR(255) | Game title |
| Developer | NVARCHAR(255) | Game developer name |
| Publisher | NVARCHAR(255) | Game publisher name |
| ReleaseYear | INT | Year the game was released |
| Description | NVARCHAR(MAX) | Game description |
| CoverImageURL | NVARCHAR(500) | URL to game cover image |
| CreatedAt | DATETIME | Record creation timestamp |
| CreatedBy | INT | Foreign key to Users(UserID) |

#### Platforms
Supported gaming platforms.

| Column | Type | Description |
|--------|------|-------------|
| PlatformID | INT | Primary key, auto-incrementing |
| PlatformName | NVARCHAR(100) | Unique platform name (e.g., PC, PlayStation 5, Xbox Series X, Nintendo Switch) |

#### Genres
Game genre classifications.

| Column | Type | Description |
|--------|------|-------------|
| GenreID | INT | Primary key, auto-incrementing |
| GenreName | NVARCHAR(100) | Unique genre name (e.g., Action, RPG, Strategy) |

#### GameGenres
Many-to-many relationship between games and genres.

| Column | Type | Description |
|--------|------|-------------|
| GameID | INT | Foreign key to Games(GameID), part of composite primary key |
| GenreID | INT | Foreign key to Genres(GenreID), part of composite primary key |

#### GamePlatforms
Many-to-many relationship between games and platforms.

| Column | Type | Description |
|--------|------|-------------|
| GameID | INT | Foreign key to Games(GameID), part of composite primary key |
| PlatformID | INT | Foreign key to Platforms(PlatformID), part of composite primary key |

#### UserGames
Personal game library entries with user-specific tracking data.

| Column | Type | Description |
|--------|------|-------------|
| UserGameID | INT | Primary key, auto-incrementing |
| UserID | INT | Foreign key to Users(UserID) |
| GameID | INT | Foreign key to Games(GameID) |
| Status | NVARCHAR(50) | Game status: 'Playing', 'Completed', 'Backlog', 'Wishlist', or 'Dropped' |
| PersonalRating | DECIMAL(3,1) | User's personal rating (0-10) |
| PersonalNotes | NVARCHAR(MAX) | User's personal notes about the game |
| HoursPlayed | DECIMAL(10,2) | Number of hours played |
| DateAdded | DATETIME | Date when game was added to library |
| LastModified | DATETIME | Last modification timestamp |

#### AuditLog
Complete audit trail tracking all changes to Users, Games, and UserGames tables.

| Column | Type | Description |
|--------|------|-------------|
| LogID | INT | Primary key, auto-incrementing |
| UserID | INT | Foreign key to Users(UserID), nullable |
| TableName | NVARCHAR(100) | Name of the table that was modified |
| Action | NVARCHAR(50) | Action performed: 'INSERT', 'UPDATE', or 'DELETE' |
| RecordID | INT | ID of the record that was modified |
| FieldName | NVARCHAR(100) | Name of the field that was changed (for UPDATE actions) |
| OldValue | NVARCHAR(MAX) | Previous value (for UPDATE/DELETE actions) |
| NewValue | NVARCHAR(MAX) | New value (for INSERT/UPDATE actions) |
| Timestamp | DATETIME | When the change occurred |

### Stored Procedures
The database includes 31 stored procedures for operations such as user authentication, game management, library operations, search functionality, statistics, and audit log retrieval.

### Triggers
- **trg_Users_AuditLog**: Automatically logs all changes to user accounts
- **trg_Games_AuditLog**: Automatically logs all changes to game records
- **trg_UserGames_AuditLog**: Automatically logs all changes to user library entries
- **trg_UpdateLastModified**: Updates LastModified timestamp on UserGames updates

### Indexes
Non-clustered indexes are created on frequently queried columns including email, roles, game titles, user library status, ratings, and audit log timestamps for optimal query performance.

## Setup

### Prerequisites
- .NET 8.0 SDK
- SQL Server (local or remote instance)
- Visual Studio, Rider, or another .NET IDE

### Database Setup
1. Execute the database scripts in order from the `DatabaseScripts` folder:
   - `00_DropDatabase.sql` (optional, drops existing database)
   - `01_CreateDatabase.sql`
   - `02_CreateTables.sql`
   - `03_CreateIndexes.sql`
   - `04_CreateStoredProcedures.sql`
   - `05_CreateTriggers.sql`
   - `06_SeedData.sql` (optional, adds sample data)

### Configuration
1. Create a `.env` file with your database connection string:
   ```
   DB_CONNECTION_STRING=Server=localhost,1433;Database=GameLibraryDB;User Id=sa;Password=YourPassword;Integrated Security=false;TrustServerCertificate=true;
   ```

   Alternatively, the default connection string can be modified in `DatabaseService.cs`.

2. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

## Default Test Accounts

The seed data includes test accounts:
- **Admin**: `admin@test.com` / `Admin123`
- **User**: `user@test.com` / `User123`

Note: Password hashes are stored in the database. Refer to the seed data script for the actual hashed values.

## Project Structure

```
GameLibraryManager/
├── Models/          # Data models (Game, User, UserGame, etc.)
├── ViewModels/      # MVVM view models with ReactiveUI
├── Views/           # Avalonia UI views (XAML)
├── Services/        # Application services and data access
├── Controls/        # Reusable UI components
└── Converters/      # Value converters for data binding

DatabaseScripts/     # SQL scripts for database setup
```

## Key Components

- **Models**: Data models representing entities (Game, User, UserGame, Genre, Platform, AuditLogEntry, UserStatistics)
- **ViewModels**: MVVM view models implementing presentation logic and data binding (LibraryViewModel, GameManagementViewModel, UserManagementViewModel, etc.)
- **Views**: Avalonia XAML views defining the user interface (LibraryView, GameDetailsView, LoginView, etc.)
- **Services**: Application services layer (AuthenticationService, DatabaseService, SessionManager, NotificationService)
- **Controls**: Reusable UI components (GameCard, FilterPanel, AsyncImage, LoadingSpinner, NotificationToast)
- **Converters**: Value converters for data binding transformations (ImageUrlConverter, NotificationConverters)