using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medical.Data;
using medical.Models;
using System.Threading.Tasks;
using System;

namespace medical.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Users/patient
        [HttpPost("patient")]
        public async Task<IActionResult> CreatePatient([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.Role = UserRole.Patient;
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient créé avec succès", user });//message
        }

        // GET: api/Users/patients
        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Users
                .Where(u => u.Role == UserRole.Patient)
                .ToListAsync();

            return Ok(patients);
        }
    }
}
