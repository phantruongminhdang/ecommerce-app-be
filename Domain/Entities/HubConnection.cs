using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public partial class HubConnection : BaseEntity
    {
        public string ConnectionId { get; set; } = null!;
        public Guid UserId { get; set; }
        public virtual required ApplicationUser ApplicationUser { get; set; }
    }
}
