using Microsoft.AspNetCore.Mvc;
using music_manager_starter.Data.Models;
using music_manager_starter.Server.Services;

namespace music_manager_starter.Server.Controllers
{
    [ApiController]
    [Route("api/playlists")]
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] string name)
        {
            var userId = User.Identity!.Name!;
            var id = await _playlistService.CreatePlaylistAsync(name, userId);
            return Ok(id);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Playlist>>> GetAll()
        {
            var userId = User.Identity!.Name!;
            var playlists = await _playlistService.GetAllPlaylistsAsync(userId);
            return Ok(playlists);
        }

        [HttpGet("{playlistId:guid}")]
        public async Task<ActionResult<Playlist>> Get(Guid playlistId)
        {
            var userId = User.Identity!.Name!;
            var playlist = await _playlistService.GetPlaylistAsync(playlistId, userId);
            return Ok(playlist);
        }

        [HttpPut("{playlistId:guid}/rename")]
        public async Task<IActionResult> Rename(Guid playlistId, [FromBody] string name)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.RenamePlaylistAsync(playlistId, name, userId);
            return NoContent();
        }

        [HttpDelete("{playlistId:guid}")]
        public async Task<IActionResult> Delete(Guid playlistId)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.DeletePlaylistAsync(playlistId, userId);
            return NoContent();
        }

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

        [HttpDelete("{playlistId:guid}/songs")]
        public async Task<IActionResult> RemoveSongs(
            Guid playlistId,
            [FromBody] IReadOnlyCollection<Guid> songIds)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.RemoveSongsAsync(playlistId, songIds, userId);
            return NoContent();
        }

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

        [HttpPost("{playlistId:guid}/undo")]
        public async Task<IActionResult> Undo(Guid playlistId)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.UndoAsync(playlistId, userId);
            return NoContent();
        }

        [HttpPost("{playlistId:guid}/redo")]
        public async Task<IActionResult> Redo(Guid playlistId)
        {
            var userId = User.Identity!.Name!;
            await _playlistService.RedoAsync(playlistId, userId);
            return NoContent();
        }

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
