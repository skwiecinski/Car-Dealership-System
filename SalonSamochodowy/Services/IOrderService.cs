using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Services
{
    public interface IOrderService
    {
        Task<SalesOrder> CreateOrderAsync(SalesOrder order);
        Task<IEnumerable<SalesOrder>> GetRecentOrdersAsync(int count);
        Task<int> GetActiveOrdersCountAsync();
        Task<int> GetOrdersCountSinceAsync(DateTime since);
        Task<IEnumerable<SalesOrder>> GetOrdersSinceAsync(DateTime since);
        Task UpdateOrderStatusAsync(int orderId, string newStatus);

        Task<SalesOrder?> GetOrderForEditAsync(int orderId);
        Task UpdateOrderDetailsAsync(int orderId, int workerId, DateTime orderDate, decimal finalPrice, string status);
        Task<IEnumerable<SalesOrder>> GetAllOrdersWithDetailsAsync();
    }
}
