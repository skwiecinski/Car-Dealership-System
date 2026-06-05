using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalonSamochodowy.Services
{
    public class ClientDto
    {
        public int ClientID { get; set; }
        public int UserID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? NIP { get; set; }
        
        public bool IsCompany => !string.IsNullOrWhiteSpace(NIP);
        public string FullName => string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName} {LastName}";
    }

    public interface IClientService
    {
        Task<IEnumerable<ClientDto>> GetAllClientsAsync();
        Task<ClientDto?> GetClientByIdAsync(int clientId);
        Task<bool> EmailExistsAsync(string email);
        Task<ClientDto> CreateClientAsync(string firstName, string lastName, string email, string phone, string? nip);
        Task DeleteClientAsync(int clientId);
    }
}
