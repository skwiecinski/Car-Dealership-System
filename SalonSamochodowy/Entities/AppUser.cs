using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class AppUser
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public virtual AppRole Role { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;
        [MaxLength(50)]
        public string LastName { get; set; } = null!;
        [MaxLength(100)]
        public string Email { get; set; } = null!;
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        
        public virtual Client? Client { get; set; }
        public virtual Worker? Worker { get; set; }
    }
}