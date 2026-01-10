namespace music_manager_starter.Server.Services
{
    public interface IAnalyticsService
    {
        Task<List<MostRatedSongDto>> GetMostRatedSongsAsync(int topN = 10);
        Task<List<RatingTrendDto>> GetRatingTrendsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<GenrePopularityDto>> GetGenrePopularityAsync();
        Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync();
    }
}