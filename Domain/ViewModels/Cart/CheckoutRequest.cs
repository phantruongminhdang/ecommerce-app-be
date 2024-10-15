using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Cart
{
    public class CheckoutRequest
    {
        public required string Address { get; set; }
        public string? Note { get; set; }
    }
}
