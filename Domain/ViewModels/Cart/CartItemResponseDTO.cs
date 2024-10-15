using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Cart
{
    public class CartItemResponseDTO
    {
        public Guid Id { get; set; }
        [Required]
        public Guid CartId { get; set; }
        [Required]
        [StringLength(40, ErrorMessage = "Tên sản phẩm dài tối đa 40 ký tự")]
        public required string ProductName { get; set; }
        public string? imageUrl { get; set; }
        public required double Price { get; set; }
        [Required]
        public required string Code { get; set; }
        public int Quantity { get; set; }
    }
}
