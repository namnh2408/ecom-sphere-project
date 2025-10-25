using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShopHub.Common.Pagination;
using ShopHub.Common.Results;
using ShopHub.Infrastructure.Caching;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Handlers;
using ShopHub.Services.ProductService.Application.Queries;
using ShopHub.Services.ProductService.Domain.Entities;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Tests.Application;

/// <summary>
/// Unit tests cho GetAllProductsQueryHandler
/// </summary>
[TestClass]
public class GetAllProductsQueryHandlerTests
{
    private Mock<IProductQueryRepository> _mockQueryRepository = null!;
    private Mock<ICacheService> _mockCacheService = null!;
    private Mock<ILogger<GetAllProductsQueryHandler>> _mockLogger = null!;
    private GetAllProductsQueryHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockQueryRepository = new Mock<IProductQueryRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<GetAllProductsQueryHandler>>();
        _handler = new GetAllProductsQueryHandler(_mockQueryRepository.Object, _mockCacheService.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task Handle_WithValidRequest_ReturnsPaginatedProducts()
    {
        // Arrange
        var query = new GetAllProductsQuery(pageNumber: 1, pageSize: 10);
        var products = new List<Product>
        {
            Product.Create("Product 1", 100m, 50, Guid.NewGuid()),
            Product.Create("Product 2", 200m, 30, Guid.NewGuid()),
            Product.Create("Product 3", 150m, 20, Guid.NewGuid())
        };

        _mockCacheService
            .Setup(x => x.GetAsync<PaginatedList<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaginatedList<ProductResponseDto>?)null);

        _mockQueryRepository
            .Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _mockQueryRepository
            .Setup(x => x.GetTotalCountAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(3, result.Data.Items.Count);
        Assert.AreEqual(1, result.Data.PageNumber);
        Assert.AreEqual(10, result.Data.PageSize);
        Assert.AreEqual(3, result.Data.TotalCount);
    }

    [TestMethod]
    public async Task Handle_WhenCacheExists_ReturnsCachedResult()
    {
        // Arrange
        var query = new GetAllProductsQuery(pageNumber: 1, pageSize: 10);
        var cachedData = new PaginatedList<ProductResponseDto>(
            new List<ProductResponseDto>
            {
                new ProductResponseDto { ProductId = Guid.NewGuid(), Name = "Cached Product" }
            },
            1, 1, 10);

        _mockCacheService
            .Setup(x => x.GetAsync<PaginatedList<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Items.Count);
        Assert.AreEqual("Cached Product", result.Data.Items.First().Name);

        // Verify that cache was checked
        _mockCacheService.Verify(
            x => x.GetAsync<PaginatedList<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify that repository was NOT called (because cache hit)
        _mockQueryRepository.Verify(
            x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var query = new GetAllProductsQuery(pageNumber: 2, pageSize: 5);
        var products = Enumerable.Range(0, 5)
            .Select(i => Product.Create($"Product {i}", 100m + i * 10, 50 - i, Guid.NewGuid()))
            .ToList();

        _mockCacheService
            .Setup(x => x.GetAsync<PaginatedList<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaginatedList<ProductResponseDto>?)null);

        _mockQueryRepository
            .Setup(x => x.GetAllAsync(2, 5, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _mockQueryRepository
            .Setup(x => x.GetTotalCountAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Data!.PageNumber);
        Assert.AreEqual(5, result.Data.PageSize);
        Assert.AreEqual(15, result.Data.TotalCount);
    }

    [TestMethod]
    public async Task Handle_OnException_ReturnsFailureResult()
    {
        // Arrange
        var query = new GetAllProductsQuery();
        _mockCacheService
            .Setup(x => x.GetAsync<PaginatedList<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("Error getting products"));
    }

    [TestMethod]
    public async Task Handle_WithEmptyResults_ReturnsEmptyPaginatedList()
    {
        // Arrange
        var query = new GetAllProductsQuery(pageNumber: 1, pageSize: 10);

        _mockCacheService
            .Setup(x => x.GetAsync<PaginatedList<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaginatedList<ProductResponseDto>?)null);

        _mockQueryRepository
            .Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        _mockQueryRepository
            .Setup(x => x.GetTotalCountAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Data!.Items.Count);
        Assert.AreEqual(0, result.Data.TotalCount);
    }
}