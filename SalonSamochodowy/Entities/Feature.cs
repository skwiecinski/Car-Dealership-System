using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class Feature
    {
        public int FeatureID { get; set; }
        [MaxLength(100)]
        public string FeatureName { get; set; } = null!;
        [MaxLength(50)]
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public virtual ICollection<TrimFeature> TrimFeatures { get; set; } = new List<TrimFeature>();
        public virtual ICollection<VehicleFeature> VehicleFeatures { get; set; } = new List<VehicleFeature>();
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}