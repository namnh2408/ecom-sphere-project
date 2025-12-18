using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Gateway.WebApi.Models;
using Catalog.Application.Commands.Categories;
using Catalog.Application.DTOs;
using Catalog.Application.Queries.Categories;

namespace Gateway.WebApi.Controllers;

/// <summary>
/// Product category management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Categories")]
[Produces("application/json")]
[Consumes("application/json")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all categories with pagination
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of all active categories.
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based indexing)</param>
    /// <param name="pageSize">Number of records per page (1-100)</param>
    /// <returns>Paginated list of categories</returns>
    /// <response code="200">Categories retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedCategoryResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllCategoriesQuery(pageNumber, pageSize);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all categories with product count
    /// </summary>
    /// <remarks>
    /// Retrieves all categories along with the count of products in each category.
    /// Useful for displaying category navigation with product counts.
    /// </remarks>
    /// <returns>List of categories with product counts</returns>
    /// <response code="200">Categories with counts retrieved successfully</response>
    [HttpGet("with-count")]
    [ProducesResponseType(typeof(IEnumerable<CategoryWithCountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWithCount()
    {
        var query = new GetCategoriesWithCountQuery();
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="categoryId">Category's unique ID</param>
    /// <returns>Category details</returns>
    /// <response code="200">Category found</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{categoryId:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid categoryId)
    {
        var query = new GetCategoryByIdQuery(categoryId);
        var result = await mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="command">Category creation data</param>
    /// <returns>Created category ID</returns>
    /// <response code="201">Category created successfully</response>
    /// <response code="400">Invalid input data or duplicate name</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return CreatedAtAction(nameof(GetById), new { categoryId = result.Value }, result.Value);
    }

    /// <summary>
    /// Update category information
    /// </summary>
    /// <param name="categoryId">Category ID to update</param>
    /// <param name="command">Updated category data</param>
    /// <returns>Success message</returns>
    /// <response code="200">Category updated successfully</response>
    /// <response code="404">Category not found</response>
    /// <response code="400">Invalid input data or duplicate name</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPut("{categoryId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid categoryId, [FromBody] UpdateCategoryCommand command)
    {
        var updatedCommand = command with { CategoryId = categoryId };
        var result = await mediator.Send(updatedCommand);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Category updated successfully" });
    }

    /// <summary>
    /// Deactivate a category
    /// </summary>
    /// <remarks>
    /// Makes category inactive without deleting it. Products in inactive categories are still available.
    /// </remarks>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Category deactivated successfully</response>
    /// <response code="404">Category not found</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("{categoryId:guid}/deactivate")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(Guid categoryId)
    {
        var command = new DeactivateCategoryCommand(categoryId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Category deactivated successfully" });
    }

    /// <summary>
    /// Reactivate a category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Category reactivated successfully</response>
    /// <response code="404">Category not found</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpPost("{categoryId:guid}/reactivate")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Reactivate(Guid categoryId)
    {
        var command = new ReactivateCategoryCommand(categoryId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Category reactivated successfully" });
    }

    /// <summary>
    /// Delete a category (soft delete)
    /// </summary>
    /// <remarks>
    /// Soft deletes the category. Products in the category are not deleted.
    /// </remarks>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Category deleted successfully</response>
    /// <response code="404">Category not found</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpDelete("{categoryId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid categoryId)
    {
        var command = new DeleteCategoryCommand(categoryId);
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Category deleted successfully" });
    }
}