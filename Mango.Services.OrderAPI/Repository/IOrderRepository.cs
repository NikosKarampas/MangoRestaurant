using Mango.Services.OrderAPI.Models;

namespace Mango.Services.OrderAPI.Repository
{
    public interface IOrderRepository
    {
        Task<bool> AddOrderAsync(OrderHeader orderHeader);

        Task UpdateOrderPaymentStatusAsync(int orderHeaderId, bool paid);
    }
}
