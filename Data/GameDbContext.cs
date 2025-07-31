using Microsoft.EntityFrameworkCore;
using Arcane_Coop.Models;

namespace Arcane_Coop.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<GameRoom> GameRooms { get; set; }
    public DbSet<Player> Players { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameRoom>(entity =>
        {
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.RoomId).HasMaxLength(50);
            entity.Property(e => e.RoomName).HasMaxLength(100);
            entity.Property(e => e.GameStateJson).HasColumnType("TEXT");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId);
            entity.Property(e => e.ConnectionId).HasMaxLength(100);
            entity.Property(e => e.PlayerName).HasMaxLength(50);
            entity.Property(e => e.Character).HasMaxLength(20);
            entity.Property(e => e.City).HasMaxLength(20);
            entity.Property(e => e.RoomId).HasMaxLength(50);

            entity.HasOne(e => e.Room)
                  .WithMany(r => r.Players)
                  .HasForeignKey(e => e.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}