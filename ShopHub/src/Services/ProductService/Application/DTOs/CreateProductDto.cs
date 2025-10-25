namespace ShopHub.Services.ProductService.Application.DTOs;

/// <summary>
/// DTO để tạo sản phẩm mới
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả sản phẩm
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Giá sản phẩm
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Số lượng tồn kho
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// SKU (Stock Keeping Unit)
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// ID danh mục
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// URL hình ảnh
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    public bool IsActive { get; set; } = true;
}