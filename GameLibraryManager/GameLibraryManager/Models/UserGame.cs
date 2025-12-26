using System;
using System.Collections.Generic;

namespace GameLibraryManager.Models
{
    public class UserGame
    {
        public int UserGameID { get; set; }
        public int UserID { get; set; }
        public int GameID { get; set; }
        public string Status { get; set; } = "Backlog";
        public decimal? PersonalRating { get; set; }
        public string? PersonalNotes { get; set; }
        public decimal? HoursPlayed { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime LastModified { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Description { get; set; }
        public string? CoverImageURL { get; set; }
        
        public List<Genre> Genres { get; set; } = new();
        public List<Platform> Platforms { get; set; } = new();

        public string GenresString => string.Join(", ", System.Linq.Enumerable.Select(Genres, g => g.GenreName));
    }
}

