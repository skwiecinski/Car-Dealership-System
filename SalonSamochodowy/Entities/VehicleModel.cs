using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class VehicleModel
    {
        public int ModelID { get; set; }
        [MaxLength(50)]
        public string Brand { get; set; } = null!;
        [MaxLength(100)]
        public string ModelName { get; set; } = null!;
        public virtual ICollection<TrimLevel> TrimLevels { get; set; } = new List<TrimLevel>();
    }
}
