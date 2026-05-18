using System.Linq;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.Services
{
    public class AuthService
    {
        public async Task<AppUser?> LoginAsync(string email, string password)
        {
            using (var dbContext = new AppDbContext())
            using (var unitOfWork = new UnitOfWork(dbContext))
            {
                var users = await unitOfWork.AppUsers.FindAsync(u => u.Email == email && u.PasswordHash == password);
                var user = users.FirstOrDefault();

                if (user != null)
                {
                    user.Role = await unitOfWork.AppRoles.GetByIdAsync(user.RoleID);
                }

                return user;
            }
        }
    }
}