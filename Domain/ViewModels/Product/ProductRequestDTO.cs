using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Product
{
    public class ProductRequestDTO
    {
        [Required]
        public Guid CategoryId { get; set; }
        [Required]
        [StringLength(40, ErrorMessage = "Tên sản phẩm dài tối đa 40 ký tự")]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? imageUrl { get; set; }
        [Required]
        public required double Price { get; set; }
        public int Quantity { get; set; }
        [Required]
        public required string Code { get; set; }
    }
}
