using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace WasherService.Models
{
      [Index(nameof(MainId), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)] // ✅ Ensures Email is Unique
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

         [Required]
        public string MainId { get; set; }=Guid.NewGuid().ToString();  // ✅ Auto-generated Unique ID
    }
}
