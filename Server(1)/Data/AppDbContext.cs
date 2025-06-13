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

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // Cấu hình các mối quan hệ, khóa chính, khóa ngoại (sử dụng Fluent API nếu cần)
        //    // Chúng ta đã định nghĩa khóa chính và khóa ngoại bằng Data Annotations,
        //    // nhưng bạn có thể ghi đè hoặc bổ sung cấu hình tại đây nếu cần.

        //    // Ví dụ về cấu hình unique constraint (nếu bạn muốn)
        //    modelBuilder.Entity<Friends>()
        //        .HasIndex(f => new { f.UserId, f.FriendId })
        //        .IsUnique();

        //    base.OnModelCreating(modelBuilder);
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           // ---Seed Data cho Users-- -
           //modelBuilder.Entity<Users>().HasData(
           //    new Users { UserId = 1, UserName = "Alice", Token = "password123", IsOnline = false, IsActive = false, CreatedAt = }, // Password đã hash
           //    new Users { UserId = 2, UserName = "Bob", Token = "password123" },
           //    new Users { UserId = 3, UserName = "Charlie", Token = "password123" }
           //);

            //// --- Seed Data cho Chatrooms ---
            //modelBuilder.Entity<ChatRooms>().HasData(
            //    new ChatRooms { ChatRoomId = 1, Name = "General Chat", CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            //    new ChatRooms { ChatRoomId = 2, Name = "Tech Talk", CreatedAt = new DateTime(2023, 1, 1, 10, 5, 0, DateTimeKind.Utc) },
            //    new ChatRooms { ChatRoomId = 3, Name = "Off-Topic", CreatedAt = new DateTime(2023, 1, 1, 10, 10, 0, DateTimeKind.Utc) }
            //);

            //// --- Seed Data cho Participants (kết nối Users và Chatrooms) ---
            //// Alice và Bob tham gia General Chat
            //modelBuilder.Entity<Participants>().HasData(
            //    new Participants
            //    {
            //        ChatroomId = 1,
            //        UserId = 1,
            //        JoinedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            //        Role = "member"
            //    },
            //    new Participants
            //    {
            //        ChatroomId = 1,
            //        UserId = 2,
            //        JoinedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            //        Role = "member",
            //    }
            //);
            //// Charlie tham gia Tech Talk
            //modelBuilder.Entity<Participants>().HasData(
            //    new Participants
            //    {
            //        ChatroomId = 2,
            //        UserId = 3,
            //        JoinedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            //        Role = "member"
            //    }
            //);

            //// --- Seed Data cho Messages ---
            //modelBuilder.Entity<Messages>().HasData(
            //    new Messages { MessageId = 1, ChatRoomId = 1, SenderId = 1, Message = "Hello everyone in General Chat!", CreatedAt = new DateTime(2023, 1, 1, 10, 15, 0, DateTimeKind.Utc) },
            //    new Messages { MessageId = 2, ChatRoomId = 1, SenderId = 2, Message = "Hi Alice!", CreatedAt = new DateTime(2023, 1, 1, 10, 16, 0, DateTimeKind.Utc) },
            //    new Messages { MessageId = 3, ChatRoomId = 2, SenderId = 3, Message = "Anyone here interested in AI?", CreatedAt = new DateTime(2023, 1, 1, 10, 17, 0, DateTimeKind.Utc) }
            //);

            //// --- Seed Data cho Friends (tùy chọn) ---

            //// Thêm các cấu hình khác nếu có (ví dụ: Fluent API cho mối quan hệ, v.v.)
            //// Đảm bảo cấu hình mối quan hệ giữa User và Message (nếu chưa có)

            //modelBuilder.Entity<Friends>().HasData(
            //    new Friends { UserId = 1, FriendId = 2, Status = FriendshipStatus.Accepted, CreatedAt = new DateTime(2023, 1, 1, 9, 0, 0, DateTimeKind.Utc) },
            //    new Friends { UserId = 2, FriendId = 1, Status = FriendshipStatus.Accepted, CreatedAt = new DateTime(2023, 1, 1, 9, 1, 0, DateTimeKind.Utc) }
            //);
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