using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VendorHub.DTOs.OrderDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Hubs;
using VendorHub.Models;
using VendorHub.Repository;
using VendorHub.Services.Caching;

namespace VendorHub.Services
{
    public class OrderService : IOrderService
    {

        private readonly IGeneralRepository<Order> _orderRepository;
        private readonly IGeneralRepository<OrderItem> _orderItemRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly IGeneralRepository<User> _userRepository;
        private readonly IGeneralRepository<Vendor> _vendorRepository;
        private readonly INotificationService _notificationService;

        public OrderService(
            IGeneralRepository<Order> orderRepository,
            IGeneralRepository<OrderItem> orderItemRepository,
            IGeneralRepository<Product> productRepository,
            IGeneralRepository<User> userRepository,
            IGeneralRepository<Vendor> vendorRepository,
            INotificationService notificationService)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _vendorRepository = vendorRepository;
            _notificationService = notificationService;
        }

        #region orders
        public async Task<GeneralResponse<VendorOrderDto>> GetVendorOrderByIdAsync(int orderId, int vendorId)
        {
            var order = await _orderRepository.GetAll()
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Vendor)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return new GeneralResponse<VendorOrderDto>()
                    .Failed("Order not found");

            var hasVendorItems = order.Items.Any(oi => oi.Product.VendorId == vendorId);
            if (!hasVendorItems)
                return new GeneralResponse<VendorOrderDto>()
                    .Failed("Access denied: You don't have items in this order");
            
            var dto = MapToVendorOrderDto(order, vendorId);

            return new GeneralResponse<VendorOrderDto>()
                .Succeeded(dto, "Order retrieved successfully");
        }
        public async Task<GeneralResponse<PagedResult<VendorOrderDto>>> GetVendorOrdersAsync(int vendorId, int page, int pageSize, string? statusFilter = null)
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId);
            if (vendor == null)
            {
                return new GeneralResponse<PagedResult<VendorOrderDto>>()
                    .Failed("Vendor not found");
            }

            var query = _orderRepository.GetAll()
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Vendor)
                .Include(o => o.Customer)
                .Where(o => o.Items.Any(oi => oi.Product.VendorId == vendorId));

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(o => o.Status.ToString() == statusFilter);

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vendorOrderDtos = orders.Select(o => MapToVendorOrderDto(o, vendorId)).ToList();

            var result = new PagedResult<VendorOrderDto>
            {
                Items = vendorOrderDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return new GeneralResponse<PagedResult<VendorOrderDto>>()
                .Succeeded(result, "Orders retrieved successfully");
        }
        public async Task<GeneralResponse<VendorOrdersStatsDto>> GetVendorOrdersStatsAsync(int vendorId)
        {
            var orders = await _orderRepository.GetAll()
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Items.Any(oi => oi.Product.VendorId == vendorId))
                .ToListAsync();

            var stats = new VendorOrdersStatsDto
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
                ShippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped),
                DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                TotalRevenue = CalculateTotalRevenue(orders, vendorId),
                PendingRevenue = CalculatePendingRevenue(orders, vendorId)
            };

            return new GeneralResponse<VendorOrdersStatsDto>().Succeeded(stats);
        }
        public async Task<GeneralResponse> UpdateOrderStatusAsync(int orderId, int vendorId, UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return new GeneralResponse().Failed("Order not found");

            var vendorItems = await _orderItemRepository.GetAll()
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId && oi.Product.VendorId == vendorId)
                .ToListAsync();

            if (!vendorItems.Any())
                return new GeneralResponse().Failed("You don't have items in this order");

            if (!IsValidStatus(dto.Status))
                return new GeneralResponse().Failed("Invalid status");

            order.Status = Enum.Parse<OrderStatus>(dto.Status);
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveAsync();

            await SendStatusUpdateNotification(order, dto.Status);

            return new GeneralResponse().Succeeded($"Order status updated to {dto.Status}");
        }

        #endregion

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
        private VendorOrderDto MapToVendorOrderDto(Order order, int vendorId)
        {
            var vendorItems = ExtractVendorItems(order, vendorId);

            return new VendorOrderDto
            {
                OrderId = order.Id,
                CustomerName = $"{order.Customer.FirstName} {order.Customer.SecondName}",
                PhoneNumber = order.Customer.PhoneNumber,
                DeliveryAddress = order.DeliveryAddress,
                TotalPrice = vendorItems.Sum(i => i.SubTotal),
                Status = order.Status.ToString(),
                OrderDate = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = vendorItems
            };
        }

        private List<VendorOrderItemDto> ExtractVendorItems(Order order, int vendorId)
        {
            return order.Items
                .Where(oi => oi.Product.VendorId == vendorId)
                .Select(oi => new VendorOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImage = oi.Product.ImgUrl,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase
                })
                .ToList();
        }

        private bool IsValidStatus(string status)
        {
            var validStatuses = new[] { "Processing", "Shipped", "Delivered" };
            return validStatuses.Contains(status);
        }

        private decimal CalculateTotalRevenue(List<Order> orders, int vendorId)
        {
            return orders
                .SelectMany(o => o.Items)
                .Where(oi => oi.Product.VendorId == vendorId)
                .Sum(oi => oi.PriceAtPurchase * oi.Quantity);
        }

        private decimal CalculatePendingRevenue(List<Order> orders, int vendorId)
        {
            return orders
                .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing)
                .SelectMany(o => o.Items)
                .Where(oi => oi.Product.VendorId == vendorId)
                .Sum(oi => oi.PriceAtPurchase * oi.Quantity);
        }

        private async Task SendStatusUpdateNotification(Order order, string newStatus)
        {
            var customer = await _userRepository.GetByIdAsync(order.CustomerId);
            if (customer != null)
            {
                var message = $"Order #{order.Id} status updated to: {newStatus}";
                await _notificationService.SendOrderStatusNotificationAsync(
                    order.CustomerId,
                    order.Id,
                    newStatus,
                    message
                );
            }
        }

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
