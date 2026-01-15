using music_manager_starter.Data.Models;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Services
{
    /// <summary>
    /// Service for managing user playlists and their operations
    /// </summary>
    public interface IPlaylistService
    {
        /// <summary>
        /// Creates a new playlist for a user
        /// </summary>
        /// <param name="name">Name of the playlist to create</param>
        /// <param name="userId">ID of the user creating the playlist</param>
        /// <returns>GUID of the newly created playlist</returns>
        Task<Guid> CreatePlaylistAsync(string name, string userId);

        /// <summary>
        /// Gets all playlists for a specific user
        /// </summary>
        /// <param name="userId">ID of the user whose playlists to retrieve</param>
        /// <returns>Read-only list of the user's playlists</returns>
        Task<IReadOnlyList<PlaylistDto>> GetAllPlaylistsAsync(string userId);

        /// <summary>
        /// Gets a specific playlist by its ID
        /// </summary>
        /// <param name="playlistId">ID of the playlist to retrieve</param>
        /// <param name="userId">ID of the user requesting the playlist</param>
        /// <returns>The requested playlist, or null if not found or not authorized</returns>
        Task<PlaylistDto> GetPlaylistAsync(Guid playlistId, string userId);

        /// <summary>
        /// Deletes a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to delete</param>
        /// <param name="userId">ID of the user requesting deletion</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task DeletePlaylistAsync(Guid playlistId, string userId);

        /// <summary>
        /// Renames an existing playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to rename</param>
        /// <param name="name">New name for the playlist</param>
        /// <param name="userId">ID of the user requesting the rename</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RenamePlaylistAsync(Guid playlistId, string name, string userId);

        /// <summary>
        /// Adds songs to a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to add songs to</param>
        /// <param name="songIds">Collection of song IDs to add to the playlist</param>
        /// <param name="userId">ID of the user requesting the addition</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task AddSongsAsync(
            Guid playlistId,
            IReadOnlyCollection<Guid> songIds,
            string userId);

        /// <summary>
        /// Removes songs from a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to remove songs from</param>
        /// <param name="songIds">Collection of song IDs to remove from the playlist</param>
        /// <param name="userId">ID of the user requesting the removal</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RemoveSongsAsync(Guid playlistId,
            IReadOnlyCollection<Guid> songIds,
            string userId);

        /// <summary>
        /// Reorders songs in a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to reorder</param>
        /// <param name="orderedSongIds">List of song IDs in the new desired order</param>
        /// <param name="userId">ID of the user requesting the reorder</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ReorderSongsAsync(
            Guid playlistId,
            IReadOnlyList<Guid> orderedSongIds,
            string userId);

        /// <summary>
        /// Exports a playlist in the specified format
        /// </summary>
        /// <param name="playlistId">ID of the playlist to export</param>
        /// <param name="format">Export format (e.g., "json", "xml", "csv")</param>
        /// <param name="userId">ID of the user requesting the export</param>
        /// <returns>Exported playlist data as a string in the requested format</returns>
        Task<string> ExportAsync(
            Guid playlistId,
            string format,
            string userId);
    }
}