using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Controllers
{
    /// <summary>
    /// API controller for managing song operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;

        /// <summary>
        /// Initializes a new instance of the SongsController class
        /// </summary>
        /// <param name="songService">The song service for handling business logic</param>
        public SongsController(ISongService songService)
        {
            _songService = songService;
        }

        /// <summary>
        /// Retrieves all songs from the system
        /// </summary>
        /// <returns>
        /// HTTP 200 OK with a list of songs if successful
        /// </returns>
        /// <remarks>
        /// This endpoint extracts the user ID from the authentication token and passes it to the song service
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult<List<Song>>> GetSongs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var songs = await _songService.GetSongsAsync(userId);
            return Ok(songs);
        }

        /// <summary>
        /// Adds a new song to the system
        /// </summary>
        /// <param name="song">The song object to be added</param>
        /// <returns>
        /// HTTP 200 OK if the song was successfully added,
        /// HTTP 400 Bad Request if the song object is null
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> PostSong(Song song)
        {
            if (song == null)
                return BadRequest();

            await _songService.AddSongAsync(song);
            return Ok();
        }

        /// <summary>
        /// Searches for songs using various criteria including text search, filters, and rating requirements
        /// </summary>
        /// <param name="request">The search request containing query parameters, filters, and pagination settings</param>
        /// <returns>
        /// HTTP 200 OK with a SongSearchResponse containing matching songs and pagination cursor if successful
        /// </returns>
        /// <remarks>
        /// This endpoint extracts the user ID from the authentication token to include user-specific ratings in the results
        /// </remarks>
        [HttpGet("search")]
        public async Task<ActionResult<SongSearchResponse>> SearchSongs(
    [FromQuery] SongSearchRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _songService.SearchSongsAsync(request, userId);
            return Ok(result);
        }

    }
}