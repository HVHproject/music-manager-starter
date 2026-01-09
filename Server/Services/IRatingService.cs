using music_manager_starter.Shared;

namespace music_manager_starter.Server.Services;

public interface IRatingService
{
    Task RateSongAsync(Guid songId, double value, string userId);
    Task<RatingSummaryDto> GetSummaryAsync(Guid songId, string userId);
}
