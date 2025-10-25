using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShopHub.Common.Results;
using ShopHub.Domain.Abstractions;
using ShopHub.Services.ProductService.Application.Commands;
using ShopHub.Services.ProductService.Application.DTOs;
using ShopHub.Services.ProductService.Application.Handlers;
using ShopHub.Services.ProductService.Domain.Entities;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Tests.Application;

/// <summary>
/// Unit tests cho UpdateProductStockCommandHandler
/// </summary>
[TestClass]
public class UpdateProductStockCommandHandlerTests
{
    private Mock<IProductRepository> _mockProductRepository = null!;
    private Mock<IUnitOfWork> _mockUnitOfWork = null!;
    private Mock<ILogger<UpdateProductStockCommandHandler>> _mockLogger = null!;
    private UpdateProductStockCommandHandler _handler = null!;

    private Guid _productId;
    private Guid _categoryId;

    [TestInitialize]
    public void Setup()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateProductStockCommandHandler>>();
        _handler = new UpdateProductStockCommandHandler(
            _mockProductRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);

        _productId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();
    }

    [TestMethod]
    public async Task Handle_AddOperation_IncreaseStock()
    {
        // Arrange
        var command = new UpdateProductStockCommand(_productId, 10, StockOperation.Add);
        var product = Product.Create("Test Product", 100m, 50, _categoryId);
        product.GetType().GetProperty(nameof(product.Id))?.SetValue(product, _productId);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mockProductRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(60, result.Data.Stock); // 50 + 10
        Assert.AreEqual("Stock updated successfully", result.Message);
    }

    [TestMethod]
    public async Task Handle_SubtractOperation_DecreaseStock()
    {
        // Arrange
        var command = new UpdateProductStockCommand(_productId, 10, StockOperation.Subtract);
        var product = Product.Create("Test Product", 100m, 50, _categoryId);
        product.GetType().GetProperty(nameof(product.Id))?.SetValue(product, _productId);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mockProductRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(40, result.Data.Stock); // 50 - 10
    }

    [TestMethod]
    public async Task Handle_SetOperation_SetExactStock()
    {
        // Arrange
        var command = new UpdateProductStockCommand(_productId, 100, StockOperation.Set);
        var product = Product.Create("Test Product", 100m, 50, _categoryId);
        product.GetType().GetProperty(nameof(product.Id))?.SetValue(product, _productId);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mockProductRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(100, result.Data.Stock);
    }

    [TestMethod]
    public async Task Handle_SubtractOperationWithInsufficientStock_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateProductStockCommand(_productId, 100, StockOperation.Subtract);
        var product = Product.Create("Test Product", 100m, 50, _categoryId);
        product.GetType().GetProperty(nameof(product.Id))?.SetValue(product, _productId);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Insufficient stock", result.Message);
        Assert.AreEqual(400, result.StatusCode);

        // Verify repository.Update was NOT called
        _mockProductRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithProductNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new UpdateProductStockCommand(Guid.NewGuid(), 10, StockOperation.Add);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Product not found", result.Message);
        Assert.AreEqual(404, result.StatusCode);
    }

    [TestMethod]
    public async Task Handle_WithDeletedProduct_ReturnsNotFoundFailure()
    {
        // Arrange
        var command = new UpdateProductStockCommand(_productId, 10, StockOperation.Add);
        var product = Product.Create("Test Product", 100m, 50, _categoryId);
        product.GetType().GetProperty(nameof(product.Id))?.SetValue(product, _productId);
        product.GetType().GetProperty(nameof(product.IsDeleted))?.SetValue(product, true);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Product not found", result.Message);
        Assert.AreEqual(404, result.StatusCode);
    }

    [TestMethod]
    public async Task Handle_SubtractExactAmount_LeaveZeroStock()
    {
        // Arrange
        var command = new UpdateProductStockCommand(_productId, 50, StockOperation.Subtract);
        var product = Product.Create("Test Product", 100m, 50, _categoryId);
        product.GetType().GetProperty(nameof(product.Id))?.SetValue(product, _productId);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mockProductRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Data!.Stock);
    }

    [TestMethod]
    public async Task Handle_CallsSaveChangesAsyncAfterUpdate()
    {
        // Arrange
        var command = new UpdateProductStockCommand(_productId, 10, StockOperation.Add);
        var product = Product.Create("Test Product", 100m, 50, _categoryId);
        product.GetType().GetProperty(nameof(product.Id))?.SetValue(product, _productId);

        _mockProductRepository
            .Setup(x => x.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _mockProductRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_OnException_ReturnsFailureResult()
    {
        // Arrange
        var command = new UpdateProductStockCommand(_productId, 10, StockOperation.Add);
        _mockProductRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("Error updating stock"));
    }
}