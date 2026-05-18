using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalonSamochodowy.Entities
{
    public class Worker
    {
        public int WorkerID { get; set; }
        public int UserID { get; set; }
        public virtual AppUser User { get; set; } = null!;
        public decimal Payroll { get; set; }
        public DateTime EndOfContractDate { get; set; }
        public int DealershipID { get; set; }
        public virtual Dealership Dealership { get; set; } = null!;
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}
