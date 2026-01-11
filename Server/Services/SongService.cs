using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Shared;
using System.Security.Claims;

namespace music_manager_starter.Server.Services
{
    /// <summary>
    /// Implementation of the song service for managing song data
    /// </summary>
    public sealed class SongService : ISongService
    {
        private readonly DataDbContext _context;

        /// <summary>
        /// Initializes a new instance of the SongService class
        /// </summary>
        /// <param name="context">The database context for data access</param>
        public SongService(DataDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of songs from the database
        /// </summary>
        /// <param name="userId">Optional user ID for filtering songs (currently not used)</param>
        /// <returns>A list of song objects without tracking for read-only operations</returns>
        public async Task<List<Shared.Song>> GetSongsAsync(string? userId)
        {
            // This is essentially what your old controller was doing
            var songsList = await _context.Songs
                .AsNoTracking()
                .ToListAsync();

            var ratingsList = await _context.Ratings
                .AsNoTracking()
                .ToListAsync();

            var songs = songsList.Select(s =>
            {
                var songRatings = ratingsList.Where(r => r.SongId == s.Id).ToList();
                var totalRatings = songRatings.Count;
                var average = totalRatings > 0 ? (double)songRatings.Average(r => r.Value) : (double?)null;
                var userRating = !string.IsNullOrEmpty(userId) ?
                    songRatings.FirstOrDefault(r => r.UserId == userId)?.Value : null;

                return new Shared.Song
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Album = s.Album,
                    Genre = s.Genre,
                    AverageRating = average,
                    UserRating = (double?)userRating,
                    TotalRatings = totalRatings,
                    YearReleased = s.YearReleased
                };
            }).ToList();

            return songs;
        }

        /// <summary>
        /// Adds a new song to the database
        /// </summary>
        /// <param name="song">The song object containing song details to add</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task AddSongAsync(Song song)
        {
            var entity = new Data.Models.Song
            {
                Id = Guid.NewGuid(),
                Title = song.Title,
                Artist = song.Artist,
                Album = song.Album,
                Genre = song.Genre,
                YearReleased = song.YearReleased
            };

            _context.Songs.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<SongSearchResponse> SearchSongsAsync(
     SongSearchRequest request,
     string? userId)
        {
            // Get all song IDs that meet the rating criteria
            List<Guid> ratedSongIds = new();

            if (request.MinRating.HasValue)
            {
                var minRatingDecimal = (decimal)request.MinRating.Value;

                ratedSongIds = await _context.Ratings
                    .AsNoTracking()
                    .GroupBy(r => r.SongId)
                    .Where(g => g.Average(r => (double)r.Value) >= request.MinRating.Value)
                    .Select(g => g.Key)
                    .ToListAsync();
            }

            // Base song query
            IQueryable<Data.Models.Song> songQuery = _context.Songs.AsNoTracking();

            // Text search
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var q = request.Query.ToLower();
                songQuery = songQuery.Where(s =>
                    s.Title.ToLower().Contains(q) ||
                    s.Artist.ToLower().Contains(q) ||
                    s.Album.ToLower().Contains(q));
            }

            // Filters
            if (!string.IsNullOrWhiteSpace(request.Genre))
                songQuery = songQuery.Where(s => s.Genre == request.Genre);

            if (request.MinYear.HasValue)
                songQuery = songQuery.Where(s => s.YearReleased >= request.MinYear);

            if (request.MaxYear.HasValue)
                songQuery = songQuery.Where(s => s.YearReleased <= request.MaxYear);

            if (request.MinRating.HasValue)
            {
                if (ratedSongIds.Any())
                {
                    songQuery = songQuery.Where(s => ratedSongIds.Contains(s.Id));
                }
                else
                {
                    return new SongSearchResponse
                    {
                        Results = new List<Shared.Song>(),
                        NextCursor = null
                    };
                }
            }

            // Keyset pagination
            if (request.Cursor.HasValue)
                songQuery = songQuery.Where(s => s.Id.CompareTo(request.Cursor.Value) > 0);

            var songs = await songQuery
                .OrderBy(s => s.Id)
                .Take(request.PageSize + 1)
                .ToListAsync();

            var songIds = songs.Select(s => s.Id).ToList();

            if (!songIds.Any())
            {
                return new SongSearchResponse
                {
                    Results = new List<Shared.Song>(),
                    NextCursor = null
                };
            }

            // Get all ratings for these songs
            var allRatings = await _context.Ratings
                .AsNoTracking()
                .Where(r => songIds.Contains(r.SongId))
                .ToListAsync();

            // Group ratings by song and calculate averages
            var ratingsBySong = allRatings
                .GroupBy(r => r.SongId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Ratings = g.ToList(),
                        Average = g.Any() ? (double?)g.Average(r => (double)r.Value) : null,
                        Count = g.Count()
                    });

            // Get user specific ratings
            Dictionary<Guid, double?> userRatings = new();
            if (!string.IsNullOrEmpty(userId))
            {
                var userRatingsList = await _context.Ratings
                    .AsNoTracking()
                    .Where(r => songIds.Contains(r.SongId) && r.UserId == userId)
                    .Select(r => new { r.SongId, r.Value })
                    .ToListAsync();

                userRatings = userRatingsList
                    .ToDictionary(r => r.SongId, r => (double?)r.Value);
            }

            // Build results
            var results = songs
                .Take(request.PageSize)
                .Select(s =>
                {
                    Shared.Song result = new()
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Album = s.Album,
                        Genre = s.Genre,
                        YearReleased = s.YearReleased
                    };

                    if (ratingsBySong.TryGetValue(s.Id, out var songRatings))
                    {
                        result.AverageRating = songRatings.Average;
                        result.TotalRatings = songRatings.Count;

                        if (userRatings.TryGetValue(s.Id, out var userRating))
                        {
                            result.UserRating = userRating;
                        }
                        else
                        {
                            result.UserRating = null;
                        }
                    }
                    else
                    {
                        result.AverageRating = null;
                        result.TotalRatings = 0;
                        result.UserRating = null;
                    }

                    return result;
                })
                .ToList();

            // Determine next cursor
            bool hasNextPage = songs.Count > request.PageSize;

            return new SongSearchResponse
            {
                Results = results,
                NextCursor = hasNextPage && results.Any()
                    ? results.Last().Id
                    : null
            };
        }
    }
}