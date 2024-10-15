using Domain.Entities;
using Domain.Enums;
using Domain.ViewModels.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Order
{
    public class OrderResponseDTO
    {
        public Guid Id { get; set; }
        [Required]
        public required Guid CustomerId { get; set; }
        [Required]
        public required string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? DeliveryDate { get; set; }
        public double TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? Note { get; set; }
        public required UserResponseDTO UserResponseDTO { get; set; }
        public required ICollection<OrderItemResponseDTO> OrderItemResponseDTOs { get; set; }
    }
}
