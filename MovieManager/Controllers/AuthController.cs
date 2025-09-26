using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MovieManager.AppServices;
using MovieManager.DTOs;
using MovieManager.Entities;
using MovieManager.Validators;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MovieManager.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AuthService _service;

        public AuthController(AppDbContext context, IConfiguration configuration, AuthService authService)
        {
            _context = context;
            _configuration = configuration;
            _service = authService;
        }

        /// <summary>
        /// Register a new user (Admin or Regular)
        /// </summary>
        /// <remarks>
        /// Roles:
        /// - 1 = Admin (can add, edit, delete movies)
        /// - 2 = Regular (can only view movies)
        /// </remarks>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Invalid request</response>
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpDTO signUpDTO)
        {
            try
            {
                string error = SignUpValidator.Validate(signUpDTO);
                if (error != null)
                {
                    return BadRequest(new { message = error });
                }

                var existingUser = await _service.GetUserFromDBAsync(signUpDTO.UserName);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already registered." });
                }

                await _service.CreateUserAsync(new User
                {
                    Username = signUpDTO.UserName,
                    Email = signUpDTO.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(signUpDTO.Password),
                    RoleId = signUpDTO.RoleId
                });

                User newUser = new User
                {
                    Username = signUpDTO.UserName,
                    Email = signUpDTO.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(signUpDTO.Password),
                    RoleId = signUpDTO.RoleId
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User registered succesfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            
        }

        /// <summary>
        /// Authenticate a user and get a JWT token.
        /// </summary>
        /// <remarks>
        /// Example request:
        /// {
        ///   "username": "adminuser",
        ///   "password": "P@ssw0rd!"
        /// }
        ///
        /// Example response:
        /// {
        ///   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        /// }
        /// </remarks>
        /// <response code="200">Authentication successful, returns JWT token</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            try
            {
                User existingUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDTO.UserName);

                if (existingUser == null)
                    return Unauthorized(new { message = "User and/or Password are incorrect." });

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDTO.Password, existingUser.PasswordHash);
                if (!isPasswordValid)
                    return Unauthorized(new { message = "User and/or Password are incorrect." });


                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, existingUser.Username),
                    new Claim(ClaimTypes.Role, existingUser.Role.Name)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"],
                    audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"])),
                    signingCredentials: creds
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }           

        }
    }
}
