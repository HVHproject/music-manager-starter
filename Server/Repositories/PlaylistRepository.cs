using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using music_manager_starter.Server.Repositories;

namespace music_manager_starter.Server.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly DataDbContext _context;

        public PlaylistRepository(DataDbContext context)
        {
            _context = context;
        }

        public async Task<Playlist?> GetByIdAsync(Guid playlistId, string userId)
        {
            return await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p =>
                    p.Id == playlistId &&
                    p.CreatedByUserId == userId);
        }

        public async Task AddAsync(Playlist playlist)
        {
            await _context.Playlists.AddAsync(playlist);
        }

        public Task UpdateAsync(Playlist playlist)
        {
            _context.Playlists.Update(playlist);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Playlist playlist)
        {
            _context.Playlists.Remove(playlist);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<PlaylistSong>> GetSongsAsync(Guid playlistId)
        {
            return await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .OrderBy(ps => ps.OrderIndex)
                .ToListAsync();
        }

        public async Task ReplaceSongsAsync(
            Guid playlistId,
            IReadOnlyList<PlaylistSong> songs)
        {
            var existing = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .ToListAsync();

            _context.PlaylistSongs.RemoveRange(existing);
            await _context.PlaylistSongs.AddRangeAsync(songs);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
