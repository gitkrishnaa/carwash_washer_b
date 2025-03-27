using WasherService.Data;
using WasherService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WasherService.Controllers
{
    [Route("api/washer")]
    [ApiController]
    public class WasherController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public WasherController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
             _configuration = configuration;
        }

        // 🔹 Get all customers
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<User>>> GetCustomers()
        {


var adminToken = _configuration["accessToken:admin"];
            Console.WriteLine(adminToken);

            var token = Request.Headers["token"].ToString();
            Console.WriteLine(token);
            if (adminToken != token)
            {
                return Unauthorized("not match");
            };

            var users = await _context.Users
                .Select(c => new { c.Id, c.Name, c.Email, c.MainId })
                .ToListAsync();
            
            if (users == null || !users.Any())
            {
                return NotFound("No customers found.");
            }

            return Ok(users);
        }

        // 🔹 Get customer by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetCustomer(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            return Ok(user);
        }

        // 🔹 Create new customer (admin only)
        [HttpPost("create")]
        public async Task<ActionResult<User>> CreateCustomer([FromBody] User user)
        {
            // Check if the email already exists in the database
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email already exists.");
            }

            // Add new user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = user.Id }, user);
        }

        // 🔹 Delete customer by ID
        [HttpDelete("{id}")]
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
