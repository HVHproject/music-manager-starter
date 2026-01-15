using Microsoft.AspNetCore.Mvc;
using music_manager_starter.Data.Models;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Controllers
{
    /// <summary>
    /// Controller for managing playlists via REST API
    /// </summary>
    [ApiController]
    [Route("api/playlists")]
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;

        /// <summary>
        /// Initializes a new instance of the PlaylistController
        /// </summary>
        /// <param name="playlistService">Service for playlist operations</param>
        public PlaylistController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        /// <summary>
        /// Creates a new playlist
        /// </summary>
        /// <param name="name">Name of the playlist to create</param>
        /// <returns>GUID of the newly created playlist</returns>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] string name)
        {
            var userId = User.Identity!.Name!;
            var id = await _playlistService.CreatePlaylistAsync(name, userId);
            return Ok(id);
        }

        /// <summary>
        /// Gets all playlists for the current user
        /// </summary>
        /// <returns>List of the user's playlists</returns>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PlaylistDto>>> GetAll()
        {
            var userId = User.Identity!.Name!;
            var playlists = await _playlistService.GetAllPlaylistsAsync(userId);
            return Ok(playlists);
        }

        /// <summary>
        /// Gets a specific playlist by its ID
        /// </summary>
        /// <param name="playlistId">ID of the playlist to retrieve</param>
        /// <returns>The requested playlist</returns>
        [HttpGet("{playlistId:guid}")]
        public async Task<ActionResult<PlaylistDto>> Get(Guid playlistId)
        {
            var userId = User.Identity!.Name!;
            var playlist = await _playlistService.GetPlaylistAsync(playlistId, userId);
            return Ok(playlist);
        }

        /// <summary>
        /// Renames an existing playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to rename</param>
        /// <param name="name">New name for the playlist</param>
        /// <returns>204 No Content on success</returns>
        [HttpPut("{playlistId:guid}/rename")]
        public async Task<IActionResult> Rename(Guid playlistId, [FromBody] string name)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.RenamePlaylistAsync(playlistId, name, userId);
            return NoContent();
        }

        /// <summary>
        /// Deletes a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to delete</param>
        /// <returns>204 No Content on success</returns>
        [HttpDelete("{playlistId:guid}")]
        public async Task<IActionResult> Delete(Guid playlistId)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.DeletePlaylistAsync(playlistId, userId);
            return NoContent();
        }

        /// <summary>
        /// Adds songs to a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to add songs to</param>
        /// <param name="songIds">Collection of song IDs to add</param>
        /// <returns>204 No Content on success</returns>
        [HttpPost("{playlistId:guid}/songs")]
        public async Task<IActionResult> AddSongs(
            Guid playlistId,
            [FromBody] IReadOnlyCollection<Guid> songIds)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.AddSongsAsync(
                playlistId,
                songIds,
                userId);

            return NoContent();
        }

        /// <summary>
        /// Removes songs from a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to remove songs from</param>
        /// <param name="songIds">Collection of song IDs to remove</param>
        /// <returns>204 No Content on success</returns>
        [HttpDelete("{playlistId:guid}/songs")]
        public async Task<IActionResult> RemoveSongs(
            Guid playlistId,
            [FromBody] IReadOnlyCollection<Guid> songIds)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.RemoveSongsAsync(playlistId, songIds, userId);
            return NoContent();
        }

        /// <summary>
        /// Reorders songs in a playlist
        /// </summary>
        /// <param name="playlistId">ID of the playlist to reorder</param>
        /// <param name="orderedSongIds">List of song IDs in the new desired order</param>
        /// <returns>204 No Content on success</returns>
        [HttpPut("{playlistId:guid}/reorder")]
        public async Task<IActionResult> Reorder(
            Guid playlistId,
            [FromBody] IReadOnlyList<Guid> orderedSongIds)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.ReorderSongsAsync(
                playlistId,
                orderedSongIds,
                userId);

            return NoContent();
        }

        /// <summary>
        /// Exports a playlist in the specified format
        /// </summary>
        /// <param name="playlistId">ID of the playlist to export</param>
        /// <param name="format">Export format (e.g., "json", "xml", "csv")</param>
        /// <returns>Exported playlist data as a string</returns>
        [HttpGet("{playlistId:guid}/export")]
        public async Task<ActionResult<string>> Export(
            Guid playlistId,
            [FromQuery] string format)
        {
            var userId = User.Identity!.Name!;
            var result = await _playlistService.ExportAsync(
                playlistId,
                format,
                userId);

            return Ok(result);
        }
    }
}