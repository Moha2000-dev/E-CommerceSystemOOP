using E_CommerceSystem.Exceptions;
using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class OrderRepo : IOrderRepo
    {
        public ApplicationDbContext _context;

        public OrderRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Order> GetAllOrders()
        {
            try
            {
                return _context.Orders.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public Order GetOrderById(int oid)
        {
            try
            {
                return _context.Orders.FirstOrDefault(o => o.OID == oid);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public IEnumerable<Order> GetOrderByUserId(int uid)
        {
            try
            {
                return _context.Orders.Where(o => o.UID == uid).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void AddOrder(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void DeleteOrder(int oid)
        {
            try
            {
                var order = GetOrderById(oid);
                if (order != null)
                {
                    _context.Orders.Remove(order);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void UpdateOrder(Order order)
        {
            try
            {
                _context.Orders.Update(order);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("The order was modified by another user. Please refresh and try again.");
            }
        }
        public bool UpdateStatus(int orderId, int uid, OrderStatus status)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.OID == orderId && o.UID == uid);
                if (order is null) return false;
                order.Status = status;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public bool Cancel(int orderId, int uid)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.OID == orderId && o.UID == uid);
                if (order is null) return false;

                // NOTE: stock restore & rules belong in the service; this just flips the flag.
                order.Status = OrderStatus.Cancelled;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }


    }
}
