using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medical.Models;
using medical.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.JsonPatch;


namespace MedicalApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserProfile
        [HttpGet]
        public async Task<ActionResult<User>> GetProfile()
        {
            // Try to get user ID from NameIdentifier claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                // Fallback to email claim if NameIdentifier is not used
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized("Invalid user token.");
                }

                var userByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == emailClaim);

                if (userByEmail == null)
                {
                    return NotFound("User not found.");
                }

                userByEmail.Password = null;
                return Ok(userByEmail);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Password = null;
            return Ok(user);
        }

        // PATCH: api/UserProfile
        [HttpPatch]
        public async Task<IActionResult> PatchProfile([FromBody] JsonPatchDocument<User> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Patch document is null.");
            }

            // Try to get user ID from NameIdentifier claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                // Fallback to email claim
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized("Invalid user token.");
                }

                var userByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == emailClaim);

                if (userByEmail == null)
                {
                    return NotFound("User not found.");
                }

                userId = userByEmail.Id;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Validate the patch document
            foreach (var operation in patchDoc.Operations)
            {
                if (operation.path.ToLower().Contains("id") ||
                    operation.path.ToLower().Contains("role") ||
                    operation.path.ToLower().Contains("createdat") ||
                    operation.path.ToLower().Contains("password"))
                {
                    return BadRequest($"Updating {operation.path} is not allowed.");
                }
            }

            // Apply the patch with custom error handling
            patchDoc.ApplyTo(user, error =>
            {
                ModelState.AddModelError(error.AffectedObject?.ToString() ?? "PatchError", error.ErrorMessage);
            });

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate the updated entity
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error updating profile: {ex.InnerException?.Message ?? ex.Message}");
            }

            return NoContent();
        }
    }
}