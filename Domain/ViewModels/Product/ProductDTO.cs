using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.ViewModels.Product
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(40, ErrorMessage = "Tên phân loại dài tối đa 40 ký tự")]
        public required string CategoryName { get; set; }
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
