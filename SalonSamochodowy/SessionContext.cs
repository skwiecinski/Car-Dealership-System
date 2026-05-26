using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SalonSamochodowy.Entities;

namespace SalonSamochodowy
{
    public static class SessionContext
    {
        public static AppUser? CurrentUser { get; set; }
    }
}