using Microsoft.EntityFrameworkCore;
using VendorHub.DTOs.OrderDto;
using VendorHub.DTOs.sharedDto;
using VendorHub.Events;
using VendorHub.Extensions;
using VendorHub.Models;
using VendorHub.Repository;

namespace VendorHub.Services
{
    public class OrderService : IOrderService
    {

        private readonly IGeneralRepository<Order> _orderRepository;
        private readonly IGeneralRepository<OrderItem> _orderItemRepository;
        private readonly IGeneralRepository<Product> _productRepository;
        private readonly ILogger<OrderService> _logger;

        private readonly IEventQueue<OrderPlacedEvent> _orderPlacedEventQueue;
        private readonly IEventQueue<OrderStatusChangedEvent> _statusChangedEventQueue;

        public OrderService(
            IGeneralRepository<Order> orderRepository,
            IGeneralRepository<OrderItem> orderItemRepository,
            IGeneralRepository<Product> productRepository,
            ILogger<OrderService> logger,

            IEventQueue<OrderPlacedEvent> orderPlacedEventQueue,
            IEventQueue<OrderStatusChangedEvent> statusChangedEventQueue
         )
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _logger = logger;

            _orderPlacedEventQueue = orderPlacedEventQueue;
            _statusChangedEventQueue = statusChangedEventQueue;
        }

        #region Vendor Read
        public async Task<GeneralResponse<VendorOrderDto>> GetVendorOrderByIdAsync(int orderId, int vendorId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository
                .GetBy(o => o.Id == orderId)
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Items.Where(oi => oi.Product.VendorId == vendorId))
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null || !order.Items.Any())
            {
                _logger.LogWarningWithContext("Vendor order fetch failed or access denied for OrderId: {OrderId}, VendorId: {VendorId}",
                    new { OrderId = orderId, VendorId = vendorId });
                return GeneralResponse<VendorOrderDto>.NotFound("Access denied or order not found");
            }

            var dto = MapToVendorOrderDto(order, vendorId);
            return GeneralResponse<VendorOrderDto>.Succeeded(dto, "Order retrieved successfully");
        }

        public async Task<GeneralResponse<PagedResult<VendorOrderDto>>> GetVendorOrdersAsync(int vendorId, int page, int pageSize, string? statusFilter = null, CancellationToken cancellationToken = default)
        {
            var query = _orderRepository
                .GetAllAsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Items.Where(oi => oi.Product.VendorId == vendorId))
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Items.Any(oi => oi.Product.VendorId == vendorId));

            if (!string.IsNullOrEmpty(statusFilter))
                if (Enum.TryParse<OrderStatus>(statusFilter, out var filterEnum))
                {
                    query = query.Where(o => o.Items.Any(oi => oi.Product.VendorId == vendorId && oi.ItemStatus == filterEnum));
                }

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var vendorOrderDtos = orders.Select(o => MapToVendorOrderDto(o, vendorId)).ToList();

            var result = new PagedResult<VendorOrderDto>
            {
                Items = vendorOrderDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return GeneralResponse<PagedResult<VendorOrderDto>>.Succeeded(result, "Orders retrieved successfully");
        }

        public async Task<GeneralResponse<VendorOrdersStatsDto>> GetVendorOrdersStatsAsync(int vendorId, CancellationToken cancellationToken = default)
        {
            var statsQuery = await _orderItemRepository.GetAllAsNoTracking()
                .Where(oi => oi.Product.VendorId == vendorId)
                .GroupBy(oi => 1)
                .Select(g => new VendorOrdersStatsDto
                {
                    TotalOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                    TotalRevenue = g.Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                    PendingRevenue = g.Where(oi => oi.ItemStatus == OrderStatus.Pending).Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                    PendingOrders = g.Where(oi => oi.ItemStatus == OrderStatus.Pending).Select(oi => oi.OrderId).Distinct().Count(),
                    ShippedOrders = g.Where(oi => oi.ItemStatus == OrderStatus.Shipped).Select(oi => oi.OrderId).Distinct().Count(),
                    DeliveredOrders = g.Where(oi => oi.ItemStatus == OrderStatus.Delivered).Select(oi => oi.OrderId).Distinct().Count(),
                    CancelledOrders = g.Where(oi => oi.ItemStatus == OrderStatus.Cancelled).Select(oi => oi.OrderId).Distinct().Count()
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (statsQuery == null)
            {
                return GeneralResponse<VendorOrdersStatsDto>.Succeeded(new VendorOrdersStatsDto(), "No orders found to generate statistics.");
            }

            return GeneralResponse<VendorOrdersStatsDto>.Succeeded(statsQuery, "Stats computed successfully");
        }

        public async Task<GeneralResponse<List<OrderStatusInfoDto>>> GetAvailableStatusesAsync(CancellationToken cancellationToken = default)
        {
            var statuses = Enum.GetNames(typeof(OrderStatus))
                .Select(name => new OrderStatusInfoDto
                {
                    Value = name
                })
                .ToList();

            return await Task.FromResult(GeneralResponse<List<OrderStatusInfoDto>>.Succeeded(statuses, "Order statuses retrieved successfully."));
        }

        public async Task<GeneralResponse> UpdateOrderStatusAsync(int orderId, int vendorId, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
        {
            var targetStatus = dto.Status;

            var vendorItems = await _orderItemRepository.GetAll()
                .Where(oi => oi.OrderId == orderId && oi.Product.VendorId == vendorId)
                .ToListAsync(cancellationToken);

            Order currentOrder = (await _orderRepository.GetByIdAsync(orderId, cancellationToken))!;

            if (!vendorItems.Any())
            {
                if (currentOrder == null)
                {
                    _logger.LogWarningWithContext("Order status update failed. Order not found. OrderId: {OrderId}", new { OrderId = orderId });
                    return GeneralResponse.NotFound("The specified order could not be found.");
                }

                _logger.LogWarningWithContext("Unauthorized order status update attempt. OrderId: {OrderId}, VendorId: {VendorId}",
                    new { OrderId = orderId, VendorId = vendorId });

                return GeneralResponse.Forbidden("You do not have permission to modify items in this order.");
            }

            var statusGroups = vendorItems.GroupBy(i => i.ItemStatus);
            int quantityMoved = 0;

            foreach (var group in statusGroups)
            {
                var oldStatusQuantity = group.Sum(i => i.Quantity);
                quantityMoved += oldStatusQuantity;
                AdjustOrderCounters(currentOrder, group.Key, oldStatusQuantity, isAdding: false);
            }

            AdjustOrderCounters(currentOrder, targetStatus, quantityMoved, isAdding: true);

            foreach (var item in vendorItems)
                item.ItemStatus = targetStatus;

            RefreshGlobalOrderStatusAsync(currentOrder);

            try
            {
                _orderRepository.Update(currentOrder);
                await _orderRepository.SaveAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogErrorWithContext("Concurrency crash updating status for OrderId: {OrderId}", ex, new { OrderId = orderId });
                return GeneralResponse.Error("The state of the order was changed by another process. Please reload and try again.");
            }

            string statusString = targetStatus.ToString();
            _logger.LogInfoWithContext("Successfully mutated order status flags via explicit vendor request pipeline.",
            new { OrderId = orderId, MutatedStatus = statusString, ActionByVendor = vendorId });

            var statusEvent = new OrderStatusChangedEvent(currentOrder.CustomerId, currentOrder.Id, statusString);
            await _statusChangedEventQueue.EnqueueAsync(statusEvent, cancellationToken);

            return GeneralResponse.Succeeded($"Your item statuses were successfully updated to {dto.Status}");
        }

        private void AdjustOrderCounters(Order order, OrderStatus status, int quantity, bool isAdding)
        {
            int adjustment = isAdding ? quantity : -quantity;

            _ = status switch
            {
                OrderStatus.Shipped => order.ShippedItemsCount += adjustment,
                OrderStatus.Delivered => order.DeliveredItemsCount += adjustment,
                OrderStatus.Cancelled => order.CancelledItemsCount += adjustment,
                _ => 0
            };

        }
        #endregion

        #region Customer Order Lifecycle
        public async Task<GeneralResponse<OrderDetailsDto>> CreateOrderAsync(CreateOrderDto dto, int customerId, CancellationToken cancellationToken)
        {
            if (dto.CartItems == null || !dto.CartItems.Any())
                return GeneralResponse<OrderDetailsDto>.InvalidInput("Cart is empty");

            using var transaction = await _orderItemRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                CartValidationResult cartResult = await ValidateAndProcessCartAsync(dto, cancellationToken);

                if (!cartResult.IsSuccess)
                {
                    _logger.LogWarningWithContext("Checkout cart structure validation failure dropped for CustomerId: {CustomerId}",
                        new { Reason = cartResult.Error }, customerId);
                    return GeneralResponse<OrderDetailsDto>.InvalidInput(cartResult.Error ?? "Validation failed");
                }

                var orderItems = cartResult.Items;
                var order = await BuildAndSaveOrderAsync(dto, customerId, orderItems, cartResult.TotalPrice, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var vendorSummaries = orderItems
                    .GroupBy(i => i.Product.VendorId)
                    .Select(g => new VendorOrderSummary(
                        VendorId: g.Key,
                        TotalItemsCount: g.Sum(i => i.Quantity),
                        Subtotal: g.Sum(i => i.PriceAtPurchase * i.Quantity)
                        ))
                    .ToList();

                var orderEvent = new OrderPlacedEvent(order, vendorSummaries);
                await _orderPlacedEventQueue.EnqueueAsync(orderEvent, cancellationToken);

                return GeneralResponse<OrderDetailsDto>.Created(MapToDetailsDto(order), "Order created successfully");

            }
            catch (DbUpdateConcurrencyException)
            {
                return GeneralResponse<OrderDetailsDto>.Error("The inventory status changed while processing your cart. Please try checkout again.");
            }
        }

        public async Task<GeneralResponse<OrderDetailsDto>> GetOrderAsync(int orderId, int customerId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository
            .GetAll()
            .Where(o => o.Id == orderId && o.CustomerId == customerId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
                return GeneralResponse<OrderDetailsDto>.NotFound("Order not found");

            return GeneralResponse<OrderDetailsDto>.Succeeded(MapToDetailsDto(order));
        }

        public async Task<GeneralResponse<IEnumerable<OrderDetailsDto>>> GetCustomerOrdersAsync(int customerId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository
                .GetAllAsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return GeneralResponse<IEnumerable<OrderDetailsDto>>.Succeeded(orders.Select(MapToDetailsDto));
        }
        #endregion

        #region Helpers & Data Mappers

        private void RefreshGlobalOrderStatusAsync(Order order)
        {
            OrderStatus nextGlobalStatus = order switch
            {
                _ when order.DeliveredItemsCount == order.TotalItemsCount => OrderStatus.Delivered,
                _ when order.CancelledItemsCount == order.TotalItemsCount => OrderStatus.Cancelled,
                _ when order.ShippedItemsCount + order.DeliveredItemsCount == order.TotalItemsCount => OrderStatus.Shipped,
                _ => OrderStatus.Pending
            };

            if (order.Status != nextGlobalStatus)
            {
                order.Status = nextGlobalStatus;
                order.UpdatedAt = DateTime.UtcNow;
            }
        }

        private VendorOrderDto MapToVendorOrderDto(Order order, int vendorId)
        {
            var vendorItems = ExtractVendorItems(order, vendorId);

            return new VendorOrderDto
            {
                OrderId = order.Id,
                CustomerName = order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.SecondName}" : "Anonymous Client",
                PhoneNumber = order.Customer?.PhoneNumber ?? order.PhoneNumber,
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
                .Where(oi => oi.Product != null && oi.Product.VendorId == vendorId)
                .Select(oi => new VendorOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name ?? "Unknown Product",
                    ProductImage = oi.Product.ImgUrl ?? string.Empty,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase,
                    Status = oi.ItemStatus
                })
                .ToList();
        }

        private async Task<CartValidationResult> ValidateAndProcessCartAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
        {
            if (dto.CartItems == null || !dto.CartItems.Any())
                return CartValidationResult.Failed("Cart contains no items.");

            var productIds = dto.CartItems.Select(ci => ci.ProductId).Distinct().ToList();

            var products = await _productRepository.GetBy(p => productIds.Contains(p.Id)).ToDictionaryAsync(P => P.Id);

            var orderItems = new List<OrderItem>();
            decimal totalPrice = 0;

            foreach (var cartItem in dto.CartItems)
            {
                string validationError = ValidationItem(cartItem, out var product, products);

                if (validationError != null)
                    return CartValidationResult.Failed(validationError);

                product.Quantity -= cartItem.Quantity;
                totalPrice += product.Price * cartItem.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = product.Price,
                    ItemStatus = OrderStatus.Pending,
                    Product = product
                });
            }

            return CartValidationResult.Success(orderItems, totalPrice);
        }

        private string? ValidationItem(CartItemDto cartItem, out Product product, Dictionary<int, Product> products)
        {
            product = null!;

            if (!products.TryGetValue(cartItem.ProductId, out product))
                return $"Product identifier {cartItem.ProductId} details not found";

            if (product.Status != ProductStatus.REVIEWED)
                return $"Product '{product.Name}' is currently unavailable";

            if (product.Quantity <= 0 || product.Quantity < cartItem.Quantity)
                return $"Insufficient stock remaining for '{product.Name}'";

            return null;
        }

        private async Task<Order> BuildAndSaveOrderAsync(CreateOrderDto dto, int customerId, List<OrderItem> orderItems, decimal precalculatedTotalPrice, CancellationToken cancellationToken = default)
        {
            var totalQuantity = orderItems.Sum(i => i.Quantity);

            var order = new Order
            {
                CustomerId = customerId,
                TotalPrice = precalculatedTotalPrice,
                Status = OrderStatus.Pending,
                DeliveryAddress = dto.DeliveryAddress,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                Items = orderItems,
                TotalItemsCount = totalQuantity,
                ShippedItemsCount = 0,
                DeliveredItemsCount = 0,
                CancelledItemsCount = 0
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveAsync(cancellationToken);
            return order;
        }

        private static OrderDetailsDto MapToDetailsDto(Order order)
        {
            return new OrderDetailsDto
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                DeliveryAddress = order.DeliveryAddress ?? string.Empty,
                PhoneNumber = order.PhoneNumber ?? string.Empty,
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
