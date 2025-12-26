using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GameLibraryManager.Models;

namespace GameLibraryManager.Services
{
    public class AuthenticationService
    {
        private readonly DatabaseService _dbService;

        public AuthenticationService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }


        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty", nameof(email));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty", nameof(password));
            }

            var passwordHash = HashPassword(password ?? string.Empty);
            return await _dbService.AuthenticateUserAsync(email, passwordHash);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string username)
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                return (false, "Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                return (false, "Password must be at least 6 characters long");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                return (false, "Username is required");
            }

            var passwordHash = HashPassword(password);
            return await _dbService.RegisterUserAsync(email, passwordHash, username);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

