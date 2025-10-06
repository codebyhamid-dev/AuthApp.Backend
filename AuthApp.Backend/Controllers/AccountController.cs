using AuthApp.Backend.AuthApp.Backend.Contracts;
using AuthApp.Backend.AuthApp.Backend.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApp.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager,IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        // ✅ Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // ✅ 1. Validate passwords match
            if (registerDto.Password != registerDto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });
            // ✅ 2. Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email is already registered." });
            // ✅ 3. Create new user
            var user = new ApplicationUser
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                UserName= registerDto.Email // Using email as username
            };
            // ✅ 4. Create user with hashed password
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            // ❌ Handle failed registration
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "User registration failed.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }
            return Ok(new
            {
                message = "User registered successfully.",
            });

        }

        // ✅ LOGIN
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!passwordValid)
                return Unauthorized(new { message = "Invalid email or password." });

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "User Login Successfully!",
                token
            });
        }

        // ✅ PRIVATE: JWT Token Generator
        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
