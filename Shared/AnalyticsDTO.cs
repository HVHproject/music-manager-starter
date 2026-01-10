namespace music_manager_starter.Shared
{
    public class MostRatedSongDto
    {
        public Guid SongId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public DateTimeOffset? LastRated { get; set; }
    }


    public class RatingTrendDto
    {
        public DateTime Date { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int TotalSongsRated { get; set; }
    }

    public class GenrePopularityDto
    {
        public string Genre { get; set; } = string.Empty;
        public int TotalSongs { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public double PopularityScore => TotalRatings * AverageRating;
    }

    public class AnalyticsSummaryDto
    {
        public int TotalSongs { get; set; }
        public int TotalRatings { get; set; }
        public int TotalUsers { get; set; }
        public double OverallAverageRating { get; set; }
        public DateTime? MostActiveDay { get; set; }
    }
}