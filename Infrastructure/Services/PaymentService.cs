using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PayFast;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Order = Core.Entities.OrderAggregate.Order;
using OrderItem = Core.Entities.OrderAggregate.OrderItem;
using Product = Core.Entities.Product;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly PayFastSettings payFastSettings;

        public PaymentService(IOptions<PayFastSettings> payFastSettings, IBasketRepository basketRepository, IUnitOfWork unitOfWork, IConfiguration config)
        {
            this.payFastSettings = payFastSettings.Value;
            _config = config;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
            if (basketId != null)
            {


                var basket = await _basketRepository.GetBasketAsync(basketId);

                if (basket == null) return null;

                var shippingPrice = 0m;

                if (basket.DeliveryMethodId.HasValue)
                {
                    var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
                        .GetByIdAsync((int)basket.DeliveryMethodId);
                    shippingPrice = deliveryMethod.Price;
                }

                foreach (var item in basket.Items)
                {
                    var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                    if (item.Price != productItem.Price)
                    {
                        item.Price = productItem.Price;
                    }
                }

                var service = new PaymentIntentService();

                PaymentIntent intent;

                if (string.IsNullOrEmpty(basket.PaymentIntentId))
                {
                    var options = new PaymentIntentCreateOptions
                    {
                        Amount = (long)basket.Items.Sum(i => i.Quantity * (i.Price * 100)) + (long)shippingPrice * 100,
                        Currency = "zar",
                        PaymentMethodTypes = new List<string> { "card" }
                    };
                    intent = await service.CreateAsync(options);
                    basket.PaymentIntentId = intent.Id;
                    basket.ClientSecret = intent.ClientSecret;
                }
                else
                {
                    var options = new PaymentIntentUpdateOptions
                    {
                        Amount = (long)basket.Items.Sum(i => i.Quantity * (i.Price * 100)) + (long)shippingPrice * 100
                    };
                    await service.UpdateAsync(basket.PaymentIntentId, options);
                }

                await _basketRepository.UpdateBasketAsync(basket);

                return basket;
            }

            else
            {
                return null;
            }
        }

        public async Task<object> CreatepayFastOrder(string email, int deliveryMethodId, string basketId)
        {
            // get the basket
            var basket = await _basketRepository.GetBasketAsync(basketId); //not working 
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

            var onceOffRequest = new PayFastRequest(this.payFastSettings.PassPhrase)
            {
                // Merchant Details
                merchant_id = this.payFastSettings.MerchantId,
                merchant_key = this.payFastSettings.MerchantKey,
                return_url = this.payFastSettings.ReturnUrl,
                cancel_url = this.payFastSettings.CancelUrl,
                notify_url = this.payFastSettings.NotifyUrl,

                // Buyer Details
                email_address = email,

                // Transaction Details
                m_payment_id = Guid.NewGuid().ToString(),
                amount = 3000,// (double)subtotal,
                item_name = "Payment",
                item_description = "Some details about the once off payment",

                // Transaction Options
                email_confirmation = false,
                confirmation_address = email
            };

            var redirectUrl = $"{this.payFastSettings.ProcessUrl}{onceOffRequest.ToString()}";

            var data = new { PaymentLink = redirectUrl, CallbackLink = this.payFastSettings.ReturnUrl };
            return data;
        }

        public async Task<Order> UpdateOrderPaymentFailed(string paymentIntentId)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

            if (order == null) return null;

            order.Status = OrderStatus.PaymentFailed;
            await _unitOfWork.Complete();

            return order;
        }

        public async Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentId)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

            if (order == null) return null;

            order.Status = OrderStatus.PaymentRecevied;
            _unitOfWork.Repository<Order>().Update(order);

            await _unitOfWork.Complete();

            return order;
        }

    }
}