namespace ShopHub.Services.ProductService.Application.DTOs;

/// <summary>
/// DTO để cập nhật sản phẩm
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// ID sản phẩm
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Tên sản phẩm
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Mô tả sản phẩm
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Giá sản phẩm
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Số lượng tồn kho
    /// </summary>
    public int? Stock { get; set; }

    /// <summary>
    /// URL hình ảnh
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    public bool? IsActive { get; set; }
}