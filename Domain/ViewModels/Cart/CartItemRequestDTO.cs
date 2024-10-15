using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Cart
{
    public class CartItemRequestDTO
    {
        [Required]
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
