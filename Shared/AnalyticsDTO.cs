namespace music_manager_starter.Shared
{
    /// <summary>
    /// Data transfer object for most rated songs
    /// </summary>
    public class MostRatedSongDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the song
        /// </summary>
        public Guid SongId { get; set; }

        /// <summary>
        /// Gets or sets the title of the song
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the artist who performs the song
        /// </summary>
        public string Artist { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the musical genre of the song
        /// </summary>
        public string Genre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the average rating of the song (0-5 scale)
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Gets or sets the total number of ratings the song has received
        /// </summary>
        public int TotalRatings { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the song was last rated
        /// </summary>
        public DateTimeOffset? LastRated { get; set; }
    }

    /// <summary>
    /// Data transfer object for rating trends over time
    /// </summary>
    public class RatingTrendDto
    {
        /// <summary>
        /// Gets or sets the date of the trend data point
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the average rating across all songs for this date
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Gets or sets the total number of ratings submitted on this date
        /// </summary>
        public int TotalRatings { get; set; }

        /// <summary>
        /// Gets or sets the number of unique songs rated on this date
        /// </summary>
        public int TotalSongsRated { get; set; }
    }

    /// <summary>
    /// Data transfer object for genre popularity analysis
    /// </summary>
    public class GenrePopularityDto
    {
        /// <summary>
        /// Gets or sets the name of the musical genre
        /// </summary>
        public string Genre { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of songs in this genre
        /// </summary>
        public int TotalSongs { get; set; }

        /// <summary>
        /// Gets or sets the total number of ratings received by songs in this genre
        /// </summary>
        public int TotalRatings { get; set; }

        /// <summary>
        /// Gets or sets the average rating of songs in this genre
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Gets the popularity score calculated as (Total Ratings × Average Rating)
        /// </summary>
        public double PopularityScore => TotalRatings * AverageRating;
    }

    /// <summary>
    /// Data transfer object for overall analytics summary
    /// </summary>
    public class AnalyticsSummaryDto
    {
        /// <summary>
        /// Gets or sets the total number of songs in the system
        /// </summary>
        public int TotalSongs { get; set; }

        /// <summary>
        /// Gets or sets the total number of ratings submitted
        /// </summary>
        public int TotalRatings { get; set; }

        /// <summary>
        /// Gets or sets the number of unique users who have submitted ratings
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Gets or sets the overall average rating across all songs
        /// </summary>
        public double OverallAverageRating { get; set; }

        /// <summary>
        /// Gets or sets the date with the highest rating activity
        /// </summary>
        public DateTime? MostActiveDay { get; set; }
    }
}