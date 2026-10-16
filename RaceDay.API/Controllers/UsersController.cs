using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ALL endpoints in this controller require login
    
    /// <summary>
    /// Handles user profile retrieval and updates for authenticated users.
    /// </summary>
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context) { _context = context; }

        /// <summary>
        /// Gets the authenticated user's profile.
        /// </summary>
        /// <response code="200">Returns the profile.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.Phone,
                user.CreatedAt
            });
        }

        /// <summary>
        /// Updates the authenticated user's profile.
        /// </summary>
        /// <response code="200">Profile updated.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(UserProfileDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }
    }
}
