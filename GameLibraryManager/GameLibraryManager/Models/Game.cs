using System;
using System.Collections.Generic;

namespace GameLibraryManager.Models
{
    public class Game
    {
        public int GameID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Description { get; set; }
        public string? CoverImageURL { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByUsername { get; set; }
        
        public List<Genre> Genres { get; set; } = new();
        public List<Platform> Platforms { get; set; } = new();
        
        public string GenresString { get; set; } = string.Empty;
        public string PlatformsString { get; set; } = string.Empty;
    }
}

