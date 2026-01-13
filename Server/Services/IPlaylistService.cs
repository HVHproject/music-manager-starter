using music_manager_starter.Data.Models;
using music_manager_starter.Shared;

namespace music_manager_starter.Server.Services
{
    public interface IPlaylistService
    {
        Task<Guid> CreatePlaylistAsync(string name, string userId);

        Task<IReadOnlyList<PlaylistDto>> GetAllPlaylistsAsync(string userId);

        Task<PlaylistDto> GetPlaylistAsync(Guid playlistId, string userId);

        Task DeletePlaylistAsync(Guid playlistId, string userId);

        Task RenamePlaylistAsync(Guid playlistId, string name, string userId);

        Task AddSongsAsync(
            Guid playlistId,
            IReadOnlyCollection<Guid> songIds,
            string userId);

        Task RemoveSongsAsync(Guid playlistId, 
            IReadOnlyCollection<Guid> songIds, 
            string userId);

        Task ReorderSongsAsync(
            Guid playlistId,
            IReadOnlyList<Guid> orderedSongIds,
            string userId);

        Task UndoAsync(Guid playlistId, string userId);

        Task RedoAsync(Guid playlistId, string userId);

        Task<string> ExportAsync(
            Guid playlistId,
            string format,
            string userId);
    }
}
