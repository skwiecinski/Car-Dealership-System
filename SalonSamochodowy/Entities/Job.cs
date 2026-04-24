using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class Job
    {
        public int JobID { get; set; }
        public int VehicleID { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;
        public int WorkerID { get; set; }
        public virtual Worker Worker { get; set; } = null!;
        public int FeatureID { get; set; }
        public virtual Feature Feature { get; set; } = null!;
        [MaxLength(50)]
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

    }
}
