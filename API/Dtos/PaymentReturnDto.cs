using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class PaymentReturnDto
    {
        public string PaymentLink { get; set; }
        public string CallbackLink { get; set; }
    }
}
