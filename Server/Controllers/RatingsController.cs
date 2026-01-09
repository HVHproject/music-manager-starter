using Microsoft.AspNetCore.Mvc;
using music_manager_starter.Server.Services;
using System.Security.Claims;

namespace music_manager_starter.Server.Controllers;

[ApiController]
[Route("api/ratings")]
public sealed class RatingsController : ControllerBase
{
    private readonly IRatingService _ratings;

    public RatingsController(IRatingService ratings)
    {
        _ratings = ratings;
    }

    [HttpPost("{songId}")]
    public async Task<IActionResult> Rate(Guid songId, [FromBody] double value)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _ratings.RateSongAsync(songId, value, userId);
        return Ok();
    }

    [HttpGet("{songId}")]
    public async Task<IActionResult> Summary(Guid songId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _ratings.GetSummaryAsync(songId, userId));
    }
}
