using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    // Engine class
    public class Engine
    {
        public int EngineID { get; set; }
        [MaxLength(50)]
        public string EngineName { get; set; } = null!;
        [MaxLength(20)]
        public string EngineSize { get; set; } = null!;
        public int Power { get; set; }
        public decimal Price { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    }
}