using WasherService.Data;
using WasherService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WasherService.Controllers
{
    [Route("internal")]
    [ApiController]
    public class IntenalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public IntenalController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
             _configuration = configuration;
        }


       [HttpGet("test")]
        public async Task<ActionResult<IEnumerable<User>>> Test()
        {
            
            return Ok("ok");
        }

        // 🔹 Get all customers
        [HttpGet("washer/list")]
        public async Task<ActionResult<IEnumerable<User>>> AllWasherList()
        {

           var adminToken = _configuration["accessToken:admin"];
            Console.WriteLine(adminToken);

            var token = Request.Headers["token"].ToString();
            Console.WriteLine(token);
            if (adminToken != token)
            {
                return Unauthorized("token not match");
            };

            var users = await _context.Users
                .Select(c => new { c.Id, c.Name, c.Email, c.MainId })
                .ToListAsync();
            
            if (users == null || !users.Any())
            {
                return NotFound("No washer found.");
            }

            return Ok(users);
        }

        // 🔹 Get customer by ID
        [HttpGet("washer/detail/{id}")]
        public async Task<ActionResult<User>> WasherDetails(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }
            return Ok(user);
        }

      

        // 🔹 Delete customer by ID
        [HttpDelete("washer/delete/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            // Remove the user from the database
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 🔹 Update customer details
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                return BadRequest("Customer ID mismatch.");
            }

            // Check if the customer exists
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            // Update the user's information
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password; // Consider using hashed passwords for security

            // Save changes to the database
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
