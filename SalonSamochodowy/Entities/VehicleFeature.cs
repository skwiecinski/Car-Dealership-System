using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalonSamochodowy.Entities
{
    public class VehicleFeature
    {
        public int VehicleID { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;
        public int FeatureID { get; set; }
        public virtual Feature Feature { get; set; } = null!;
        public decimal PurchasePrice { get; set; }
    }
}
