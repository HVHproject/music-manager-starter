using Microsoft.AspNetCore.Mvc;
using music_manager_starter.Server.Services;
using System.Security.Claims;

namespace music_manager_starter.Server.Controllers;

/// <summary>
/// Controller for managing song ratings
/// </summary>
[ApiController]
[Route("api/ratings")]
public sealed class RatingsController : ControllerBase
{
    private readonly IRatingService _ratings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RatingsController"/> class
    /// </summary>
    /// <param name="ratings">The rating service</param>
    public RatingsController(IRatingService ratings)
    {
        _ratings = ratings;
    }

    /// <summary>
    /// Rates a song
    /// </summary>
    /// <param name="songId">The ID of the song to rate</param>
    /// <param name="value">The rating value (0-5 in 0.5 increments)</param>
    /// <returns>HTTP 200 OK if successful</returns>
    /// <response code="200">Rating saved successfully</response>
    /// <response code="400">Invalid rating value</response>
    [HttpPost("{songId}")]
    public async Task<IActionResult> Rate(Guid songId, [FromBody] double value)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _ratings.RateSongAsync(songId, value, userId);
        return Ok();
    }

    /// <summary>
    /// Gets a summary of ratings for a song
    /// </summary>
    /// <param name="songId">The ID of the song to get ratings for</param>
    /// <returns>Rating summary including average, distribution, and user's rating</returns>
    /// <response code="200">Returns the rating summary</response>
    [HttpGet("{songId}")]
    public async Task<IActionResult> Summary(Guid songId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _ratings.GetSummaryAsync(songId, userId));
    }
}
