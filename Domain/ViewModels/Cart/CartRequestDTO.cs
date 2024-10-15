using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Cart
{
    public class CartRequestDTO
    {
        [Required]
        public required Guid CustomerId { get; set; }


    }
}
