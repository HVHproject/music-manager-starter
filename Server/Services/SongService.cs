using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Shared;

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
        public async Task<List<Song>> GetSongsAsync(string? userId)
        {
            var songs = await _context.Songs
                .AsNoTracking()
                .Select(s => new Song
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    Album = s.Album,
                    Genre = s.Genre,
                })
                .ToListAsync();

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
            };

            _context.Songs.Add(entity);
            await _context.SaveChangesAsync();
        }
    }
}