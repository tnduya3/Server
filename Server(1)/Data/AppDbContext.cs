using Microsoft.EntityFrameworkCore;
using Server_1_.Models;

namespace Server_1_.Data // Thay YourProjectName bằng tên project của bạn
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet cho các models của bạn
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
    }
}