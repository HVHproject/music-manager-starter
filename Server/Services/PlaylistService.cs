using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data.Models;
using music_manager_starter.Server.Repositories;
using music_manager_starter.Server.Services;
using System.Text;
using System.Text.Json;

namespace music_manager_starter.Server.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public async Task<Guid> CreatePlaylistAsync(string name, string userId)
        {
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedByUserId = userId
            };

            await _playlistRepository.AddAsync(playlist);
            await _playlistRepository.SaveChangesAsync();

            return playlist.Id;
        }

        public async Task<IReadOnlyList<Playlist>> GetAllPlaylistsAsync(string userId)
        {
            return await _playlistRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<Playlist> GetPlaylistAsync(Guid playlistId, string userId)
        {
            return await RequirePlaylistAsync(playlistId, userId);
        }

        public async Task DeletePlaylistAsync(Guid playlistId, string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);

            await _playlistRepository.DeleteAsync(playlist);
            await _playlistRepository.SaveChangesAsync();
        }

        public async Task RenamePlaylistAsync(Guid playlistId, string name, string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);

            playlist.Name = name;
            playlist.UpdatedAt = DateTime.UtcNow;

            await _playlistRepository.UpdateAsync(playlist);
            await _playlistRepository.SaveChangesAsync();
        }

        public async Task AddSongsAsync(
            Guid playlistId,
            IReadOnlyCollection<Guid> songIds,
            string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);

            var existingSongIds = playlist.PlaylistSongs
                .Select(ps => ps.SongId)
                .ToHashSet();

            var nextIndex = playlist.PlaylistSongs.Count == 0
                ? 0
                : playlist.PlaylistSongs.Max(ps => ps.OrderIndex) + 1;

            foreach (var songId in songIds.Distinct())
            {
                if (existingSongIds.Contains(songId))
                    continue;

                playlist.PlaylistSongs.Add(new PlaylistSong
                {
                    PlaylistId = playlist.Id,
                    SongId = songId,
                    OrderIndex = nextIndex++
                });
            }

            playlist.UpdatedAt = DateTime.UtcNow;

            await _playlistRepository.UpdateAsync(playlist);
            await _playlistRepository.SaveChangesAsync();
        }

        public async Task RemoveSongsAsync(
    Guid playlistId,
    IReadOnlyCollection<Guid> songIds,
    string userId)
        {
            await RequirePlaylistAsync(playlistId, userId);

            await _playlistRepository.RemoveSongsAsync(playlistId, songIds);
            await _playlistRepository.SaveChangesAsync();
        }

        public async Task ReorderSongsAsync(
            Guid playlistId,
            IReadOnlyList<Guid> orderedSongIds,
            string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);

            var lookup = playlist.PlaylistSongs
                .ToDictionary(ps => ps.SongId);

            var reordered = new List<PlaylistSong>();

            for (var i = 0; i < orderedSongIds.Count; i++)
            {
                if (!lookup.TryGetValue(orderedSongIds[i], out var ps))
                    throw new InvalidOperationException("Invalid song order.");

                reordered.Add(new PlaylistSong
                {
                    PlaylistId = playlistId,
                    SongId = ps.SongId,
                    OrderIndex = i
                });
            }

            await _playlistRepository.ReplaceSongsAsync(playlistId, reordered);
            await _playlistRepository.SaveChangesAsync();
        }

        public Task UndoAsync(Guid playlistId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task RedoAsync(Guid playlistId, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ExportAsync(
            Guid playlistId,
            string format,
            string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);

            var songs = playlist.PlaylistSongs
                .OrderBy(ps => ps.OrderIndex)
                .Select(ps => ps.Song)
                .ToList();

            return format.ToLowerInvariant() switch
            {
                "json" => JsonSerializer.Serialize(songs, new JsonSerializerOptions
                {
                    WriteIndented = true
                }),
                "csv" => ExportCsv(songs),
                _ => throw new ArgumentException("Unsupported export format.")
            };
        }

        private static string ExportCsv(IEnumerable<Song> songs)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Title,Artist,Album,Genre");

            foreach (var song in songs)
            {
                sb.AppendLine($"{song.Title},{song.Artist},{song.Album},{song.Genre}");
            }

            return sb.ToString();
        }

        private async Task<Playlist> RequirePlaylistAsync(Guid playlistId, string userId)
        {
            var playlist = await _playlistRepository
                .GetByIdAsync(playlistId, userId);

            if (playlist is null)
                throw new KeyNotFoundException("Playlist not found.");

            return playlist;
        }
    }
}