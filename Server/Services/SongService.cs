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
    }
}