using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.Services
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _uow;

        public ClientService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
        {
            var clients = await _uow.Clients.GetAllWithIncludesAsync(c => c.User);
            var dtos = new List<ClientDto>();

            foreach (var c in clients)
            {
                if (c.User != null)
                {
                    dtos.Add(new ClientDto
                    {
                        ClientID = c.ClientID,
                        UserID = c.User.UserID,
                        FirstName = c.User.FirstName,
                        LastName = c.User.LastName,
                        Email = c.User.Email,
                        Phone = c.Phone ?? "",
                        NIP = c.NIP
                    });
                }
            }
            return dtos;
        }

        public async Task<ClientDto?> GetClientByIdAsync(int clientId)
        {
            var clients = await _uow.Clients.FindWithIncludesAsync(x => x.ClientID == clientId, x => x.User);
            var c = clients.FirstOrDefault();
            if (c == null || c.User == null) return null;
            var user = c.User;

            return new ClientDto
            {
                ClientID = c.ClientID,
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = c.Phone ?? "",
                NIP = c.NIP
            };
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var existing = await _uow.AppUsers.FindAsync(u => u.Email == email);
            return existing.Any();
        }

        public async Task<ClientDto> CreateClientAsync(string firstName, string lastName, string email, string phone, string? nip)
        {
            var roleKlient = (await _uow.AppRoles.FindAsync(r => r.RoleName == RoleNames.Klient)).FirstOrDefault();
            if (roleKlient == null)
                throw new Exception("Rola 'Klient' nie istnieje w bazie danych.");

            var newUser = new AppUser
            {
                FirstName    = firstName,
                LastName     = lastName,
                Email        = email,
                PasswordHash = "",
                RoleID       = roleKlient.RoleID,
                BirthDate    = DateTime.Today
            };
            
            await _uow.AppUsers.AddAsync(newUser);
            await _uow.CompleteAsync(); 

            var newClient = new Client
            {
                UserID = newUser.UserID,
                NIP    = nip,
                Phone  = phone
            };
            
            await _uow.Clients.AddAsync(newClient);
            await _uow.CompleteAsync(); 

            return new ClientDto
            {
                ClientID = newClient.ClientID,
                UserID = newUser.UserID,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Phone = newClient.Phone ?? "",
                NIP = newClient.NIP
            };
        }

        public async Task DeleteClientAsync(int clientId)
        {
            var clients = await _uow.Clients.FindWithIncludesAsync(x => x.ClientID == clientId, x => x.User);
            var client = clients.FirstOrDefault();
            if (client == null) return;

            var user = client.User;
            _uow.Clients.Delete(client);
            if (user != null)
            {
                _uow.AppUsers.Delete(user);
            }
            await _uow.CompleteAsync();
        }
    }
}
