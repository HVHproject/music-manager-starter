using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Shared;
using System.Linq;

namespace music_manager_starter.Server.Services
{
    /// <summary>
    /// Implementation of <see cref="IAnalyticsService"/> for providing analytics data
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly DataDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsService"/> class
        /// </summary>
        /// <param name="context">The database context for analytics operations</param>
        public AnalyticsService(DataDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<List<MostRatedSongDto>> GetMostRatedSongsAsync(int topN = 10)
        {
            // First, get the ratings data separately to avoid complex queries
            var songs = await _context.Songs.ToListAsync();
            var ratings = await _context.Ratings.ToListAsync();

            var result = songs
                .Select(song =>
                {
                    var songRatings = ratings.Where(r => r.SongId == song.Id).ToList();
                    var lastRating = songRatings.Any()
                        ? songRatings.Max(r => r.CreatedAt)
                        : (DateTimeOffset?)null;

                    return new MostRatedSongDto
                    {
                        SongId = song.Id,
                        Title = song.Title,
                        Artist = song.Artist,
                        Genre = song.Genre ?? "Unknown",
                        AverageRating = songRatings.Any()
                            ? (double)songRatings.Average(r => r.Value) // Cast to double
                            : 0,
                        TotalRatings = songRatings.Count,
                        LastRated = lastRating
                    };
                })
                .Where(s => s.TotalRatings > 0)
                .OrderByDescending(s => s.TotalRatings)
                .ThenByDescending(s => s.AverageRating)
                .Take(topN)
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public async Task<List<RatingTrendDto>> GetRatingTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Default to last 180 days if not specified
            startDate ??= DateTime.UtcNow.AddMonths(-6);
            endDate ??= DateTime.UtcNow;

            // Convert to UTC dates for comparison
            var utcStartDate = startDate.Value.Date.ToUniversalTime();
            var utcEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

            // Get all ratings and filter/group in memory (client evaluation)
            var allRatings = await _context.Ratings.ToListAsync();

            var filteredRatings = allRatings
                .Where(r => r.CreatedAt.UtcDateTime >= utcStartDate && r.CreatedAt.UtcDateTime <= utcEndDate)
                .ToList();

            var trends = filteredRatings
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new RatingTrendDto
                {
                    Date = g.Key,
                    AverageRating = (double)g.Average(r => r.Value),
                    TotalRatings = g.Count(),
                    TotalSongsRated = g.Select(r => r.SongId).Distinct().Count()
                })
                .OrderBy(t => t.Date)
                .ToList();

            return trends;
        }

        /// <inheritdoc/>
        public async Task<List<GenrePopularityDto>> GetGenrePopularityAsync()
        {
            // Get data separately to avoid complex queries
            var songs = await _context.Songs
                .Where(s => s.Genre != null)
                .ToListAsync();

            var ratings = await _context.Ratings.ToListAsync();

            var result = songs
                .GroupBy(s => s.Genre ?? "Unknown")
                .Select(g =>
                {
                    var songIds = g.Select(s => s.Id).ToList();
                    var genreRatings = ratings.Where(r => songIds.Contains(r.SongId)).ToList();

                    var totalRatings = genreRatings.Count;
                    var averageRating = genreRatings.Any()
                        ? (double)genreRatings.Average(r => r.Value)
                        : 0;

                    return new GenrePopularityDto
                    {
                        Genre = g.Key ?? "Unknown",
                        TotalSongs = g.Count(),
                        TotalRatings = totalRatings,
                        AverageRating = averageRating,
                    };
                })
                .Where(g => g.TotalRatings > 0)
                .OrderByDescending(g => g.PopularityScore)
                .ThenByDescending(g => g.TotalRatings)
                .ThenByDescending(g => g.AverageRating)
                .ToList();

            return result;
        }

        /// <inheritdoc/>
        public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync()
        {
            var ratings = await _context.Ratings.ToListAsync();

            var summary = new AnalyticsSummaryDto
            {
                TotalSongs = await _context.Songs.CountAsync(),
                TotalRatings = ratings.Count,
                TotalUsers = ratings.Select(r => r.UserId).Distinct().Count(),
                OverallAverageRating = ratings.Any()
                    ? (double)ratings.Average(r => r.Value) // Cast to double
                    : 0,
                MostActiveDay = ratings.Any()
                    ? ratings
                        .GroupBy(r => r.CreatedAt.Date)
                        .Select(g => new { Date = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Select(x => (DateTime?)x.Date)
                        .FirstOrDefault()
                    : null
            };

            return summary;
        }
    }

    /// <summary>
    /// Data transfer object for most rated songs
    /// </summary>
    public class MostRatedSongDto
    {
        /// <summary>Gets or sets the song identifier</summary>
        public Guid SongId { get; set; }

        /// <summary>Gets or sets the song title</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets the song artist</summary>
        public string Artist { get; set; } = string.Empty;

        /// <summary>Gets or sets the song genre</summary>
        public string Genre { get; set; } = string.Empty;

        /// <summary>Gets or sets the average rating</summary>
        public double AverageRating { get; set; }

        /// <summary>Gets or sets the total number of ratings</summary>
        public int TotalRatings { get; set; }

        /// <summary>Gets or sets the date of the last rating</summary>
        public DateTimeOffset? LastRated { get; set; }
    }

    /// <summary>
    /// Data transfer object for rating trends
    /// </summary>
    public class RatingTrendDto
    {
        /// <summary>Gets or sets the date of the trend data</summary>
        public DateTime Date { get; set; }

        /// <summary>Gets or sets the average rating for the date</summary>
        public double AverageRating { get; set; }

        /// <summary>Gets or sets the total ratings for the date</summary>
        public int TotalRatings { get; set; }

        /// <summary>Gets or sets the total songs rated for the date</summary>
        public int TotalSongsRated { get; set; }
    }

    /// <summary>
    /// Data transfer object for genre popularity
    /// </summary>
    public class GenrePopularityDto
    {
        /// <summary>Gets or sets the genre name</summary>
        public string Genre { get; set; } = string.Empty;

        /// <summary>Gets or sets the total songs in this genre</summary>
        public int TotalSongs { get; set; }

        /// <summary>Gets or sets the total ratings for this genre</summary>
        public int TotalRatings { get; set; }

        /// <summary>Gets or sets the average rating for this genre</summary>
        public double AverageRating { get; set; }

        /// <summary>Gets the popularity score (total ratings × average rating)</summary>
        public double PopularityScore => TotalRatings * AverageRating;
    }

    /// <summary>
    /// Data transfer object for analytics summary
    /// </summary>
    public class AnalyticsSummaryDto
    {
        /// <summary>Gets or sets the total number of songs</summary>
        public int TotalSongs { get; set; }

        /// <summary>Gets or sets the total number of ratings</summary>
        public int TotalRatings { get; set; }

        /// <summary>Gets or sets the total number of unique users who rated</summary>
        public int TotalUsers { get; set; }

        /// <summary>Gets or sets the overall average rating</summary>
        public double OverallAverageRating { get; set; }

        /// <summary>Gets or sets the most active day for ratings</summary>
        public DateTime? MostActiveDay { get; set; }
    }
}