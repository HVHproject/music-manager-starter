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
            IQueryable<Data.Models.Song> query = _context.Songs.AsNoTracking();

            // Text search
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var q = request.Query.ToLower();
                query = query.Where(s =>
                    s.Title.ToLower().Contains(q) ||
                    s.Artist.ToLower().Contains(q) ||
                    s.Album.ToLower().Contains(q));
            }

            // Filters
            if (!string.IsNullOrWhiteSpace(request.Genre))
                query = query.Where(s => s.Genre == request.Genre);

            if (request.MinYear.HasValue)
                query = query.Where(s => s.YearReleased >= request.MinYear);

            if (request.MaxYear.HasValue)
                query = query.Where(s => s.YearReleased <= request.MaxYear);

            // Keyset pagination
            if (request.Cursor.HasValue)
                query = query.Where(s => s.Id.CompareTo(request.Cursor.Value) > 0);

            query = query
                .OrderBy(s => s.Id)
                .Take(request.PageSize + 1);

            var songs = await query.ToListAsync();

            var songIds = songs.Select(s => s.Id).ToList();

            var ratings = await _context.Ratings
                .AsNoTracking()
                .Where(r => songIds.Contains(r.SongId))
                .ToListAsync();

            var results = songs
                .Take(request.PageSize)
                .Select(s =>
                {
                    var songRatings = ratings.Where(r => r.SongId == s.Id).ToList();
                    var total = songRatings.Count;
                    var avg = total > 0 ? (double?)songRatings.Average(r => r.Value) : null;
                    var userRating = !string.IsNullOrEmpty(userId)
                        ? songRatings.FirstOrDefault(r => r.UserId == userId)?.Value
                        : null;

                    return new Shared.Song
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        Album = s.Album,
                        Genre = s.Genre,
                        YearReleased = s.YearReleased,
                        AverageRating = avg,
                        UserRating = (double?)userRating,
                        TotalRatings = total
                    };
                })
                .ToList();

            return new SongSearchResponse
            {
                Results = results,
                NextCursor = songs.Count > request.PageSize
                    ? songs.Last().Id
                    : null
            };
        }

    }
}