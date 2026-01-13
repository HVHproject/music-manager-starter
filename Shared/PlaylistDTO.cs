using System;
using System.Collections.Generic;

namespace music_manager_starter.Shared
{
    public sealed class PlaylistDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PlaylistSongDto> PlaylistSongs { get; set; } = new();
    }

    public sealed class PlaylistSongDto
    {
        public Guid PlaylistId { get; set; }
        public Guid SongId { get; set; }
        public Song Song { get; set; } = null!;
        public int OrderIndex { get; set; }
        public DateTime AddedAt { get; set; }
    }
}