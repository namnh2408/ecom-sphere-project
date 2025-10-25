using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShopHub.Common.Results;
using ShopHub.Infrastructure.Caching;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Handlers;
using ShopHub.Services.ProductService.Application.Queries;
using ShopHub.Services.ProductService.Domain.Entities;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Tests.Application;

/// <summary>
/// Unit tests cho GetRelatedProductsQueryHandler
/// </summary>
[TestClass]
public class GetRelatedProductsQueryHandlerTests
{
    private Mock<IProductRepository> _mockProductRepository = null!;
    private Mock<IProductQueryRepository> _mockQueryRepository = null!;
    private Mock<ICacheService> _mockCacheService = null!;
    private Mock<ILogger<GetRelatedProductsQueryHandler>> _mockLogger = null!;
    private GetRelatedProductsQueryHandler _handler = null!;

    private Guid _mainProductId;
    private Guid _categoryId;

    [TestInitialize]
    public void Setup()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockQueryRepository = new Mock<IProductQueryRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<GetRelatedProductsQueryHandler>>();
        _handler = new GetRelatedProductsQueryHandler(
            _mockProductRepository.Object,
            _mockQueryRepository.Object,
            _mockCacheService.Object,
            _mockLogger.Object);

        _mainProductId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();
    }

    [TestMethod]
    public async Task Handle_WithValidProductId_ReturnsRelatedProducts()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(_mainProductId, limit: 5);
        var mainProduct = Product.Create("Main Product", 100m, 50, _categoryId);
        mainProduct.GetType().GetProperty(nameof(mainProduct.Id))?.SetValue(mainProduct, _mainProductId);

        var relatedProducts = new List<Product>
        {
            Product.Create("Related 1", 110m, 40, _categoryId),
            Product.Create("Related 2", 120m, 30, _categoryId),
            Product.Create("Related 3", 90m, 60, _categoryId)
        };

        _mockCacheService
            .Setup(x => x.GetAsync<List<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ProductResponseDto>?)null);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_mainProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mainProduct);

        _mockQueryRepository
            .Setup(x => x.GetByCategoryAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(relatedProducts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(3, result.Data.Count);
        Assert.IsTrue(result.Data.All(p => p.CategoryId == _categoryId));
    }

    [TestMethod]
    public async Task Handle_WithInvalidProductId_ReturnsNotFoundFailure()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(Guid.NewGuid(), limit: 5);

        _mockCacheService
            .Setup(x => x.GetAsync<List<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ProductResponseDto>?)null);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Product not found", result.Message);
        Assert.AreEqual(404, result.StatusCode);
    }

    [TestMethod]
    public async Task Handle_WhenProductIsDeleted_ReturnsNotFoundFailure()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(_mainProductId, limit: 5);
        var deletedProduct = Product.Create("Deleted Product", 100m, 50, _categoryId);
        deletedProduct.GetType().GetProperty(nameof(deletedProduct.Id))?.SetValue(deletedProduct, _mainProductId);
        
        // Mark as deleted (assuming there's a method or property to do this)
        deletedProduct.GetType().GetProperty(nameof(deletedProduct.IsDeleted))?.SetValue(deletedProduct, true);

        _mockCacheService
            .Setup(x => x.GetAsync<List<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ProductResponseDto>?)null);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_mainProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedProduct);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Product not found", result.Message);
    }

    [TestMethod]
    public async Task Handle_WhenCacheExists_ReturnsCachedResult()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(_mainProductId, limit: 5);
        var cachedData = new List<ProductResponseDto>
        {
            new ProductResponseDto { ProductId = Guid.NewGuid(), Name = "Cached Related Product" }
        };

        _mockCacheService
            .Setup(x => x.GetAsync<List<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Data!.Count);
        Assert.AreEqual("Cached Related Product", result.Data.First().Name);

        // Verify repository was NOT called (because cache hit)
        _mockProductRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithLimitParameter_ReturnsOnlyRequestedCount()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(_mainProductId, limit: 3);
        var mainProduct = Product.Create("Main Product", 100m, 50, _categoryId);
        mainProduct.GetType().GetProperty(nameof(mainProduct.Id))?.SetValue(mainProduct, _mainProductId);

        var manyProducts = Enumerable.Range(1, 10)
            .Select(i => Product.Create($"Related {i}", 100m + i * 10, 50, _categoryId))
            .ToList();

        _mockCacheService
            .Setup(x => x.GetAsync<List<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ProductResponseDto>?)null);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_mainProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mainProduct);

        _mockQueryRepository
            .Setup(x => x.GetByCategoryAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manyProducts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3, result.Data!.Count);
    }

    [TestMethod]
    public async Task Handle_ExcludesCurrentProductFromResults()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(_mainProductId, limit: 5);
        var mainProduct = Product.Create("Main Product", 100m, 50, _categoryId);
        mainProduct.GetType().GetProperty(nameof(mainProduct.Id))?.SetValue(mainProduct, _mainProductId);

        var allCategoryProducts = new List<Product>
        {
            mainProduct, // Same as query product
            Product.Create("Related 1", 110m, 40, _categoryId),
            Product.Create("Related 2", 120m, 30, _categoryId)
        };

        _mockCacheService
            .Setup(x => x.GetAsync<List<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ProductResponseDto>?)null);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_mainProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mainProduct);

        _mockQueryRepository
            .Setup(x => x.GetByCategoryAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCategoryProducts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Data!.Count);
        Assert.IsFalse(result.Data!.Any(p => p.ProductId == _mainProductId));
    }

    [TestMethod]
    public async Task Handle_OnException_ReturnsFailureResult()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(_mainProductId, limit: 5);
        _mockCacheService
            .Setup(x => x.GetAsync<List<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache service error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("Error getting related products"));
    }

    [TestMethod]
    public async Task Handle_WithNoRelatedProducts_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(_mainProductId, limit: 5);
        var mainProduct = Product.Create("Main Product", 100m, 50, _categoryId);
        mainProduct.GetType().GetProperty(nameof(mainProduct.Id))?.SetValue(mainProduct, _mainProductId);

        _mockCacheService
            .Setup(x => x.GetAsync<List<ProductResponseDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ProductResponseDto>?)null);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_mainProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mainProduct);

        _mockQueryRepository
            .Setup(x => x.GetByCategoryAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Data!.Count);
    }
}