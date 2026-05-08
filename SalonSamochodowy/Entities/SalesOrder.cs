using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class SalesOrder
    {
        public int OrderID { get; set; }
        public int VehicleID { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;
        public int ClientID { get; set; }
        public virtual Client Client { get; set; } = null!;
        public int WorkerID { get; set; }
        public virtual Worker Worker { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal FinalPrice { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } = null!;
        public int DealershipID { get; set; }
        public virtual Dealership Dealership { get; set; } = null!;
    }
}
