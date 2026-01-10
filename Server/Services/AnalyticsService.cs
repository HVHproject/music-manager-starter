using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Shared;
using System.Linq;

namespace music_manager_starter.Server.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly DataDbContext _context;

        public AnalyticsService(DataDbContext context)
        {
            _context = context;
        }

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

        public async Task<List<RatingTrendDto>> GetRatingTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddMonths(-6);
            endDate ??= DateTime.UtcNow;

            // Get ALL ratings first, then filter in memory
            var allRatings = await _context.Ratings.ToListAsync();

            // Filter by date in memory
            var filteredRatings = allRatings
                .Where(r => r.CreatedAt.UtcDateTime >= startDate.Value &&
                           r.CreatedAt.UtcDateTime <= endDate.Value)
                .ToList();

            // Process on client side
            var trends = filteredRatings
                .GroupBy(r => r.CreatedAt.Date) // Use Date property of DateTimeOffset
                .Select(g => new RatingTrendDto
                {
                    Date = g.Key,
                    AverageRating = (double)g.Average(r => r.Value), // Cast to double
                    TotalRatings = g.Count(),
                    TotalSongsRated = g.Select(r => r.SongId).Distinct().Count()
                })
                .OrderBy(t => t.Date)
                .ToList();

            return trends;
        }

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