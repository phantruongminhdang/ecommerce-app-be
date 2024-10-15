using Domain.Enums;
using Domain.ViewModels.Order;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Cart
{
    public class CartResponseDTO
    {
        public Guid Id { get; set; }
        [Required]
        public required Guid CustomerId { get; set; }
        public double TotalPrice => CartItemResponseDTOs.Sum(item => item.Price * item.Quantity);

        public required ICollection<CartItemResponseDTO> CartItemResponseDTOs { get; set; } = [];
    }
}
