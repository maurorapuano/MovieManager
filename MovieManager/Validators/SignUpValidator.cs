using MovieManager.DTOs;
using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MovieManager.Validators
{
    public static class SignUpValidator
    {
        public static string? Validate(SignUpDTO dto)
        {
            var usernameError = ValidateUsername(dto.UserName);
            if (usernameError != null)
                return usernameError;

            var emailError = ValidateEmail(dto.Email);
            if (emailError != null)
                return emailError;

            var passwordError = ValidatePassword(dto.Password);
            if (passwordError != null)
                return passwordError;

            return null;
        }

        private static string? ValidateUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "Username is required.";

            if (username.Length < 3 || username.Length > 20)
                return "Username must contain between 3 and 20 characters.";

            return null;
        }

        private static string? ValidateEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email is required.";

            try
            {
                var addr = new MailAddress(email);
                if (addr.Address != email)
                    return "Invalid Email.";
            }
            catch (Exception ex)
            {
                return "Invalid Email.";
            }

            return null;
        }

        private static string? ValidatePassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password is required.";

            if (password.Length < 8)
                return "Password must contain at least 8 characters.";

            if (!Regex.IsMatch(password, @"[0-9]"))
                return "Password must include at least one number.";

            if (!Regex.IsMatch(password, @"[a-zA-Z]"))
                return "Password must include at least one letter.";

            return null;
        }

    }
}
