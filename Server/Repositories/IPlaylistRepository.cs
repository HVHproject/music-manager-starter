using music_manager_starter.Data.Models;

namespace music_manager_starter.Server.Repositories
{
    /// <summary>
    /// Repository for managing playlist data persistence
    /// </summary>
    public interface IPlaylistRepository
    {
        /// <summary>
        /// Gets a playlist by its ID for a specific user
        /// </summary>
        /// <param name="playlistId">ID of the playlist to retrieve</param>
        /// <param name="userId">ID of the user who owns the playlist</param>
        /// <returns>The playlist entity, or null if not found</returns>
        Task<Playlist?> GetByIdAsync(Guid playlistId, string userId);

        /// <summary>
        /// Gets all playlists for a specific user
        /// </summary>
        /// <param name="userId">ID of the user whose playlists to retrieve</param>
        /// <returns>Read-only list of the user's playlists</returns>
        Task<IReadOnlyList<Playlist>> GetAllByUserIdAsync(string userId);

        /// <summary>
        /// Adds a new playlist to the repository
        /// </summary>
        /// <param name="playlist">Playlist entity to add</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task AddAsync(Playlist playlist);

        /// <summary>
        /// Updates an existing playlist in the repository
        /// </summary>
        /// <param name="playlist">Playlist entity with updated information</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdateAsync(Playlist playlist);

        /// <summary>
        /// Deletes a playlist from the repository
        /// </summary>
        /// <param name="playlist">Playlist entity to delete</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task DeleteAsync(Playlist playlist);

        /// <summary>
        /// Removes specific songs from a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to remove songs from</param>
        /// <param name="songIds">Collection of song IDs to remove</param>
        /// <param name="userId">The User updating the playlist</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RemoveSongsAsync(Guid playlistId, IReadOnlyCollection<Guid> songIds, string userId);

        /// <summary>
        /// Gets all songs in a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to retrieve songs from</param>
        /// <returns>Read-only list of playlist-song associations</returns>
        Task<IReadOnlyList<PlaylistSong>> GetSongsAsync(Guid playlistId);

        /// <summary>
        /// Replaces all songs in a playlist with new ones
        /// </summary>
        /// <param name="playlistId">ID of the playlist to update</param>
        /// <param name="songs">New list of playlist-song associations</param>
        /// <param name="userId">The User updating the playlist</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ReplaceSongsAsync(
            Guid playlistId,
            IReadOnlyList<PlaylistSong> songs,
            string userId);

        /// <summary>
        /// Saves all pending changes to the repository
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SaveChangesAsync();
    }
}