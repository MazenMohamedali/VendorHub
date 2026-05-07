using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VendorHub.DTOs.OrderDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Hubs;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGeneralRepository<Order> _orderRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<Notification> _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OrderService(
            IGeneralRepository<Order> orderRepository,
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<Notification> notificationRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        #region Create Order
        public async Task<GeneralResponse<OrderDetailsDto>> CreateOrderAsync(CreateOrderDto dto, int customerId)
        {
            if (!dto.CartItems.Any())
                return new GeneralResponse<OrderDetailsDto>().Failed("Cart is empty");

            var (orderItems, totalPrice, validationMessage) = await ValidateAndProcessCartAsync(dto);

            if (orderItems == null)
                return new GeneralResponse<OrderDetailsDto>().Failed(validationMessage);

            var order = await BuildAndSaveOrderAsync(dto, customerId, orderItems);

            //_ = SendRealTimeNotificationsAsync(order);
            await SendRealTimeNotificationsAsync(order);

            return new GeneralResponse<OrderDetailsDto>().Succeeded(MapToDetailsDto(order));
        }
        #endregion

        #region Get Methods

        public async Task<GeneralResponse<OrderDetailsDto>> GetOrderAsync(int orderId, int customerId)
        {
            var order = await _orderRepository
                .GetAll()
                .Where(o => o.Id == orderId && o.CustomerId == customerId)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (order == null)
                return new GeneralResponse<OrderDetailsDto>().Failed("Order not found");

            return new GeneralResponse<OrderDetailsDto>().Succeeded(MapToDetailsDto(order));
        }

        public async Task<GeneralResponse<IEnumerable<OrderDetailsDto>>> GetCustomerOrdersAsync(int customerId, int pageNumber = 1, int pageSize = 10)
        {
            var orders = await _orderRepository
                .GetAll()
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderDtos = orders.Select(MapToDetailsDto).ToList();

            return new GeneralResponse<IEnumerable<OrderDetailsDto>>()
                .Succeeded(orderDtos);
        }

        #endregion

        #region Private Helpers
        private async Task<(List<OrderItem>? Items, decimal TotalPrice, string? Error)> ValidateAndProcessCartAsync(CreateOrderDto dto)
        {
            var orderItems = new List<OrderItem>();
            decimal totalPrice = 0;

            foreach (var cartItem in dto.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);

                string? validateProduct = ValidProduct(product!);
                if (!string.IsNullOrEmpty(validateProduct))
                    return (null, 0, validateProduct);

                decimal curPrice = await QuantityAndTotalPriceHandle(product, cartItem);
                if (curPrice == 0)
                    return (null, 0, $"Insufficient stock for '{product.Name}'");

                totalPrice += curPrice;

                orderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = cartItem.Price
                });
            }

            return (orderItems, totalPrice, null);
        }

        private string? ValidProduct(Product product)
        {
            if (product == null)
                return "Product is null";

            if (product.Status != ProductStatus.REVIEWED)
                return $"Product '{product.Name}' is not available";

            if (product.Quantity <= 0)
                return $"Product '{product.Name}' is out of stock";

            return null;
        }

        public async Task<decimal> QuantityAndTotalPriceHandle(Product product, CartItemDto cartItem)
        {
            if (product.Quantity < cartItem.Quantity)
                return 0;

            product.Quantity -= cartItem.Quantity;
            await _productRepository.UpdateAsync(product);

            return cartItem.Price * cartItem.Quantity;
        }

        private async Task<Order> BuildAndSaveOrderAsync(CreateOrderDto dto, int customerId, List<OrderItem> orderItems)
        {
            var order = new Order
            {
                CustomerId = customerId,
                TotalPrice = orderItems.Sum(i => i.PriceAtPurchase * i.Quantity),
                Status = OrderStatus.Pending,
                DeliveryAddress = dto.DeliveryAddress,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                Items = orderItems
            };

            await _orderRepository.AddAsync(order);
            await _productRepository.SaveAsync();
            await _orderRepository.SaveAsync();

            return order;
        }

        private async Task SendRealTimeNotificationsAsync(Order order)
        {
            var fullOrder = await _orderRepository
                .GetAll()
                .Where(o => o.Id == order.Id)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (fullOrder == null) return;

            var vendorGroups = fullOrder.Items
                .GroupBy(i => i.Product?.VendorId)
                .Where(g => g.Key.HasValue);

            foreach (var vendorGroup in vendorGroups)
            {
                var vendorId = vendorGroup.Key.Value;
                await SendVendorNotificationAsync(vendorId, vendorGroup.ToList(), fullOrder.Id);
                await SaveVendorNotificationsAsync(vendorId, vendorGroup.ToList(), fullOrder.Id);
            }

            await SendCustomerConfirmationAsync(fullOrder.Id, fullOrder.CustomerId);
        }

        private async Task SendVendorNotificationAsync(int vendorId, List<OrderItem> items, int orderId)
        {
            var itemList = string.Join(", ",
                items.Select(i => $"{i.Quantity}x {i.Product?.Name}"));

            var notification = new
            {
                Title = "🎉 New Purchase!",
                Message = $"Customer bought: {itemList}",
                Type = NotificationType.NewPurchase.ToString(),
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            };

            await _hubContext.Clients
                .Group($"vendor-{vendorId}")
                .SendAsync("ReceiveNotification", notification);
        }

        private async Task SaveVendorNotificationsAsync(int vendorId, List<OrderItem> items, int orderId)
        {
            var itemList = string.Join(", ",
                items.Select(i => $"{i.Quantity}x {i.Product?.Name}"));

            var notification = new Notification
            {
                UserId = vendorId,
                Title = "New Purchase!",
                Message = $"Customer bought: {itemList}",
                Type = NotificationType.NewPurchase,
                OrderId = orderId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveAsync();
        }

        private async Task SendCustomerConfirmationAsync(int orderId, int customerId)
        {
            var confirmation = new
            {
                OrderId = orderId,
                Status = OrderStatus.Confirmed.ToString(),
                Message = "Your order has been placed successfully!",
                UpdatedAt = DateTime.UtcNow
            };

            await _hubContext.Clients
                .Group($"user-{customerId}")
                .SendAsync("OrderStatusChanged", confirmation);
        }

        private static OrderDetailsDto MapToDetailsDto(Order order)
        {
            return new OrderDetailsDto
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                DeliveryAddress = order.DeliveryAddress,
                PhoneNumber = order.PhoneNumber,
                CreatedAt = order.CreatedAt,
                Items = order.Items?.Select(i => new OrderItemDto
                {
                    Id = i.ProductId,
                    Name = i.Product?.Name ?? "Unknown Product",
                    Quantity = i.Quantity,
                    Price = i.PriceAtPurchase
                }).ToList() ?? new List<OrderItemDto>()
            };
        }
        #endregion

    }
}
