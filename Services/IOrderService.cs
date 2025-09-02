using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IOrderService
    {
        IEnumerable<OrdersOutputOTD> GetAllOrders(int uid);
        IEnumerable<OrdersOutputOTD> GetOrderById(int oid, int uid);
        IEnumerable<Order> GetOrderByUserId(int uid);
        void DeleteOrder(int oid);

        void AddOrder(Order order);
        void UpdateOrder(Order order);
       void PlaceOrder(List<OrderItemDTO> items, int uid);
        bool UpdateStatus(int orderId, int uid, OrderStatus status);
        bool Cancel(int orderId, int uid);
    }
}