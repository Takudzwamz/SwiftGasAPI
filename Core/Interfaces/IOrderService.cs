using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities.OrderAggregate;

namespace Core.Interfaces
{
    public interface IOrderService
    {
         Task<object> CreateOrderAsync(string buyerEmail, int deliveryMethod, string basketId, Address shippingAddress);
         Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail);
         Task<Order> GetOrderByIdAsync(int id, string buyerEmail);
        Task<Order> GetOrderById(int id);
        Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync();
        Task<IReadOnlyList<Order>> GetOrders();
    }
}