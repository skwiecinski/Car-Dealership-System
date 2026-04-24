using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class Vehicle
    {
        public int VehicleID { get; set; }
        [MaxLength(17)]
        public string VIN { get; set; } = null!;
        public int TrimID { get; set; }
        public virtual TrimLevel Trim { get; set; } = null!;
        public int EngineID { get; set; }
        public virtual Engine Engine { get; set; } = null!;
        public int Mileage { get; set; }
        public bool IsUsed { get; set; }
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
        public virtual ICollection<VehicleFeature> VehicleFeatures { get; set; } = new List<VehicleFeature>();
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}
