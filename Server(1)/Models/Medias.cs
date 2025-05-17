using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class Medias
    {
        [Key]
        public int MediaId { get; set; } // MediaId, là khóa chính duy nhất

        // Khóa ngoại liên kết Media với Message (một media thuộc về một tin nhắn)
        [ForeignKey("Messages")]
        public int MessageId { get; set; }
        public required Messages Message { get; set; }


        public required string FileUrl { get; set; } // Đường dẫn đến tệp tin (ví dụ: trên cloud storage)

        public required string FileType { get; set; } // Loại tệp tin (ví dụ: image/jpeg, video/mp4)

        public long FileSize { get; set; } // Kích thước tệp tin (bytes)

        public DateTime UploadAt { get; set; } = DateTime.UtcNow;

        // Các thuộc tính khác có thể có:
        // public string? ThumbnailUrl { get; set; } // Đường dẫn đến ảnh thumbnail
        // public int? Width { get; set; }
        // public int? Height { get; set; }
        // public string? Caption { get; set; }
    }
}
