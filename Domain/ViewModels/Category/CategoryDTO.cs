using System.ComponentModel.DataAnnotations;

namespace Domain.ViewModels.Category
{
    public class CategoryDTO
    {
        [Required]
        [StringLength(40, ErrorMessage = "Tên phân loại dài tối đa 40 ký tự")]
        public required string Name { get; set; }
    }
}
