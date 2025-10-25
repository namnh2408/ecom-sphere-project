using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShopHub.Services.ProductService.Domain.Entities;

namespace ShopHub.Services.ProductService.Tests.Domain;

/// <summary>
/// Unit tests cho Product entity
/// </summary>
[TestClass]
public class ProductTests
{
    private const string ValidProductName = "Test Product";
    private const decimal ValidPrice = 100.00m;
    private const int ValidStock = 50;
    private static readonly Guid ValidCategoryId = Guid.NewGuid();

    /// <summary>
    /// Test tạo sản phẩm với dữ liệu hợp lệ
    /// </summary>
    [TestMethod]
    public void CreateProduct_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var product = Product.Create(
            name: ValidProductName,
            price: ValidPrice,
            stock: ValidStock,
            categoryId: ValidCategoryId);

        // Assert
        Assert.IsNotNull(product);
        Assert.AreEqual(ValidProductName, product.Name);
        Assert.AreEqual(ValidPrice, product.Price);
        Assert.AreEqual(ValidStock, product.Stock);
        Assert.AreEqual(ValidCategoryId, product.CategoryId);
        Assert.IsTrue(product.IsActive);
        Assert.IsFalse(product.IsDeleted);
    }

    /// <summary>
    /// Test tạo sản phẩm với tên rỗng
    /// </summary>
    [TestMethod]
    public void CreateProduct_WithEmptyName_ShouldThrow()
    {
        // Act & Assert
        try
        {
            Product.Create(
                name: string.Empty,
                price: ValidPrice,
                stock: ValidStock,
                categoryId: ValidCategoryId);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Test tạo sản phẩm với giá âm
    /// </summary>
    [TestMethod]
    public void CreateProduct_WithNegativePrice_ShouldThrow()
    {
        // Act & Assert
        try
        {
            Product.Create(
                name: ValidProductName,
                price: -100,
                stock: ValidStock,
                categoryId: ValidCategoryId);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Test tạo sản phẩm với stock âm
    /// </summary>
    [TestMethod]
    public void CreateProduct_WithNegativeStock_ShouldThrow()
    {
        // Act & Assert
        try
        {
            Product.Create(
                name: ValidProductName,
                price: ValidPrice,
                stock: -10,
                categoryId: ValidCategoryId);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Test cập nhật thông tin sản phẩm
    /// </summary>
    [TestMethod]
    public void UpdateProduct_WithValidData_ShouldSucceed()
    {
        // Arrange
        var product = Product.Create(
            name: ValidProductName,
            price: ValidPrice,
            stock: ValidStock,
            categoryId: ValidCategoryId);

        var newName = "Updated Product";
        var newPrice = 150.00m;

        // Act
        product.Update(name: newName, price: newPrice);

        // Assert
        Assert.AreEqual(newName, product.Name);
        Assert.AreEqual(newPrice, product.Price);
        Assert.IsNotNull(product.UpdatedAt);
    }

    /// <summary>
    /// Test deactivate sản phẩm
    /// </summary>
    [TestMethod]
    public void DeactivateProduct_ShouldSetIsActiveFalse()
    {
        // Arrange
        var product = Product.Create(
            name: ValidProductName,
            price: ValidPrice,
            stock: ValidStock,
            categoryId: ValidCategoryId);

        // Act
        product.Deactivate();

        // Assert
        Assert.IsFalse(product.IsActive);
    }

    /// <summary>
    /// Test activate sản phẩm
    /// </summary>
    [TestMethod]
    public void ActivateProduct_ShouldSetIsActiveTrue()
    {
        // Arrange
        var product = Product.Create(
            name: ValidProductName,
            price: ValidPrice,
            stock: ValidStock,
            categoryId: ValidCategoryId);
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        Assert.IsTrue(product.IsActive);
    }

    /// <summary>
    /// Test cập nhật stock
    /// </summary>
    [TestMethod]
    public void UpdateStock_WithPositiveQuantity_ShouldIncrease()
    {
        // Arrange
        var product = Product.Create(
            name: ValidProductName,
            price: ValidPrice,
            stock: ValidStock,
            categoryId: ValidCategoryId);

        // Act
        product.UpdateStock(10);

        // Assert
        Assert.AreEqual(ValidStock + 10, product.Stock);
    }

    /// <summary>
    /// Test cập nhật stock không đủ
    /// </summary>
    [TestMethod]
    public void UpdateStock_WithInsufficientStock_ShouldThrow()
    {
        // Arrange
        var product = Product.Create(
            name: ValidProductName,
            price: ValidPrice,
            stock: 10,
            categoryId: ValidCategoryId);

        // Act & Assert
        try
        {
            product.UpdateStock(-20);
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    /// <summary>
    /// Test domain event được tạo
    /// </summary>
    [TestMethod]
    public void CreateProduct_ShouldRaiseDomainEvent()
    {
        // Arrange & Act
        var product = Product.Create(
            name: ValidProductName,
            price: ValidPrice,
            stock: ValidStock,
            categoryId: ValidCategoryId);

        // Assert
        Assert.HasCount(product.DomainEvents, 1);
        Assert.IsInstanceOfType<ProductCreatedEvent>(product.DomainEvents[0]);
    }

    /// <summary>
    /// Test equality dựa trên ID
    /// </summary>
    [TestMethod]
    public void TwoProducts_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var product1 = Product.Create(
            name: ValidProductName,
            price: ValidPrice,
            stock: ValidStock,
            categoryId: ValidCategoryId);

        var product2 = Product.Create(
            name: "Different Name",
            price: 200,
            stock: 100,
            categoryId: ValidCategoryId);

        // Act
        var product2WithSameId = product1; // Same reference

        // Assert
        Assert.AreEqual(product1, product2WithSameId);
    }
}