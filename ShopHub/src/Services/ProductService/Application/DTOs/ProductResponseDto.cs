namespace ShopHub.Services.ProductService.Application.DTOs;

/// <summary>
/// DTO để trả về thông tin sản phẩm
/// </summary>
public class ProductResponseDto
{
    /// <summary>
    /// ID sản phẩm
    /// </summary>
    public Guid ProductId { get; set; }

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
    /// SKU
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
    public bool IsActive { get; set; }

    /// <summary>
    /// Rating trung bình
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Số lượng reviews
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Tổng số lượng đã bán
    /// </summary>
    public int TotalSold { get; set; }
}