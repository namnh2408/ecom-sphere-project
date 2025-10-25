using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MediatR;
using ShopHub.Common.Pagination;
using ShopHub.Common.Results;
using ShopHub.Services.ProductService.Application.Commands;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Queries;
using ShopHub.Services.ProductService.Presentation.Controllers;

namespace ShopHub.Services.ProductService.Tests.Application;

/// <summary>
/// Unit tests cho ProductsController
/// </summary>
[TestClass]
public class ProductsControllerTests
{
    private Mock<IMediator> _mockMediator = null!;
    private Mock<ILogger<ProductsController>> _mockLogger = null!;
    private ProductsController _controller = null!;

    private Guid _productId;
    private Guid _categoryId;

    [TestInitialize]
    public void Setup()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockMediator.Object, _mockLogger.Object);

        _productId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();
    }

    #region GetAllProducts Tests

    [TestMethod]
    public async Task GetAllProducts_WithValidRequest_ReturnsOkWithPaginatedList()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var paginatedList = new PaginatedList<ProductResponseDto>(
            new List<ProductResponseDto>
            {
                new ProductResponseDto { ProductId = Guid.NewGuid(), Name = "Product 1", Price = 100m },
                new ProductResponseDto { ProductId = Guid.NewGuid(), Name = "Product 2", Price = 200m }
            },
            2, pageNumber, pageSize);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedList<ProductResponseDto>>.Success(paginatedList));

        // Act
        var result = await _controller.GetAllProducts(pageNumber, pageSize, cancellationToken: CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        
        var returnedData = okResult.Value as PaginatedList<ProductResponseDto>;
        Assert.IsNotNull(returnedData);
        Assert.AreEqual(2, returnedData.Items.Count);
    }

    [TestMethod]
    public async Task GetAllProducts_WhenQueryFails_ReturnsBadRequest()
    {
        // Arrange
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedList<ProductResponseDto>>.Failure("Error retrieving products"));

        // Act
        var result = await _controller.GetAllProducts(cancellationToken: CancellationToken.None);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetAllProducts_WithPaginationParameters_SendsCorrectQuery()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 20;
        var emptyResult = new PaginatedList<ProductResponseDto>(new List<ProductResponseDto>(), 0, pageNumber, pageSize);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedList<ProductResponseDto>>.Success(emptyResult));

        // Act
        await _controller.GetAllProducts(pageNumber, pageSize, cancellationToken: CancellationToken.None);

        // Assert
        _mockMediator.Verify(
            x => x.Send(
                It.Is<GetAllProductsQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetRelatedProducts Tests

    [TestMethod]
    public async Task GetRelatedProducts_WithValidProductId_ReturnsOkWithRelatedProducts()
    {
        // Arrange
        var relatedProducts = new List<ProductResponseDto>
        {
            new ProductResponseDto { ProductId = Guid.NewGuid(), Name = "Related 1", Price = 100m },
            new ProductResponseDto { ProductId = Guid.NewGuid(), Name = "Related 2", Price = 150m }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetRelatedProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ProductResponseDto>>.Success(relatedProducts));

        // Act
        var result = await _controller.GetRelatedProducts(_productId, cancellationToken: CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var returnedData = okResult.Value as List<ProductResponseDto>;
        Assert.IsNotNull(returnedData);
        Assert.AreEqual(2, returnedData.Count);
    }

    [TestMethod]
    public async Task GetRelatedProducts_WhenProductNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetRelatedProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ProductResponseDto>>.Failure("Product not found", 404));

        // Act
        var result = await _controller.GetRelatedProducts(_productId, cancellationToken: CancellationToken.None);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }

    [TestMethod]
    public async Task GetRelatedProducts_WithLimitParameter_SendsCorrectQuery()
    {
        // Arrange
        var limit = 3;
        var emptyResult = new List<ProductResponseDto>();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetRelatedProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ProductResponseDto>>.Success(emptyResult));

        // Act
        await _controller.GetRelatedProducts(_productId, limit, CancellationToken.None);

        // Assert
        _mockMediator.Verify(
            x => x.Send(
                It.Is<GetRelatedProductsQuery>(q => q.ProductId == _productId && q.Limit == limit),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetRelatedProducts_ReturnsEmptyListWhenNoRelated_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyResult = new List<ProductResponseDto>();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetRelatedProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ProductResponseDto>>.Success(emptyResult));

        // Act
        var result = await _controller.GetRelatedProducts(_productId, cancellationToken: CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var returnedData = okResult.Value as List<ProductResponseDto>;
        Assert.IsNotNull(returnedData);
        Assert.AreEqual(0, returnedData.Count);
    }

    #endregion

    #region UpdateProductStock Tests

    [TestMethod]
    public async Task UpdateProductStock_WithAddOperation_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var quantity = 10;
        var operation = StockOperation.Add;
        var updatedProduct = new ProductResponseDto
        {
            ProductId = _productId,
            Name = "Test Product",
            Stock = 60,
            Price = 100m
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateProductStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductResponseDto>.Success(updatedProduct, "Stock updated successfully"));

        // Act
        var result = await _controller.UpdateProductStock(_productId, quantity, operation, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var returnedData = okResult.Value as ProductResponseDto;
        Assert.IsNotNull(returnedData);
        Assert.AreEqual(60, returnedData.Stock);
    }

    [TestMethod]
    public async Task UpdateProductStock_WithInsufficientStock_ReturnsBadRequest()
    {
        // Arrange
        var quantity = 100;
        var operation = StockOperation.Subtract;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateProductStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductResponseDto>.Failure("Insufficient stock", 400));

        // Act
        var result = await _controller.UpdateProductStock(_productId, quantity, operation, CancellationToken.None);

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.IsNotNull(statusCodeResult);
        Assert.AreEqual(400, statusCodeResult.StatusCode);
    }

    [TestMethod]
    public async Task UpdateProductStock_WithProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var quantity = 10;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateProductStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductResponseDto>.Failure("Product not found", 404));

        // Act
        var result = await _controller.UpdateProductStock(_productId, quantity, StockOperation.Add, CancellationToken.None);

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.IsNotNull(statusCodeResult);
        Assert.AreEqual(404, statusCodeResult.StatusCode);
    }

    [TestMethod]
    public async Task UpdateProductStock_SendsCorrectCommandWithParameters()
    {
        // Arrange
        var quantity = 25;
        var operation = StockOperation.Set;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateProductStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductResponseDto>.Success(new ProductResponseDto()));

        // Act
        await _controller.UpdateProductStock(_productId, quantity, operation, CancellationToken.None);

        // Assert
        _mockMediator.Verify(
            x => x.Send(
                It.Is<UpdateProductStockCommand>(cmd => 
                    cmd.ProductId == _productId && 
                    cmd.Quantity == quantity && 
                    cmd.Operation == operation),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task UpdateProductStock_WithSubtractOperation_ReturnsOk()
    {
        // Arrange
        var quantity = 5;
        var operation = StockOperation.Subtract;
        var updatedProduct = new ProductResponseDto
        {
            ProductId = _productId,
            Stock = 45, // 50 - 5
            Price = 100m
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateProductStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductResponseDto>.Success(updatedProduct));

        // Act
        var result = await _controller.UpdateProductStock(_productId, quantity, operation, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(45, (okResult.Value as ProductResponseDto)?.Stock);
    }

    #endregion
}