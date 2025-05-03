public class OrderService : IOrderService
{
    private readonly ElasticsearchRepository<Order> _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ElasticsearchRepository<Order> repository, 
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        // Basic validation
        if (order.Items == null || !order.Items.Any())
        {
            _logger.LogWarning("Cannot create order without items");
            return null;
        }

        // Set order details
        order.OrderDate = DateTime.UtcNow;
        order.CreatedDate = DateTime.UtcNow;
        order.LastModifiedDate = DateTime.UtcNow;
        order.Status = "Pending";

        // Calculate total amount
        order.TotalAmount = order.Items.Sum(item => item.UnitPrice * item.Quantity);

        // Save order
        var success = await _repository.IndexAsync(order);
        return success ? order : null;
    }

    public async Task<Order> GetOrderByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Order>> GetOrdersByCustomerIdAsync(string customerId)
    {
        try 
        {
            // This is a simplified implementation. In a real-world scenario, 
            // you'd want a more efficient way to query by customer ID
            var result = await _repository.FilterAsync(
                order => order.CustomerId == customerId);

            return result.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer");
            return new List<Order>();
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus)
    {
        try 
        {
            var order = await _repository.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found");
                return false;
            }

            order.Status = newStatus;
            order.LastModifiedDate = DateTime.UtcNow;

            return await _repository.UpdateAsync(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return false;
        }
    }
}