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
            var clients = await _uow.Clients.GetAllAsync();
            var dtos = new List<ClientDto>();

            foreach (var c in clients)
            {
                var user = await _uow.AppUsers.GetByIdAsync(c.UserID);
                if (user != null)
                {
                    dtos.Add(new ClientDto
                    {
                        ClientID = c.ClientID,
                        UserID = user.UserID,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Phone = c.Phone ?? "",
                        NIP = c.NIP
                    });
                }
            }
            return dtos;
        }

        public async Task<ClientDto?> GetClientByIdAsync(int clientId)
        {
            var c = await _uow.Clients.GetByIdAsync(clientId);
            if (c == null) return null;

            var user = await _uow.AppUsers.GetByIdAsync(c.UserID);
            if (user == null) return null;

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
            var roleKlient = (await _uow.AppRoles.FindAsync(r => r.RoleName == "Klient")).FirstOrDefault();
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
            await _uow.CompleteAsync(); // Wymagane by uzyskać newUser.UserID

            var newClient = new Client
            {
                UserID = newUser.UserID,
                NIP    = nip,
                Phone  = phone
            };
            
            await _uow.Clients.AddAsync(newClient);
            await _uow.CompleteAsync(); // Wymagane by zapisać nowego klienta

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
    }
}
