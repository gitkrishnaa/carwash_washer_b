using WasherService.Models;
using Microsoft.EntityFrameworkCore;

namespace WasherService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
    }
}
