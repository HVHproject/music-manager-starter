using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace music_manager_starter.Data
{
    public class DataDbContext : DbContext
    {
        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) { }

        public DbSet<Song> Songs { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Song>().HasData(
                new Song { Id = Guid.Parse("6f47c84f-4a7d-4e83-8b8f-1829f0eafca3"), Title = "Circle With Me", Artist = "Spiritbox", Album = "Spiritbox", Genre = "Metal" },
                new Song { Id = Guid.Parse("2a76a0b1-b3e1-4ff0-9aa5-5f5e4c81bc45"), Title = "Notes on a River Town", Artist = "Pony Bradshaw", Album = "Canyon", Genre = "Folk" },
                new Song { Id = Guid.Parse("fa38a0ed-4f00-48e2-b9c5-5d68f9c0ef41"), Title = "Flower Shops", Artist = "Morgan Allen", Album = "Single", Genre = "Country" },
                new Song { Id = Guid.Parse("d94aa1d4-75ee-4f7a-a89f-f77de7050c8d"), Title = "Sweater Weather", Artist = "The Neighbourhood", Album = "I Love You.", Genre = "Alternative" },
                new Song { Id = Guid.Parse("42e4b4d5-93bb-4e46-bb6e-c57de62e7f6e"), Title = "When the Party's Over", Artist = "Billie Eilish", Album = "When We All Fall Asleep, Where Do We Go?", Genre = "Pop" },
                new Song { Id = Guid.Parse("b7cc1c82-77e2-40d0-8bc2-d7e05962c0e3"), Title = "Utah", Artist = "French Cassettes", Album = "The Great Escape", Genre = "Indie" },
                new Song { Id = Guid.Parse("22aa6f84-06d8-4a0e-bdad-3000b35b5b5f"), Title = "Something Real", Artist = "Post Malone", Album = "Twelve Carat Toothache", Genre = "Hip Hop" }
            );

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Value)
                      .HasPrecision(2, 1)
                      .IsRequired();

                entity.Property(r => r.UserId)
                      .IsRequired();

                entity.Property(r => r.CreatedAt)
                      .IsRequired();

                entity.HasOne(r => r.Song)
                      .WithMany()
                      .HasForeignKey(r => r.SongId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Enforce one rating per user per song
                entity.HasIndex(r => new { r.SongId, r.UserId })
                      .IsUnique();

                // Performance index for analytics
                entity.HasIndex(r => r.SongId);
            });

            modelBuilder.Entity<PlaylistSong>(entity =>
            {
                entity.HasKey(ps => new { ps.PlaylistId, ps.SongId });

                entity.HasOne(ps => ps.Playlist)
                      .WithMany(p => p.PlaylistSongs)
                      .HasForeignKey(ps => ps.PlaylistId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ps => ps.Song)
                      .WithMany()
                      .HasForeignKey(ps => ps.SongId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ps => new { ps.PlaylistId, ps.OrderIndex });
            });
        }

    }
}
