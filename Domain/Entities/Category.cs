using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Category : BaseEntity
    {
        public required string Name { get; set; }
        [JsonIgnore]
        public IList<Product>? Products { get; set; }
    }
}
