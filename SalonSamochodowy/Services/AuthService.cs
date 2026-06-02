using System.Linq;
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
            var users = await _uow.AppUsers.FindAsync(u => u.Email == email && u.PasswordHash == password);
            var user = users.FirstOrDefault();

            if (user != null)
            {
                user.Role = await _uow.AppRoles.GetByIdAsync(user.RoleID);
            }

            return user;
        }
    }
}