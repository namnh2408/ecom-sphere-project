using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Gateway.WebApi.Models;
using Catalog.Application.Commands.Products;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Products;

namespace Gateway.WebApi.Controllers;

/// <summary>
/// Product catalog management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Products")]
[Produces("application/json")]
[Consumes("application/json")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all products with pagination
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of all active products.
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based indexing)</param>
    /// <param name="pageSize">Number of records per page (1-100)</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Products retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedProductResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllProductsQuery(pageNumber, pageSize);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="productId">Product's unique ID</param>
    /// <returns>Product details</returns>
    /// <response code="200">Product found</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid productId)
    {
        var query = new GetProductByIdQuery(productId);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Search products by name, SKU, or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="pageNumber">Page number (1-based indexing)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <returns>Search results</returns>
    /// <response code="200">Search completed</response>
    /// <response code="400">Invalid search term</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedProductResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new SearchProductsQuery(searchTerm ?? "", pageNumber, pageSize);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Filter products by category, price, status, and stock
    /// </summary>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="status">Product status filter (Active, Inactive, Discontinued)</param>
    /// <param name="inStock">Filter by stock availability</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Filtered products</returns>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(PaginatedProductResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? inStock = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new FilterProductsQuery(categoryId, minPrice, maxPrice, status, inStock, pageNumber, pageSize);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Products in category</returns>
    /// <response code="200">Products retrieved</response>
    /// <response code="404">Category not found</response>
    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(PaginatedProductResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCategory(
        Guid categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsByCategoryQuery(categoryId, pageNumber, pageSize);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="command">Product creation data</param>
    /// <returns>Created product ID</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return CreatedAtAction(nameof(GetById), new { productId = result.Value }, result.Value);
    }

    /// <summary>
    /// Update product information
    /// </summary>
    /// <param name="productId">Product ID to update</param>
    /// <param name="command">Updated product data</param>
    /// <returns>Success message</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPut("{productId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid productId, [FromBody] UpdateProductCommand command)
    {
        var updatedCommand = command with { ProductId = productId };
        var result = await mediator.Send(updatedCommand);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Product updated successfully" });
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="command">Stock update data</param>
    /// <returns>Success message</returns>
    /// <response code="200">Stock updated successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("{productId:guid}/stock")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateStock(Guid productId, [FromBody] UpdateProductStockCommand command)
    {
        var updatedCommand = command with { ProductId = productId };
        var result = await mediator.Send(updatedCommand);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Stock updated successfully" });
    }

    /// <summary>
    /// Deactivate a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Product deactivated successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("{productId:guid}/deactivate")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(Guid productId)
    {
        var command = new DeactivateProductCommand(productId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Product deactivated successfully" });
    }

    /// <summary>
    /// Reactivate a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Product reactivated successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("{productId:guid}/reactivate")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Reactivate(Guid productId)
    {
        var command = new ReactivateProductCommand(productId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Product reactivated successfully" });
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid productId)
    {
        var command = new DeleteProductCommand(productId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Product deleted successfully" });
    }
}