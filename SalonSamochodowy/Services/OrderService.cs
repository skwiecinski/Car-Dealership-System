using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;

        public OrderService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<SalesOrder> CreateOrderAsync(SalesOrder order)
        {
            await _uow.SalesOrders.AddAsync(order);
            
            var vehicle = await _uow.Vehicles.GetByIdAsync(order.VehicleID);
            if (vehicle != null)
            {
                if (order.Status == "Zrealizowane" || order.Status == "Sfinalizowane")
                    vehicle.Status = "Sprzedany";
                else if (order.Status == "Anulowane")
                    vehicle.Status = "Dostępny";
                else
                    vehicle.Status = "Zarezerwowany";
                    
                _uow.Vehicles.Update(vehicle);
            }

            await _uow.CompleteAsync();
            return order;
        }

        public async Task<IEnumerable<SalesOrder>> GetRecentOrdersAsync(int count)
        {
            return await _uow.SalesOrders.GetTopOrderedDescAsync(o => o.OrderDate, count);
        }

        public async Task<int> GetOrdersCountSinceAsync(DateTime since)
        {
            return await _uow.SalesOrders.CountAsync(o => o.OrderDate >= since);
        }

        public async Task<IEnumerable<SalesOrder>> GetOrdersSinceAsync(DateTime since)
        {
            return await _uow.SalesOrders.FindAsync(o => o.OrderDate >= since);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _uow.SalesOrders.GetByIdAsync(orderId);
            if (order == null) throw new Exception("Nie znaleziono zamówienia.");

            order.Status = newStatus;
            _uow.SalesOrders.Update(order);

            var vehicle = await _uow.Vehicles.GetByIdAsync(order.VehicleID);
            if (vehicle != null)
            {
                if (newStatus == "Zrealizowane" || newStatus == "Sfinalizowane")
                    vehicle.Status = "Sprzedany";
                else if (newStatus == "Anulowane")
                    vehicle.Status = "Dostępny";
                else
                    vehicle.Status = "Zarezerwowany";

                _uow.Vehicles.Update(vehicle);
            }

            await _uow.CompleteAsync();
        }
    }
}
