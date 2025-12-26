using System;

namespace GameLibraryManager.Models
{
    public class AuditLogEntry
    {
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int RecordID { get; set; }
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

