namespace music_manager_starter.Server.Services
{
    /// <summary>
    /// Service for providing analytics data about songs and ratings
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Gets the most rated songs
        /// </summary>
        /// <param name="topN">Number of songs to return (default: 10)</param>
        /// <returns>List of most rated songs with their statistics</returns>
        Task<List<MostRatedSongDto>> GetMostRatedSongsAsync(int topN = 10);

        /// <summary>
        /// Gets rating trends over time
        /// </summary>
        /// <param name="startDate">Start date for the trend analysis</param>
        /// <param name="endDate">End date for the trend analysis</param>
        /// <returns>List of rating trends by date</returns>
        Task<List<RatingTrendDto>> GetRatingTrendsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets genre popularity based on ratings
        /// </summary>
        /// <returns>List of genres with their popularity scores</returns>
        Task<List<GenrePopularityDto>> GetGenrePopularityAsync();

        /// <summary>
        /// Gets an overall analytics summary
        /// </summary>
        /// <returns>Summary of all analytics data</returns>
        Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync();
    }
}