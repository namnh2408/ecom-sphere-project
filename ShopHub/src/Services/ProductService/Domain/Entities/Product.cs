using ShopHub.Domain.Base;
using ShopHub.Domain.Abstractions;

namespace ShopHub.Services.ProductService.Domain.Entities;

/// <summary>
/// Aggregate Root: Product
/// Quản lý thông tin sản phẩm và các sự kiện liên quan
/// </summary>
public class Product : BaseAggregateRoot
{
    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Mô tả sản phẩm
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Giá sản phẩm
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Số lượng tồn kho
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// SKU (Stock Keeping Unit)
    /// </summary>
    public string? Sku { get; private set; }

    /// <summary>
    /// ID danh mục
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// URL hình ảnh
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Tổng số lượng sold
    /// </summary>
    public int TotalSold { get; private set; }

    /// <summary>
    /// Constructor private (phải dùng factory method)
    /// </summary>
    private Product()
    {
    }

    /// <summary>
    /// Factory method để tạo sản phẩm mới
    /// </summary>
    public static Product Create(
        string name,
        decimal price,
        int stock,
        Guid categoryId,
        string? description = null,
        string? sku = null,
        string? imageUrl = null)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Stock = stock,
            CategoryId = categoryId,
            Description = description,
            Sku = sku,
            ImageUrl = imageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            TotalSold = 0
        };

        // Thêm domain event
        product.AddDomainEvent(new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Price));

        return product;
    }

    /// <summary>
    /// Cập nhật thông tin sản phẩm
    /// </summary>
    public void Update(
        string? name = null,
        string? description = null,
        decimal? price = null,
        string? imageUrl = null)
    {
        if (name != null && string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (price.HasValue && price.Value < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        var oldPrice = Price;

        if (name != null)
            Name = name;

        if (description != null)
            Description = description;

        if (price.HasValue && price.Value != oldPrice)
        {
            Price = price.Value;
            AddDomainEvent(new ProductPriceChangedEvent(
                Id,
                oldPrice,
                price.Value));
        }

        if (imageUrl != null)
            ImageUrl = imageUrl;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật tồn kho
    /// </summary>
    public void UpdateStock(int quantity)
    {
        if (Stock + quantity < 0)
            throw new InvalidOperationException("Insufficient stock");

        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Thêm số lượng vào tồn kho
    /// </summary>
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Giảm tồn kho
    /// </summary>
    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (Stock < quantity)
            throw new InvalidOperationException("Insufficient stock");

        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đặt tồn kho tuyệt đối
    /// </summary>
    public void SetStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(quantity));

        Stock = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate sản phẩm
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate sản phẩm
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Tăng số lượng sold
    /// </summary>
    public void IncrementSoldCount(int quantity = 1)
    {
        TotalSold += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Domain Event: Product Created
/// </summary>
public class ProductCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public int Version => 1;

    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal Price { get; }

    public ProductCreatedEvent(Guid productId, string productName, decimal price)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        ProductId = productId;
        ProductName = productName;
        Price = price;
    }
}

/// <summary>
/// Domain Event: Product Price Changed
/// </summary>
public class ProductPriceChangedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public int Version => 1;

    public Guid ProductId { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }

    public ProductPriceChangedEvent(Guid productId, decimal oldPrice, decimal newPrice)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}