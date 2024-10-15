using System.Text.Json.Serialization;

namespace Domain.Entities.Base
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = new Guid();

        public bool IsDeleted { get; set; } = false;
    }
}
