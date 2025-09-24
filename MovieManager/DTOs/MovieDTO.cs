using System.ComponentModel.DataAnnotations;

namespace MovieManager.DTOs
{
    public class MovieDTO
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Director { get; set; }

        [Required]
        public string ReleaseYear { get; set; }

        public string? Description { get; set; }
    }
}
