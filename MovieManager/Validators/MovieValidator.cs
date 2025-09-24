using System;

namespace MovieManager.Validators
{
    public static class MovieValidator
    {
        public static string? Validate(string title, string director, string releaseYear, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "Title is required.";

            if (title.Length < 2)
                return "Title must be at least 2 characters long.";

            if (string.IsNullOrWhiteSpace(director))
                return "Director is required.";

            if (director.Length < 2)
                return "Director must be at least 2 characters long.";

            if (string.IsNullOrWhiteSpace(releaseYear))
                return "Release Year is required.";

            if (!int.TryParse(releaseYear, out int year) || year < 1888 || year > DateTime.Now.Year + 1)
                return "Release Year must be a valid year.";

            return null;
        }
    }
}
