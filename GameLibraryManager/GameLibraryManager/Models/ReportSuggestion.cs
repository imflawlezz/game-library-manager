using System;

namespace GameLibraryManager.Models
{
    public class ReportSuggestion
    {
        public int ReportID { get; set; }
        public int UserID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = "Unreviewed";
        public int? ReviewedBy { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        
        public string CreatedByUsername { get; set; } = string.Empty;
        public string? CreatedByEmail { get; set; }
        public string? CreatedByProfileImageURL { get; set; }
        public string? ReviewedByUsername { get; set; }
        public string? ReviewedByProfileImageURL { get; set; }
        
        public bool IsUnreviewed => Status == "Unreviewed";
    }
}

