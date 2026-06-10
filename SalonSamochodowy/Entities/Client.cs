using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class Client
    {
        public int ClientID { get; set; }
        public int UserID { get; set; }
        public virtual AppUser User { get; set; } = null!;
        [MaxLength(15)]
        
        public string? NIP { get; set; }
        [MaxLength(20)]
        public string Phone { get; set; } = null!;
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}