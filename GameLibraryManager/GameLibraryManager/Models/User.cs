using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GameLibraryManager.Models
{
    public class User : ReactiveObject
    {
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        
        [Reactive] public string Username { get; set; } = string.Empty;
        [Reactive] public string? ProfileImageURL { get; set; }
        [Reactive] public string Role { get; set; } = "User";
        
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        
        public bool IsAdmin => Role == "Admin";
    }
}
