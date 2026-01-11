using music_manager_starter.Shared;

namespace music_manager_starter.Server.Services
{
    /// <summary>
    /// Service for managing song operations
    /// </summary>
    public interface ISongService
    {
        /// <summary>
        /// Retrieves a list of songs
        /// </summary>
        /// <param name="userId">Optional user ID for filtering songs (currently not used)</param>
        /// <returns>A list of song objects</returns>
        Task<List<Song>> GetSongsAsync(string? userId);

        /// <summary>
        /// Adds a new song to the database
        /// </summary>
        /// <param name="song">The song object to add</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task AddSongAsync(Song song);

        Task<SongSearchResponse> SearchSongsAsync(
            SongSearchRequest request,
            string? userId
        );
    }
}