namespace GameLibraryManager.Models
{
    public class UserStatistics
    {
        public int TotalGames { get; set; }
        public int PlayingCount { get; set; }
        public int CompletedCount { get; set; }
        public int BacklogCount { get; set; }
        public int WishlistCount { get; set; }
        public int DroppedCount { get; set; }
        public decimal? AverageRating { get; set; }
        public decimal? TotalHoursPlayed { get; set; }
    }
}

