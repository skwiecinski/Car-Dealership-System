using System.Windows;

namespace SalonSamochodowy.Services
{
    public static class LocalizationHelper
    {
        public static string GetString(string key)
        {
            if (Application.Current == null)
                return key;

            var resource = Application.Current.TryFindResource(key);
            if (resource != null && resource is string str)
            {
                return str;
            }

            return key;
        }
    }
}
