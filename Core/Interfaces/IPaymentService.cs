using Core.Entities;
using Core.Entities.OrderAggregate;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IPaymentService
    {
        Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId);
        Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentId);
        Task<Order> UpdateOrderPaymentFailed(string paymentIntentId);
        Task<object> CreatepayFastOrder(string email, int deliveryMethodId, string basketId, CustomerBasket basket);
    }
}