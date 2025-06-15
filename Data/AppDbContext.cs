using Microsoft.EntityFrameworkCore;
using TicketSprint.Model;

namespace TicketSprint.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<EventSector> EventSectors { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        
        public DbSet<Subsector> Subsectors { get; set; }
        
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Participant1)
                .WithMany(p => p.EventsAsParticipant1)
                .HasForeignKey(e => e.Participant1Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Participant2)
                .WithMany(p => p.EventsAsParticipant2)
                .HasForeignKey(e => e.Participant2Id)
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(f => f.FavoriteId);

                entity.HasOne(f => f.User)
                    .WithMany(u => u.Favorites)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Participant)
                    .WithMany()
                    .HasForeignKey(f => f.ParticipantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(f => new { f.UserId, f.ParticipantId }).IsUnique();
            });
            
            modelBuilder.Entity<Subsector>(entity =>
            {
                entity.HasKey(s => s.SubsectorId);

                entity.HasOne(s => s.Sector)
                    .WithMany(sector => sector.Subsectors)
                    .HasForeignKey(s => s.SectorId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<EventSector>(entity =>
            {
                entity.HasKey(es => es.EventSectorId);

                entity.HasOne(es => es.Subsector)
                    .WithMany(s => s.EventSectors)
                    .HasForeignKey(es => es.SubsectorId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(es => es.Event)
                    .WithMany(e => e.EventSectors)
                    .HasForeignKey(es => es.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                
                entity.HasIndex(es => new { es.EventId, es.SubsectorId }).IsUnique();
            });
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.EventSector)
                .WithMany(es => es.Tickets)
                .HasForeignKey(t => t.EventSectorId)
                .OnDelete(DeleteBehavior.Cascade); 

        }
    }
}