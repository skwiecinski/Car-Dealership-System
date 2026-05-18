using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class AppRole
    {
        public int RoleID { get; set; }
        [MaxLength(50)]
        public string RoleName { get; set; } = null!;
        public int PermissionLevel { get; set; }

        public virtual ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}