using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.Services
{
    public class AuthService
    {
        private readonly IUnitOfWork _uow;

        public AuthService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AppUser?> LoginAsync(string email, string password)
        {
            var hashed = HashPassword(password);
            var users = await _uow.AppUsers.FindAsync(u => u.Email == email && u.PasswordHash == hashed);
            var user = users.FirstOrDefault();

            if (user != null)
            {
                user.Role = await _uow.AppRoles.GetByIdAsync(user.RoleID);
            }

            return user;
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}