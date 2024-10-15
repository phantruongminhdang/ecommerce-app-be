using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Order
{
    public class OrderRequestDTO
    {
        [Required]
        public required Guid CustomerId { get; set; }
        [Required]
        public required string Address { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedDeliveryDate { get; set; } = DateTime.Now.AddDays(10);
        public double TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Waiting;
        public string? Note { get; set; }

    }
}
