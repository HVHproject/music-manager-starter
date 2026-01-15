using music_manager_starter.Shared;

namespace music_manager_starter.Server.Services;

/// <summary>
/// Service for managing song ratings
/// </summary>
public interface IRatingService
{
    /// <summary>
    /// Rates a song for a specific user
    /// </summary>
    /// <param name="songId">The ID of the song to rate</param>
    /// <param name="value">The rating value (0-5 in 0.5 increments)</param>
    /// <param name="userId">The ID of the user rating the song</param>
    /// <returns>A task representing the async operation</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when rating is invalid</exception>
    Task RateSongAsync(Guid songId, double value, string userId);

    /// <summary>
    /// Gets a summary of ratings for a specific song
    /// </summary>
    /// <param name="songId">The ID of the song to get ratings for</param>
    /// <param name="userId">The ID of the user requesting the summary</param>
    /// <returns>A rating summary including average, distribution, and user's rating</returns>
    Task<RatingSummaryDto> GetSummaryAsync(Guid songId, string userId);
}