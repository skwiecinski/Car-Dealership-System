using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SalonSamochodowy.Entities
{
    public class TrimFeature
    {
        public int TrimID { get; set; }
        public virtual TrimLevel Trim { get; set; } = null!;
        public int FeatureID { get; set; }
        public virtual Feature Feature { get; set; } = null!;
        public bool IsStandard { get; set; }
        public decimal AdditionalPrice { get; set; }

    }
}