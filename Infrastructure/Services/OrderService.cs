using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Options;
using PayFast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly PayFastSettings payFastSettings;
        public OrderService(IOptions<PayFastSettings> payFastSettings, IBasketRepository basketRepo, IUnitOfWork unitOfWork, IPaymentService paymentService)
        {
            this.payFastSettings = payFastSettings.Value;
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
            _basketRepo = basketRepo;
        }

        public class OrderRet : Order
        {
            public string PaymentURl { get; set; }
        }
        public async Task<object> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress)
        {
            // get basket from the repo
            var basket = await _basketRepo.GetBasketAsync(basketId); //working

            // get items from the product repo
            var items = new List<OrderItem>();
            foreach (var item in basket.Items)
            {
                var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                string photo = productItem.Photos.FirstOrDefault(x => x.IsMain)?.PictureUrl;
                string save = item.PictureUrl.Split("Content/")[1];
                photo = (photo != null) ? photo : save;
                var itemOrdered = new ProductItemOrdered(productItem.Id, productItem.Name, photo
                    );
                var orderItem = new OrderItem(itemOrdered, productItem.Price, item.Quantity);
                items.Add(orderItem);
            }

            // get delivery method from repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

            // calc subtotal
            var subtotal = items.Sum(item => item.Price * item.Quantity);

            // check to see if order exists
            //var spec = new OrderByPaymentIntentIdSpecification(basket.PaymentIntentId);
            //var existingOrder = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

            //if (existingOrder != null)
            //{
            //    _unitOfWork.Repository<Order>().Delete(existingOrder);
            //    await _paymentService.CreateOrUpdatePaymentIntent(basket.PaymentIntentId);
            //}

            // create order
            var order = new Order(items, buyerEmail, shippingAddress, deliveryMethod, subtotal, basket.PaymentIntentId);
            _unitOfWork.Repository<Order>().Add(order);

            // save to db
            var result = await _unitOfWork.Complete();

            //clear the cart
            basket.Items = new List<BasketItem>();
            var basketUpdate = await _basketRepo.UpdateBasketAsync(basket);

            if (result <= 0) return null;


            var onceOffRequest = new PayFastRequest(this.payFastSettings.PassPhrase)
            {
                // Merchant Details
                merchant_id = this.payFastSettings.MerchantId,
                merchant_key = this.payFastSettings.MerchantKey,
                return_url = this.payFastSettings.ReturnUrl,
                cancel_url = this.payFastSettings.CancelUrl,
                notify_url = this.payFastSettings.NotifyUrl,

                // Buyer Details
                email_address = buyerEmail,

                // Transaction Details
                m_payment_id = Guid.NewGuid().ToString(),
                amount = (double)subtotal,
                item_name = "Payment",
                item_description = "Some details about the once off payment",

                // Transaction Options
                email_confirmation = false,
                confirmation_address = buyerEmail
            };

            var redirectUrl = $"{this.payFastSettings.ProcessUrl}{onceOffRequest.ToString()}";

            OrderRet orderRet = new OrderRet()
            {
                PaymentURl = redirectUrl,
                BuyerEmail = order.BuyerEmail,
                DeliveryMethod = order.DeliveryMethod,
                Id = order.Id,
                OrderDate = order.OrderDate,
                OrderItems = order.OrderItems,
                PaymentIntentId = order.PaymentIntentId,
                ShipToAddress = order.ShipToAddress,
                Status = order.Status,
                Subtotal = order.Subtotal
            };
            // return order
            return orderRet;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            return await _unitOfWork.Repository<DeliveryMethod>().ListAllAsync();
        }

        public async Task<Order> GetOrderById(int id)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(id);

            return await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
        }

        public async Task<Order> GetOrderByIdAsync(int id, string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(id, buyerEmail);

            return await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Order>> GetOrders()
        {
            return await _unitOfWork.Repository<Order>().ListAllAsync();

        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);

            return await _unitOfWork.Repository<Order>().ListAsync(spec);
        }
    }
}