using MovieManager.Entities;
using System.ComponentModel.DataAnnotations;

namespace MovieManager.DTOs
{
    /// <summary>
    /// User registration request
    /// </summary>
    public class SignUpDTO
    {
        /// <example>john.doe</example>
        [Required]
        public string UserName { get; set; }

        /// <example>john.doe@test.com</example>
        [Required]
        public string Email { get; set; }

        /// <example>P@ssw0rd!</example>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Role of the user.  
        /// 1 = Admin (full access)  
        /// 2 = Regular (view only)
        /// </summary>
        /// <example>2</example>
        [Required]
        public int RoleId { get; set; }

    }
}
