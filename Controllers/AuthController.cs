using WasherService.Data;
using WasherService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WasherService.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 🔹 Admin Registration (Signup)
        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(a => a.Email == user.Email))
                return BadRequest(" email already exists!");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password); // Hash Password
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new {message="Washer registered successfully!"});
        }

        // 🔹 Admin Login (Signin)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest user)
        {
            var dbAdmin = await _context.Users.FirstOrDefaultAsync(a => a.Email == user.Email);
            if (dbAdmin == null || !BCrypt.Net.BCrypt.Verify(user.Password, dbAdmin.Password))
                return Unauthorized("Invalid credentials!");

            var token = GenerateJwtToken(dbAdmin);
            return Ok(new { Token = token,user=dbAdmin });
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var claims = new List<Claim>
            {
                // new Claim("mainId", user.MainId.ToString()),
                //  new Claim("id", user.Id.ToString()),
                // new Claim("email", user.Email)

        new Claim(JwtRegisteredClaimNames.Sub,  user.Id.ToString()),
            new Claim("MainId", user.MainId.ToString()),
               new Claim("email", user.Email),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
