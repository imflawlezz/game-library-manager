using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using GameLibraryManager.Models;

namespace GameLibraryManager.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            LoadEnvIfNeeded();

            var envConnection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            _connectionString = !string.IsNullOrWhiteSpace(envConnection)
                ? envConnection!
                : "Server=localhost,1433;Database=GameLibraryDB;User Id=sa;Password=YourPassword;Integrated Security=false;TrustServerCertificate=true;";
        }

        private static bool _envLoaded = false;

        private static void LoadEnvIfNeeded()
        {
            if (_envLoaded)
            {
                return;
            }

            var baseDir = AppContext.BaseDirectory;
            var currentDir = Directory.GetCurrentDirectory();
            var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var executableDir = !string.IsNullOrEmpty(executablePath) ? Path.GetDirectoryName(executablePath) : null;
            
            var appBundleResources = Path.Combine(baseDir, "..", "Resources");
            var appBundleResourcesResolved = Path.GetFullPath(appBundleResources);
            
            var candidatePaths = new[]
            {
                Path.Combine(currentDir, ".env"),
                Path.Combine(baseDir, ".env"),
                executableDir != null ? Path.Combine(executableDir, ".env") : null,
                File.Exists(appBundleResourcesResolved) ? Path.Combine(appBundleResourcesResolved, ".env") : null,
                Path.Combine(baseDir, "..", "..", "..", ".env"),
                Path.Combine(baseDir, "..", "..", "..", "..", ".env"),
                Path.Combine(baseDir, "..", "..", "..", "..", "..", ".env"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".env"),
            };

            foreach (var envPath in candidatePaths)
            {
                if (string.IsNullOrEmpty(envPath) || !File.Exists(envPath))
                {
                    continue;
                }

                try
                {
                    foreach (var line in File.ReadAllLines(envPath))
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                        {
                            continue;
                        }

                        var parsed = ParseEnvLine(line);
                        if (parsed is { Key: { Length: >0 } key, Value: var value })
                        {
                            Environment.SetEnvironmentVariable(key, value);
                        }
                    }

                    _envLoaded = true;
                    break;
                }
                catch
                {
                    continue;
                }
            }

            _envLoaded = true;
        }

        private static (string Key, string Value)? ParseEnvLine(string line)
        {
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                return null;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value[1..^1];
            }

            return (key, value);
        }

        public async Task TestConnectionAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand("sp_TestConnection", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            await command.ExecuteScalarAsync();
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_CheckEmailExists", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Email", email);
                
                var existsParam = new SqlParameter("@Exists", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(existsParam);

                await command.ExecuteNonQueryAsync();
                
                return existsParam.Value != DBNull.Value && (bool)existsParam.Value;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> AuthenticateUserAsync(string email, string passwordHash)
        {
            try
            {
                var userExists = await UserExistsAsync(email);
                
                if (!userExists)
                {
                    return null;
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_AuthenticateUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Email", email.Trim());
                command.Parameters.AddWithValue("@PasswordHash", passwordHash.Trim());
                
                var userIDParam = new SqlParameter("@UserID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var usernameParam = new SqlParameter("@Username", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output };
                var roleParam = new SqlParameter("@Role", SqlDbType.NVarChar, 20) { Direction = ParameterDirection.Output };
                var profileImageParam = new SqlParameter("@ProfileImageURL", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                
                command.Parameters.Add(userIDParam);
                command.Parameters.Add(usernameParam);
                command.Parameters.Add(roleParam);
                command.Parameters.Add(profileImageParam);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                var returnVal = returnValue.Value != DBNull.Value ? (int)returnValue.Value : -1;
                var userID = userIDParam.Value != DBNull.Value && userIDParam.Value != null ? (int)userIDParam.Value : 0;
                
                if (returnVal == 0 && userID > 0)
                {
                    var user = new User
                    {
                        UserID = userID,
                        Email = email,
                        Username = usernameParam.Value?.ToString() ?? string.Empty,
                        Role = roleParam.Value?.ToString() ?? "User",
                        ProfileImageURL = profileImageParam.Value != DBNull.Value ? profileImageParam.Value?.ToString() : null
                    };

                    await UpdateLastLoginAsync(user.UserID);

                    return user;
                }

                return null;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Database error during authentication: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Authentication failed: {ex.Message}", ex);
            }
        }

        public async Task<(bool Success, string Message)> RegisterUserAsync(string email, string passwordHash, string username)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_RegisterUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Username", username);
                
                var userIDParam = new SqlParameter("@UserID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(userIDParam);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                if ((int)returnValue.Value == 0)
                {
                    return (true, "User registered successfully");
                }

                return (false, "Registration failed");
            }
            catch (SqlException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Registration error: {ex.Message}");
            }
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateLastLogin", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
            }
        }

        public async Task<List<Game>> GetAllGamesAsync()
        {
            var games = new List<Game>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetAllGames", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    games.Add(new Game
                    {
                        GameID = reader.GetInt32("GameID"),
                        Title = reader.GetString("Title"),
                        Developer = reader.IsDBNull("Developer") ? null : reader.GetString("Developer"),
                        Publisher = reader.IsDBNull("Publisher") ? null : reader.GetString("Publisher"),
                        ReleaseYear = reader.IsDBNull("ReleaseYear") ? null : reader.GetInt32("ReleaseYear"),
                        Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                        CoverImageURL = reader.IsDBNull("CoverImageURL") ? null : reader.GetString("CoverImageURL"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        CreatedByUsername = reader.IsDBNull("CreatedByUsername") ? null : reader.GetString("CreatedByUsername"),
                        GenresString = reader.IsDBNull("Genres") ? string.Empty : reader.GetString("Genres"),
                        PlatformsString = reader.IsDBNull("Platforms") ? string.Empty : reader.GetString("Platforms")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get games: {ex.Message}", ex);
            }

            return games;
        }

        public async Task<Game?> GetGameByIdAsync(int gameId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetGameByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@GameID", gameId);

                Game? game = null;

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    game = new Game
                    {
                        GameID = reader.GetInt32("GameID"),
                        Title = reader.GetString("Title"),
                        Developer = reader.IsDBNull("Developer") ? null : reader.GetString("Developer"),
                        Publisher = reader.IsDBNull("Publisher") ? null : reader.GetString("Publisher"),
                        ReleaseYear = reader.IsDBNull("ReleaseYear") ? null : reader.GetInt32("ReleaseYear"),
                        Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                        CoverImageURL = reader.IsDBNull("CoverImageURL") ? null : reader.GetString("CoverImageURL"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        CreatedBy = reader.GetInt32("CreatedBy"),
                        CreatedByUsername = reader.IsDBNull("CreatedByUsername") ? null : reader.GetString("CreatedByUsername")
                    };
                }

                if (game != null)
                {
                    if (await reader.NextResultAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            game.Genres.Add(new Genre
                            {
                                GenreID = reader.GetInt32("GenreID"),
                                GenreName = reader.GetString("GenreName")
                            });
                        }
                    }

                    if (await reader.NextResultAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            game.Platforms.Add(new Platform
                            {
                                PlatformID = reader.GetInt32("PlatformID"),
                                PlatformName = reader.GetString("PlatformName")
                            });
                        }
                    }
                }

                return game;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get game: {ex.Message}", ex);
            }
        }

        public async Task<List<UserGame>> GetUserLibraryAsync(int userId, string? status = null, int? genreId = null, int? platformId = null)
        {
            var userGames = new List<UserGame>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetUserLibrary", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);
                if (status != null) command.Parameters.AddWithValue("@Status", status);
                if (genreId.HasValue) command.Parameters.AddWithValue("@GenreID", genreId.Value);
                if (platformId.HasValue) command.Parameters.AddWithValue("@PlatformID", platformId.Value);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    userGames.Add(new UserGame
                    {
                        UserGameID = reader.GetInt32("UserGameID"),
                        UserID = reader.GetInt32("UserID"),
                        GameID = reader.GetInt32("GameID"),
                        Status = reader.GetString("Status"),
                        PersonalRating = reader.IsDBNull("PersonalRating") ? null : reader.GetDecimal("PersonalRating"),
                        PersonalNotes = reader.IsDBNull("PersonalNotes") ? null : reader.GetString("PersonalNotes"),
                        HoursPlayed = reader.IsDBNull("HoursPlayed") ? null : reader.GetDecimal("HoursPlayed"),
                        DateAdded = reader.GetDateTime("DateAdded"),
                        LastModified = reader.GetDateTime("LastModified"),
                        Title = reader.GetString("Title"),
                        Developer = reader.IsDBNull("Developer") ? null : reader.GetString("Developer"),
                        Publisher = reader.IsDBNull("Publisher") ? null : reader.GetString("Publisher"),
                        ReleaseYear = reader.IsDBNull("ReleaseYear") ? null : reader.GetInt32("ReleaseYear"),
                        Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                        CoverImageURL = reader.IsDBNull("CoverImageURL") ? null : reader.GetString("CoverImageURL")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user library: {ex.Message}", ex);
            }

            return userGames;
        }

        public async Task<UserGame?> GetUserGameDetailsAsync(int userId, int gameId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetUserGameDetails", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@GameID", gameId);

                UserGame? userGame = null;

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userGame = new UserGame
                    {
                        UserGameID = reader.GetInt32("UserGameID"),
                        UserID = reader.GetInt32("UserID"),
                        GameID = reader.GetInt32("GameID"),
                        Status = reader.GetString("Status"),
                        PersonalRating = reader.IsDBNull("PersonalRating") ? null : reader.GetDecimal("PersonalRating"),
                        PersonalNotes = reader.IsDBNull("PersonalNotes") ? null : reader.GetString("PersonalNotes"),
                        HoursPlayed = reader.IsDBNull("HoursPlayed") ? null : reader.GetDecimal("HoursPlayed"),
                        DateAdded = reader.GetDateTime("DateAdded"),
                        LastModified = reader.GetDateTime("LastModified"),
                        Title = reader.GetString("Title"),
                        Developer = reader.IsDBNull("Developer") ? null : reader.GetString("Developer"),
                        Publisher = reader.IsDBNull("Publisher") ? null : reader.GetString("Publisher"),
                        ReleaseYear = reader.IsDBNull("ReleaseYear") ? null : reader.GetInt32("ReleaseYear"),
                        Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                        CoverImageURL = reader.IsDBNull("CoverImageURL") ? null : reader.GetString("CoverImageURL")
                    };
                }

                if (userGame != null)
                {
                    if (await reader.NextResultAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            userGame.Genres.Add(new Genre
                            {
                                GenreID = reader.GetInt32("GenreID"),
                                GenreName = reader.GetString("GenreName")
                            });
                        }
                    }

                    if (await reader.NextResultAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            userGame.Platforms.Add(new Platform
                            {
                                PlatformID = reader.GetInt32("PlatformID"),
                                PlatformName = reader.GetString("PlatformName")
                            });
                        }
                    }
                }

                return userGame;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user game details: {ex.Message}", ex);
            }
        }

        public async Task<bool> AddGameToLibraryAsync(int userId, int gameId, string status = "Backlog")
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_AddGameToLibrary", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@GameID", gameId);
                command.Parameters.AddWithValue("@Status", status);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                return (int)returnValue.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> UpdateUserGameAsync(UserGame userGame)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateUserGame", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserGameID", userGame.UserGameID);
                command.Parameters.AddWithValue("@Status", userGame.Status ?? "Backlog");
                
                var ratingParam = new SqlParameter("@PersonalRating", SqlDbType.Decimal);
                ratingParam.Precision = 3;
                ratingParam.Scale = 1;
                if (userGame.PersonalRating.HasValue)
                {
                    var rating = Math.Round(Math.Min(Math.Max(userGame.PersonalRating.Value, 0m), 10m), 1);
                    ratingParam.Value = rating;
                }
                else
                {
                    ratingParam.Value = DBNull.Value;
                }
                command.Parameters.Add(ratingParam);
                
                command.Parameters.AddWithValue("@PersonalNotes", userGame.PersonalNotes ?? (object)DBNull.Value);
                
                var hoursParam = new SqlParameter("@HoursPlayed", SqlDbType.Decimal);
                hoursParam.Precision = 10;
                hoursParam.Scale = 2;
                if (userGame.HoursPlayed.HasValue)
                {
                    var hours = Math.Round(Math.Min(Math.Max(userGame.HoursPlayed.Value, 0m), 99999999.99m), 2);
                    hoursParam.Value = hours;
                }
                else
                {
                    hoursParam.Value = DBNull.Value;
                }
                command.Parameters.Add(hoursParam);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) 
                { 
                    Direction = ParameterDirection.ReturnValue 
                };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                return (int)returnValue.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> RemoveGameFromLibraryAsync(int userId, int gameId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_RemoveGameFromLibrary", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@GameID", gameId);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                return (int)returnValue.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<int> AddGameAsync(Game game, List<int> genreIds, List<int> platformIds)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertGame", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Title", game.Title);
                command.Parameters.AddWithValue("@Developer", game.Developer ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Publisher", game.Publisher ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ReleaseYear", game.ReleaseYear ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Description", game.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CoverImageURL", game.CoverImageURL ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedBy", game.CreatedBy);
                command.Parameters.AddWithValue("@GenreIDs", string.Join(",", genreIds));
                command.Parameters.AddWithValue("@PlatformIDs", string.Join(",", platformIds));

                var gameIDParam = new SqlParameter("@GameID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(gameIDParam);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                if ((int)returnValue.Value == 0)
                {
                    return (int)gameIDParam.Value;
                }

                throw new Exception("Failed to add game");
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> UpdateGameAsync(Game game, List<int> genreIds, List<int> platformIds)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateGame", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@GameID", game.GameID);
                command.Parameters.AddWithValue("@Title", game.Title);
                command.Parameters.AddWithValue("@Developer", game.Developer ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Publisher", game.Publisher ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ReleaseYear", game.ReleaseYear ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Description", game.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CoverImageURL", game.CoverImageURL ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GenreIDs", string.Join(",", genreIds));
                command.Parameters.AddWithValue("@PlatformIDs", string.Join(",", platformIds));

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                return (int)returnValue.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> DeleteGameAsync(int gameId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteGame", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@GameID", gameId);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                return (int)returnValue.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<Genre>> GetAllGenresAsync()
        {
            var genres = new List<Genre>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetAllGenres", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    genres.Add(new Genre
                    {
                        GenreID = reader.GetInt32("GenreID"),
                        GenreName = reader.GetString("GenreName")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get genres: {ex.Message}", ex);
            }

            return genres;
        }

        public async Task<List<Platform>> GetAllPlatformsAsync()
        {
            var platforms = new List<Platform>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetAllPlatforms", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    platforms.Add(new Platform
                    {
                        PlatformID = reader.GetInt32("PlatformID"),
                        PlatformName = reader.GetString("PlatformName")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get platforms: {ex.Message}", ex);
            }

            return platforms;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetAllUsers", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        UserID = reader.GetInt32("UserID"),
                        Email = reader.GetString("Email"),
                        Username = reader.GetString("Username"),
                        ProfileImageURL = reader.IsDBNull("ProfileImageURL") ? null : reader.GetString("ProfileImageURL"),
                        Role = reader.GetString("Role"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        LastLogin = reader.IsDBNull("LastLogin") ? null : reader.GetDateTime("LastLogin")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get users: {ex.Message}", ex);
            }

            return users;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string role)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateUserRole", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@Role", role);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                return (int)returnValue.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, string username, string? profileImageUrl = null, string? newPasswordHash = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateUserProfile", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@ProfileImageURL", profileImageUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash ?? (object)DBNull.Value);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                return (int)returnValue.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_DeleteUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@UserID", userId);

                var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                return (int)returnValue.Value == 0;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<Game>> SearchGamesAsync(string searchTerm)
        {
            var games = new List<Game>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_SearchGames", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@SearchTerm", searchTerm);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    games.Add(new Game
                    {
                        GameID = reader.GetInt32("GameID"),
                        Title = reader.GetString("Title"),
                        Developer = reader.IsDBNull("Developer") ? null : reader.GetString("Developer"),
                        Publisher = reader.IsDBNull("Publisher") ? null : reader.GetString("Publisher"),
                        ReleaseYear = reader.IsDBNull("ReleaseYear") ? null : reader.GetInt32("ReleaseYear"),
                        Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                        CoverImageURL = reader.IsDBNull("CoverImageURL") ? null : reader.GetString("CoverImageURL")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to search games: {ex.Message}", ex);
            }

            return games;
        }

        public async Task<UserStatistics?> GetUserStatisticsAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetUserStatistics", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new UserStatistics
                    {
                        TotalGames = reader.GetInt32("TotalGames"),
                        PlayingCount = reader.GetInt32("PlayingCount"),
                        CompletedCount = reader.GetInt32("CompletedCount"),
                        BacklogCount = reader.GetInt32("BacklogCount"),
                        WishlistCount = reader.GetInt32("WishlistCount"),
                        DroppedCount = reader.GetInt32("DroppedCount"),
                        AverageRating = reader.IsDBNull("AverageRating") ? null : reader.GetDecimal("AverageRating"),
                        TotalHoursPlayed = reader.IsDBNull("TotalHoursPlayed") ? null : reader.GetDecimal("TotalHoursPlayed")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user statistics: {ex.Message}", ex);
            }
        }

        public async Task<List<AuditLogEntry>> GetAuditLogAsync(DateTime? startDate = null, DateTime? endDate = null, string? tableName = null, string? action = null, int? userId = null, int limit = 1000)
        {
            var entries = new List<AuditLogEntry>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetAuditLog", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (startDate.HasValue)
                    command.Parameters.AddWithValue("@StartDate", startDate.Value);
                if (endDate.HasValue)
                    command.Parameters.AddWithValue("@EndDate", endDate.Value);
                if (!string.IsNullOrEmpty(tableName))
                    command.Parameters.AddWithValue("@TableName", tableName);
                if (!string.IsNullOrEmpty(action))
                    command.Parameters.AddWithValue("@Action", action);
                if (userId.HasValue)
                    command.Parameters.AddWithValue("@UserID", userId.Value);
                command.Parameters.AddWithValue("@Limit", limit);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    entries.Add(new AuditLogEntry
                    {
                        LogID = reader.GetInt32("LogID"),
                        UserID = reader.IsDBNull("UserID") ? null : reader.GetInt32("UserID"),
                        Username = reader.IsDBNull("Username") ? null : reader.GetString("Username"),
                        Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                        TableName = reader.GetString("TableName"),
                        Action = reader.GetString("Action"),
                        RecordID = reader.GetInt32("RecordID"),
                        FieldName = reader.IsDBNull("FieldName") ? null : reader.GetString("FieldName"),
                        OldValue = reader.IsDBNull("OldValue") ? null : reader.GetString("OldValue"),
                        NewValue = reader.IsDBNull("NewValue") ? null : reader.GetString("NewValue"),
                        Timestamp = reader.GetDateTime("Timestamp")
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get audit log: {ex.Message}", ex);
            }

            return entries;
        }
    }
}

