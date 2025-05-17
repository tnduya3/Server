using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server_1_.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        public string? UserName { get; set; }
        public string? Token { get; set; }
        public bool IsOnline { get; set; }
    }
}
