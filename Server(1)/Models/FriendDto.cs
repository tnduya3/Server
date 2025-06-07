namespace Server_1_.Models
{
    public class FriendRequestDto
    {
        public int UserId { get; set; }
        public int FriendId { get; set; }
    }

    public class FriendshipResponseDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsOnline { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class FriendshipStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
