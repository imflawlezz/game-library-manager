# Game Library Manager

A cross-platform desktop application for managing personal game collections. This is a university project built with Avalonia UI and .NET 8, allowing users to organize, track, and discover games across multiple platforms.

## Features

- User authentication and role-based access control with session validation
- Personal game library with status tracking, ratings, notes, and hours played
- Game catalog management with genres and platforms
- Administrative tools for user and game management
- Reports & Suggestions system for user feedback
- XML export/import functionality for user libraries and global game catalog
- Audit logging with detailed change tracking

## Technology Stack

- **Framework**: .NET 8.0
- **UI Framework**: Avalonia 11.3.9
- **Architecture**: MVVM with ReactiveUI
- **Database**: Microsoft SQL Server with stored procedures and triggers
- **Icons**: FontAwesome (via Projektanker.Icons.Avalonia)

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

#### Reports
User reports and suggestions for feedback and feature requests.

| Column | Type | Description |
|--------|------|-------------|
| ReportID | INT | Primary key, auto-incrementing |
| UserID | INT | Foreign key to Users(UserID) |
| Type | NVARCHAR(20) | Type: 'Report' or 'Suggestion' |
| Title | NVARCHAR(255) | Report/suggestion title |
| Content | NVARCHAR(MAX) | Report/suggestion content |
| Status | NVARCHAR(20) | Status: 'Unreviewed', 'Reviewed', 'Resolved', or 'Dismissed' (default: 'Unreviewed') |
| ReviewedBy | INT | Foreign key to Users(UserID), nullable (admin who reviewed) |
| AdminNotes | NVARCHAR(MAX) | Admin response/notes, nullable |
| CreatedAt | DATETIME | Creation timestamp |
| ReviewedAt | DATETIME | Review timestamp, nullable |

#### AuditLog
Complete audit trail tracking all changes to Users, Games, UserGames, and Reports tables.

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

The database includes stored procedures for:

- **Authentication**: User login, registration, email validation, session validation
- **User Management**: Create, update, delete users, get user statistics
- **Game Management**: CRUD operations for games, genres, and platforms
- **Library Operations**: Add/remove games from library, update library entries, get user library
- **Search & Filtering**: Search games by title, filter by genre/platform/status
- **Statistics**: User library statistics, game statistics
- **Audit Log**: Retrieve audit log entries with filtering
- **Reports Management**: Create, retrieve, update reports and suggestions
- **XML Export/Import**:

### Triggers

- **trg_Users_AuditLog**: Automatically logs all changes to user accounts
- **trg_Games_AuditLog**: Automatically logs all changes to game records
- **trg_UserGames_AuditLog**: Automatically logs all changes to user library entries
- **trg_Reports_AuditLog**: Automatically logs all changes to reports and suggestions
- **trg_UpdateLastModified**: Updates LastModified timestamp on UserGames updates

### Indexes

Non-clustered indexes are created on frequently queried columns including:
- Email, roles, and user IDs
- Game titles and creation metadata
- User library status, ratings, and game IDs
- Report status, type, user ID, and creation dates
- Audit log timestamps, table names, and field names
- Genre and platform relationships

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
1. Create a `.env` file in the project root with your database connection string:
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

- **Models**: Data models representing entities (Game, User, UserGame, Genre, Platform, ReportSuggestion, AuditLogEntry, UserStatistics)
- **ViewModels**: MVVM view models implementing presentation logic and data binding (LibraryViewModel, GameManagementViewModel, UserManagementViewModel, ReportsViewModel, ReportsManagementViewModel, etc.)
- **Views**: Avalonia XAML views defining the user interface (LibraryView, GameDetailsView, LoginView, ReportsView, etc.)
- **Services**: Application services layer (AuthenticationService, DatabaseService, SessionManager, NotificationService)
- **Controls**: Reusable UI components (GameCard, FilterPanel, AsyncImage, LoadingSpinner, NotificationToast)
- **Converters**: Value converters for data binding transformations (ImageUrlConverter, ActionColorConverter, ActionIconConverter, TableNameColorConverter, StatusIconConverter, StatusColorConverter, ConnectionStatusConverters, etc.)