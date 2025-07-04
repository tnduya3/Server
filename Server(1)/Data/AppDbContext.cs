using Microsoft.EntityFrameworkCore;
using Server_1_.Models;
using static Server_1_.Models.Friends;

namespace Server_1_.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<ChatRooms> Chatrooms { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Participants> Participants { get; set; }
        public DbSet<Friends> Friends { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Medias> Medias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Messages>()
                .HasOne<Users>() // Specify the related entity type explicitly
                .WithMany() // Assuming no navigation property in Users for Messages
                .HasForeignKey(m => m.SenderId);

            modelBuilder.Entity<Messages>()
                .HasOne<ChatRooms>()
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình khóa chính cho Participant (khóa chính kết hợp)
            modelBuilder.Entity<Participants>()
                .HasKey(p => new { p.ChatroomId, p.UserId });

            // Cấu hình khóa chính cho Friend (khóa chính kết hợp)
            modelBuilder.Entity<Friends>()
                .HasKey(f => new { f.UserId, f.FriendId });

            modelBuilder.Entity<Friends>()
                .HasOne(f => f.User)
                .WithMany(u => u.SentFriendRequests) // Sử dụng SentFriendRequests
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Hoặc NoAction để tránh xóa cascade

            modelBuilder.Entity<Friends>()
                .HasOne(f => f.FriendUser)
                .WithMany(u => u.ReceivedFriendRequests) // Sử dụng ReceivedFriendRequests
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict); // Hoặc NoAction

        }
    }
}