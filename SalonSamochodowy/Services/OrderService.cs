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

        public async Task<int> GetActiveOrdersCountAsync()
        {
            return await _uow.SalesOrders.CountAsync(o => o.Status != OrderStatuses.Finished && o.Status != OrderStatuses.FinishedAlt && o.Status != OrderStatuses.Canceled);
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

        public async Task<SalesOrder?> GetOrderForEditAsync(int orderId)
        {
            var orders = await _uow.SalesOrders.FindWithIncludesAsync(
                o => o.OrderID == orderId,
                o => o.Vehicle.Trim.Model,
                o => o.Vehicle.Engine,
                o => o.Client.User,
                o => o.Worker.User);
            return orders.FirstOrDefault();
        }

        public async Task UpdateOrderDetailsAsync(int orderId, int workerId, DateTime orderDate, decimal finalPrice, string status)
        {
            var order = await _uow.SalesOrders.GetByIdAsync(orderId);
            if (order == null) throw new Exception("Nie znaleziono zamówienia.");

            var oldStatus = order.Status;

            order.WorkerID   = workerId;
            order.OrderDate  = orderDate;
            order.FinalPrice = finalPrice;
            order.Status     = status;
            _uow.SalesOrders.Update(order);

            if (oldStatus != status)
            {
                var vehicle = await _uow.Vehicles.GetByIdAsync(order.VehicleID);
                if (vehicle != null)
                {
                    if (status == OrderStatuses.Finished || status == OrderStatuses.FinishedAlt)
                        vehicle.Status = "Sprzedany";
                    else if (status == OrderStatuses.Canceled)
                        vehicle.Status = "Dostępny";
                    else
                        vehicle.Status = "Zarezerwowany";

                    _uow.Vehicles.Update(vehicle);
                }
            }

            await _uow.CompleteAsync();
        }

        public async Task<IEnumerable<SalesOrder>> GetAllOrdersWithDetailsAsync()
        {
            return await _uow.SalesOrders.GetAllWithIncludesAsync(
                o => o.Vehicle.Trim.Model,
                o => o.Vehicle.Engine,
                o => o.Client.User,
                o => o.Worker.User,
                o => o.Dealership);
        }
    }
}
