
namespace E_CommerceSystem.Services
{
    public interface IOrderSummaryService
    {
        Task<object> GetSummaryAsync(DateTime from, DateTime to);
    }
}