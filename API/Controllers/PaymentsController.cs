using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Order = Core.Entities.OrderAggregate.Order;

namespace API.Controllers
{
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private readonly string _whSecret;
        private readonly ILogger<IPaymentService> _logger;
        private readonly IMapper _mapper;
        // private readonly IBasketRepository _basketRepository;

        public PaymentsController(IPaymentService paymentService, ILogger<IPaymentService> logger, IConfiguration config, IMapper mapper) //IBasketRepository basketRepository,
        {
            //  _basketRepository = basketRepository;
            _logger = logger;
            _paymentService = paymentService;
            _mapper = mapper;
            _whSecret = config.GetSection("StripeSettings:WhSecret").Value;
        }

        [Authorize]
        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if (basket == null) return BadRequest(new ApiResponse(400, "Problem with your basket"));

            return basket;
        }

        [Authorize]
        [HttpPost("pay")]
        public async Task<ActionResult> CreatepayFastOrder(OrderDto orderDto)
        {
            var email = HttpContext.User.RetrieveEmailFromPrincipal();
            // var address = _mapper.Map<AddressDto, Address>(orderDto.ShipToAddress);

            var result = await _paymentService.CreatepayFastOrder(email, orderDto.DeliveryMethodId, orderDto.BasketId);

            if (result == null) return BadRequest(new ApiResponse(400, "Problem with your payment"));

            return Ok(result);
        }

        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _whSecret);

            PaymentIntent intent;
            Order order;

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogInformation("Payment Succeeded: ", intent.Id);
                    order = await _paymentService.UpdateOrderPaymentSucceeded(intent.Id);
                    _logger.LogInformation("Order updated to payment received: ", order.Id);
                    break;
                case "payment_intent.payment_failed":
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogInformation("Payment Failed: ", intent.Id);
                    order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                    _logger.LogInformation("Payment Failed: ", order.Id);
                    break;
            }

            return new EmptyResult();
        }


        /// <summary>
        /// On receiving the payment notification from PayFast, return a header 200 to prevent further retries.
        /// If no 200 response is returned the notification will be re-sent immediately, then after 10 minutes and then at exponentially longer 
        /// intervals until eventually stopping.
        /// </summary>
        /// <param name="notify"></param>
        /// <returns></returns>
        [HttpPost("payfastnotifywebhook")]
        public async Task PayFastWebhook(PayFast.PayFastNotify notify)
        {
            const string callbackScheme = "myapp";

            // Get parameters to send back to the callback
            var qs = new Dictionary<string, string>
                {
                    { "access_token", "access" },
                    { "refresh_token", "token" ?? string.Empty }
                };

            // Build the result url
            var url = callbackScheme + "://#" + string.Join(
                "&",
                qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
                .Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

            // Redirect to final url
            Request.HttpContext.Response.Redirect(url);

        }
    }
}