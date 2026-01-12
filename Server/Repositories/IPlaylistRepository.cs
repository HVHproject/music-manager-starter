using music_manager_start.Data.Models;

namespace music_manager_starter.Server.Repositories
{
    public interface IPlaylistRepository
    {
        Task<Playlist?> GetByIdAsync(Guid playlistId, string userId);

        Task AddAsync(Playlist playlist);

        Task UpdateAsync(Playlist playlist);

        Task DeleteAsync(Playlist playlist);

        Task<IReadOnlyList<PlaylistSong>> GetSongsAsync(Guid playlistId);

        Task ReplaceSongsAsync(
            Guid playlistId,
            IReadOnlyList<PlaylistSong> songs);

        Task SaveChangesAsync();
    }
}
