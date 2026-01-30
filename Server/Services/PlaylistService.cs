using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data.Models;
using music_manager_starter.Server.Repositories;
using music_manager_starter.Server.Services;
using music_manager_starter.Shared;
using System.Text;
using System.Text.Json;

namespace music_manager_starter.Server.Services
{
    /// <summary>
    /// Implementation of the playlist service that manages playlist business logic
    /// </summary>
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;

        /// <summary>
        /// Maps a Playlist entity to a PlaylistDto
        /// </summary>
        /// <param name="playlist">Playlist entity to map</param>
        /// <returns>PlaylistDto with ordered songs</returns>
        private static PlaylistDto MapToDto(Playlist playlist)
        {
            return new PlaylistDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                CreatedByUserId = playlist.CreatedByUserId,
                CreatedAt = playlist.CreatedAt,
                UpdatedAt = playlist.UpdatedAt,
                UpdatedByUserId = playlist.UpdatedByUserId ?? string.Empty,
                PlaylistSongs = playlist.PlaylistSongs
                    .OrderBy(ps => ps.OrderIndex)
                    .Select(ps => new PlaylistSongDto
                    {
                        PlaylistId = ps.PlaylistId,
                        SongId = ps.SongId,
                        OrderIndex = ps.OrderIndex,
                        AddedAt = ps.AddedAt,
                        Song = new Shared.Song
                        {
                            Id = ps.Song.Id,
                            Title = ps.Song.Title,
                            Artist = ps.Song.Artist,
                            Album = ps.Song.Album,
                            Genre = ps.Song.Genre
                        }
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Initializes a new instance of the PlaylistService
        /// </summary>
        /// <param name="playlistRepository">Repository for playlist data access</param>
        public PlaylistService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        /// <inheritdoc/>
        public async Task<Guid> CreatePlaylistAsync(string name, string userId)
        {
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedByUserId = userId,
                UpdatedByUserId = userId,
            };

            await _playlistRepository.AddAsync(playlist);
            await _playlistRepository.SaveChangesAsync();

            return playlist.Id;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PlaylistDto>> GetAllPlaylistsAsync(string userId)
        {
            var playlists = await _playlistRepository.GetAllByUserIdAsync(userId);
            return playlists.Select(MapToDto).ToList();
        }

        /// <inheritdoc/>
        public async Task<PlaylistDto> GetPlaylistAsync(Guid playlistId, string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);
            return MapToDto(playlist);
        }

        /// <inheritdoc/>
        public async Task DeletePlaylistAsync(Guid playlistId, string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);

            await _playlistRepository.DeleteAsync(playlist);
            await _playlistRepository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RenamePlaylistAsync(Guid playlistId, string name, string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);

            playlist.Name = name;
            playlist.UpdatedAt = DateTime.UtcNow;
            playlist.UpdatedByUserId = userId;

            await _playlistRepository.UpdateAsync(playlist);
            await _playlistRepository.SaveChangesAsync();
        }

        /// <inheritdoc/>
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
            playlist.UpdatedByUserId = userId;

            await _playlistRepository.UpdateAsync(playlist);
            await _playlistRepository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RemoveSongsAsync(
            Guid playlistId,
            IReadOnlyCollection<Guid> songIds,
            string userId)
        {
            await RequirePlaylistAsync(playlistId, userId);

            await _playlistRepository.RemoveSongsAsync(playlistId, songIds, userId);
            await _playlistRepository.SaveChangesAsync();
        }

        /// <inheritdoc/>
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

            await _playlistRepository.ReplaceSongsAsync(playlistId, reordered, userId);
            await _playlistRepository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<string> ExportAsync(
            Guid playlistId,
            string format,
            string userId)
        {
            var playlist = await RequirePlaylistAsync(playlistId, userId);

            var songs = playlist.PlaylistSongs
                .OrderBy(ps => ps.OrderIndex)
                .Select(ps => ConvertToSharedSong(ps.Song))
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

        /// <summary>
        /// Converts a data model Song to a shared Song DTO
        /// </summary>
        /// <param name="modelSong">Data model song entity</param>
        /// <returns>Shared song DTO with basic song information</returns>
        private static Shared.Song ConvertToSharedSong(Data.Models.Song modelSong)
        {
            return new Shared.Song
            {
                Id = modelSong.Id,
                Title = modelSong.Title,
                Artist = modelSong.Artist,
                Album = modelSong.Album,
                Genre = modelSong.Genre,
                YearReleased = modelSong.YearReleased,
                // Note: AverageRating, UserRating, TotalRatings will be default values since they are not exported
                AverageRating = null,
                UserRating = null,
                TotalRatings = 0
            };
        }

        /// <summary>
        /// Exports songs to CSV format
        /// </summary>
        /// <param name="songs">Songs to export</param>
        /// <returns>CSV-formatted string</returns>
        private static string ExportCsv(IEnumerable<Shared.Song> songs)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Title,Artist,Album,Genre,YearReleased");

            foreach (var song in songs)
            {
                var title = EscapeCsvField(song.Title);
                var artist = EscapeCsvField(song.Artist);
                var album = EscapeCsvField(song.Album);
                var genre = EscapeCsvField(song.Genre);
                var year = song.YearReleased?.ToString() ?? "";

                sb.AppendLine($"{title},{artist},{album},{genre},{year}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Escapes a field for CSV formatting
        /// </summary>
        /// <param name="field">Field value to escape</param>
        /// <returns>Escaped field value</returns>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        /// <summary>
        /// Retrieves a playlist and ensures it exists and belongs to the user
        /// </summary>
        /// <param name="playlistId">ID of the playlist to retrieve</param>
        /// <param name="userId">ID of the user requesting the playlist</param>
        /// <returns>The playlist entity</returns>
        /// <exception cref="KeyNotFoundException">Thrown when playlist is not found or doesn't belong to user</exception>
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