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

        public async Task<IReadOnlyList<Playlist>> GetAllByUserIdAsync(string userId)
        {
            return await _context.Playlists
                .Where(p => p.CreatedByUserId == userId)
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();
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

        public async Task RemoveSongsAsync(Guid playlistId, IReadOnlyCollection<Guid> songIds)
        {
            if (songIds == null || !songIds.Any())
                return;

            var allPlaylistSongs = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .ToListAsync();

            var songsToRemove = allPlaylistSongs
                .Where(ps => songIds.Contains(ps.SongId))
                .ToList();

            if (songsToRemove.Count == 0)
                return;

            _context.PlaylistSongs.RemoveRange(songsToRemove);

            var remainingSongs = allPlaylistSongs
                .Where(ps => !songIds.Contains(ps.SongId))
                .OrderBy(ps => ps.OrderIndex)
                .ToList();

            for (int i = 0; i < remainingSongs.Count; i++)
            {
                remainingSongs[i].OrderIndex = i;
            }

            foreach (var song in remainingSongs)
            {
                _context.Entry(song).State = EntityState.Modified;
            }

            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist != null)
            {
                playlist.UpdatedAt = DateTime.UtcNow;
                _context.Entry(playlist).State = EntityState.Modified;
            }
        }

        public async Task<IReadOnlyList<PlaylistSong>> GetSongsAsync(Guid playlistId)
        {
            return await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .OrderBy(ps => ps.OrderIndex)
                .ToListAsync();
        }

        public async Task ReplaceSongsAsync(Guid playlistId, IReadOnlyList<PlaylistSong> songs)
        {
            // Get existing songs from database
            var existing = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .ToListAsync();

            // Create a dictionary for lookups
            var existingLookup = existing.ToDictionary(e => e.SongId);

            // Create a set of incoming song IDs
            var incomingSongIds = songs.Select(s => s.SongId).ToHashSet();


            var songsToRemove = existing.Where(e => !incomingSongIds.Contains(e.SongId)).ToList();
            _context.PlaylistSongs.RemoveRange(songsToRemove);

            // Update or add songs with new order
            for (int i = 0; i < songs.Count; i++)
            {
                var newSong = songs[i];

                if (existingLookup.TryGetValue(newSong.SongId, out var existingSong))
                {
                    existingSong.OrderIndex = i;
                    _context.Entry(existingSong).State = EntityState.Modified;
                }
                else
                {
                    newSong.OrderIndex = i;
                    newSong.PlaylistId = playlistId;
                    _context.PlaylistSongs.Add(newSong);
                }
            }

            var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId);
            if (playlist != null)
            {
                playlist.UpdatedAt = DateTime.UtcNow;
                _context.Entry(playlist).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}