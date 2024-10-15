using Domain.Entities.Base;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Cart : BaseEntity
    {
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = [];
        public virtual Customer? Customer { get; set; }
    }
}
