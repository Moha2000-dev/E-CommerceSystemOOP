using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;

namespace E_CommerceSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductService _productService;
        private readonly IOrderProductsService _orderProductsService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _email;
        private readonly IUserService _userService;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IOrderRepo orderRepo, IProductService productService, IOrderProductsService orderProductsService, IMapper mapper, IEmailSender email,
                    IUserService userService, ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _mapper = mapper;
            _email = email;
            _userService = userService;
            _logger = logger;
        }

        //get all orders for login user
        // get all orders for logged-in user -> list of lightweight order rows
        public IEnumerable<OrdersOutputOTD> GetAllOrders(int uid)
        {
            var orders = _orderRepo.GetOrderByUserId(uid);
            if (orders == null || !orders.Any())
                return Enumerable.Empty<OrdersOutputOTD>();

            var result = new List<OrdersOutputOTD>();

            foreach (var order in orders)
            {
                var orderProducts = _orderProductsService.GetOrdersByOrderId(order.OID) ?? new List<OrderProducts>();

                // Map each OrderProducts -> OrderItemDTO with AutoMapper,
                // then create your existing OrdersOutputOTD row (keep your current shape).
                foreach (var op in orderProducts)
                {
                    var product = _productService.GetProductById(op.PID);
                    if (product is null) continue;

                    // we already configured: CreateMap<OrderProducts, OrderItemDTO>()
                    var itemDto = _mapper.Map<OrderItemDTO>(op);

                    result.Add(new OrdersOutputOTD
                    {
                        ProductName = itemDto.ProductName,   // from map (or empty string if nav not set)
                        Quantity = itemDto.Quantity,
                        OrderDate = order.OrderDate,
                        TotalAmount = itemDto.Quantity * product.Price
                    });
                }
            }

            return result;
        }

        //get order by order id for the login user
        public IEnumerable<OrdersOutputOTD> GetOrderById(int oid, int uid)
        {
            var order = _orderRepo.GetOrderById(oid);
            if (order == null || order.UID != uid)
                return Enumerable.Empty<OrdersOutputOTD>();

            var items = new List<OrdersOutputOTD>();
            var products = _orderProductsService.GetOrdersByOrderId(oid) ?? new List<OrderProducts>();

            foreach (var op in products)
            {
                var product = _productService.GetProductById(op.PID);
                if (product is null) continue;

                var itemDto = _mapper.Map<OrderItemDTO>(op);

                items.Add(new OrdersOutputOTD
                {
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    OrderDate = order.OrderDate,
                    TotalAmount = itemDto.Quantity * product.Price
                });
            }

            return items;
        }
        // update order status for login user
        public bool UpdateStatus(int orderId, int uid, OrderStatus status)
        {
            var order = _orderRepo.GetOrderById(orderId);
            if (order is null || order.UID != uid) return false;
            if (order.Status == OrderStatus.Cancelled) return false; // no updates after cancel

            order.Status = status;
            _orderRepo.UpdateOrder(order);
            try
            {
                var user = _userService.GetUserById(uid);
                _ = _email.SendAsync(
                    user.Email,
                    $"Order #{order.OID} placed",
                    $"<h3>Thanks for your order #{order.OID}</h3><p>Total: {order.TotalAmount:C}</p>");
            }
            catch { /* optional: log */ }
            return true;
        }
        // cancel order for login user
        public bool Cancel(int orderId, int uid)
        {
            var order = _orderRepo.GetOrderById(orderId);
            if (order == null)
            {
                _logger.LogWarning("User {UserId} tried to cancel non-existing order {OrderId}", uid, orderId);
                return false;
            }

            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("User {UserId} tried to cancel order {OrderId} with status {Status}", uid, orderId, order.Status);
                return false;
            }

            foreach (var op in _orderProductsService.GetOrdersByOrderId(order.OID))
            {
                var product = _productService.GetProductById(op.PID);
                product.Stock += op.Quantity;
                _productService.UpdateProduct(product);
                _logger.LogInformation("Restored stock for Product {ProductId} by {Qty}", op.PID, op.Quantity);
            }

            order.Status = OrderStatus.Cancelled;
            _orderRepo.UpdateOrder(order);

            _logger.LogInformation("User {UserId} cancelled order {OrderId}", uid, orderId);
            return true;
        }


        //get order by user id
        public IEnumerable<Order> GetOrderByUserId(int uid)
        {
            var order = _orderRepo.GetOrderByUserId(uid);
            if (order == null)
                throw new KeyNotFoundException($"order with user ID {uid} not found.");

            return order;
        }


        //delete order by order id
        public void DeleteOrder(int oid)
        {
            var order = _orderRepo.GetOrderById(oid);
            if (order == null)
                throw new KeyNotFoundException($"order with ID {oid} not found.");

            _orderRepo.DeleteOrder(oid);
            throw new Exception($"order with ID {oid} is deleted");
        }
        public void AddOrder(Order order)
        {
            _orderRepo.AddOrder(order);
        }
        public void UpdateOrder(Order order)
        {
            _orderRepo.UpdateOrder(order);
        }

        //Places an order for the given list of items and user ID.
        public void PlaceOrder( List<OrderItemDTO> items, int uid)
        {
            // Temporary variable to hold the currently processed product
            Product existingProduct = null;
            
            decimal TotalPrice, totalOrderPrice = 0; // Variables to hold the total price of each item and the overall order

            OrderProducts orderProducts = null;

            // Validate all items in the order
            for (int i = 0; i < items.Count; i++)
            {
                TotalPrice = 0;
                existingProduct = _productService.GetProductByName(items[i].ProductName);
                if (existingProduct == null)
                    throw new Exception($"{items[i].ProductName} not Found");

                if (existingProduct.Stock < items[i].Quantity)
                    throw new Exception($"{items[i].ProductName} is out of stock");

            }
            // Create a new order for the user
            var order = new Order { UID = uid, OrderDate = DateTime.Now, TotalAmount = 0 };
            AddOrder(order); // Save the order to the database

            // Process each item in the order
            foreach (var item in items)
            {
                // Retrieve the product by its name
                existingProduct = _productService.GetProductByName(item.ProductName);
               
                // Calculate the total price for the current item
                TotalPrice = item.Quantity * existingProduct.Price;

                // Deduct the ordered quantity from the product's stock
                existingProduct.Stock -= item.Quantity;

                // Update the overall total order price
                totalOrderPrice += TotalPrice;

                // Create a relationship record between the order and product
                orderProducts = new OrderProducts {OID = order.OID, PID = existingProduct.PID, Quantity = item.Quantity  };
                _orderProductsService.AddOrderProducts(orderProducts);

                // Update the product's stock in the database
                _productService.UpdateProduct(existingProduct);
            }

            // Update the total amount of the order
            order.TotalAmount = totalOrderPrice;
            UpdateOrder(order);

        }
    }
}
