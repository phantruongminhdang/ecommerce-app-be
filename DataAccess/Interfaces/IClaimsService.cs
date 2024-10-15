namespace DataAccess.Interfaces
{
    public interface IClaimsService
    {
        public Guid GetCurrentUserId { get; }
    }
}
