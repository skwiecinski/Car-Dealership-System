using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class TrimLevel
    {
        public int TrimID { get; set; }
        public int ModelID { get; set; }
        public virtual VehicleModel Model { get; set; } = null!;
        [MaxLength(50)]
        public string TrimName { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<TrimFeature> TrimFeatures { get; set; } = new List<TrimFeature>();
    }
}
