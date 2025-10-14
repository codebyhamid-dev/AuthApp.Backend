using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApp.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        // 🌍 Public — accessible to anyone (no token required)
        [AllowAnonymous]
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("🌍 Public endpoint — anyone can access this.");
        }

        // 👤 Authenticated — accessible to any logged-in user (User or Admin)
        [Authorize(Roles = "User")]
        [HttpGet("user")]
        public IActionResult UserOnly()
        {
            return Ok($"👤 Hello {User.Identity?.Name}, you are authenticated!");
        }

        // 🛡️ Admin-only — requires Admin role
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok($"🛡️ Welcome Admin {User.Identity?.Name}, you have special access!");
        }

        // 🤝 Shared access — either User or Admin role can access
        [Authorize(Roles = "User,Admin")]
        [HttpGet("shared")]
        public IActionResult SharedAccess()
        {
            return Ok("🤝 Shared endpoint — both Users and Admins can access this.");
        }
    }
}
