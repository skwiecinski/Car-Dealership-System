using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalonSamochodowy.Entities
{
    internal class Dealership
    {
        public int DealershipID { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string Address { get; set; } = null!;

        [MaxLength(100)]
        public string City { get; set; } = null!;

        [MaxLength(200)]
        public string Owner { get; set; } = null!;
        public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}
