using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Common.Pagination;
using ShopHub.Services.ProductService.Application.Commands;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Queries;

namespace ShopHub.Services.ProductService.Presentation.Controllers;

/// <summary>
/// API Controller cho Product Service
/// Sử dụng CQRS pattern với MediatR
/// </summary>
[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IMediator mediator,
        ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Tạo sản phẩm mới
    /// </summary>
    /// <param name="dto">Product creation DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid product data</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new product");

        var command = new CreateProductCommand(
            dto.Name,
            dto.Price,
            dto.Stock,
            dto.CategoryId,
            dto.Description,
            dto.Sku,
            dto.ImageUrl);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return CreatedAtAction(nameof(GetProduct), new { id = result.Data?.ProductId }, result.Data);
    }

    /// <summary>
    /// Lấy thông tin sản phẩm theo ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    /// <response code="200">Product found</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting product {id}");

        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Cập nhật sản phẩm
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Product update DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid product data</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Updating product {id}");

        var command = new UpdateProductCommand(
            id,
            dto.Name,
            dto.Description,
            dto.Price,
            dto.ImageUrl);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Xóa sản phẩm
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Deleting product {id}");

        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return NoContent();
    }

    /// <summary>
    /// Tìm kiếm sản phẩm
    /// </summary>
    /// <param name="searchTerm">Search keyword</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDirection">Sort direction (asc/desc)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products</returns>
    /// <response code="200">Products found</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Searching products with term: {searchTerm}");

        var query = new SearchProductsQuery(searchTerm, pageNumber, pageSize, sortBy, sortDirection);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Lấy tất cả sản phẩm (phân trang)
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDirection">Sort direction (asc/desc)</param>
    /// <param name="isActive">Filter by active status (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Products retrieved successfully</response>
    [HttpGet("all")]
    [ProducesResponseType(typeof(PaginatedList<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Getting all products, page: {pageNumber}, pageSize: {pageSize}");

        var query = new GetAllProductsQuery(pageNumber, pageSize, sortBy, sortDirection, isActive);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Lấy sản phẩm liên quan (cùng danh mục)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="limit">Number of related products to return (default: 5)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of related products</returns>
    /// <response code="200">Related products found</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id:guid}/related")]
    [ProducesResponseType(typeof(List<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelatedProducts(
        [FromRoute] Guid id,
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Getting related products for: {id}");

        var query = new GetRelatedProductsQuery(id, limit);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Cập nhật số tồn kho sản phẩm
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="quantity">Quantity to add/subtract/set</param>
    /// <param name="operation">Stock operation: Add(0), Subtract(1), Set(2)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Stock updated successfully</response>
    /// <response code="400">Invalid stock operation or insufficient stock</response>
    /// <response code="404">Product not found</response>
    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductStock(
        [FromRoute] Guid id,
        [FromQuery] int quantity,
        [FromQuery] StockOperation operation = StockOperation.Add,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Updating stock for product {id}: operation={operation}, quantity={quantity}");

        var command = new UpdateProductStockCommand(id, quantity, operation);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            var statusCode = result.StatusCode > 0 ? result.StatusCode : 400;
            return StatusCode(statusCode, new { message = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }
}