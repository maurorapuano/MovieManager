using MovieManager.Entities;
using System.ComponentModel.DataAnnotations;

namespace MovieManager.DTOs
{
    public class SignUpDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; }

    }
}
