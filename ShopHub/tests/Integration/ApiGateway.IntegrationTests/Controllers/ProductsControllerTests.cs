using System.Net;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShopHub.Services.ProductService.Application.DTOs;

namespace ShopHub.IntegrationTests.Controllers;

/// <summary>
/// Integration tests cho ProductsController
/// </summary>
[TestClass]
public class ProductsControllerTests : IDisposable
{
    private HttpClient _httpClient = null!;

    [TestInitialize]
    public async Task Setup()
    {
        // TODO: Setup HttpClient với test server
        // Ví dụ:
        // var factory = new WebApplicationFactory<Program>();
        // _httpClient = factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _httpClient?.Dispose();
    }

    /// <summary>
    /// Test tạo sản phẩm mới - thành công
    /// </summary>
    [TestMethod]
    public async Task CreateProduct_WithValidData_ShouldReturn201()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Price = 99.99m,
            Stock = 100,
            CategoryId = Guid.NewGuid(),
            Description = "Test description",
            Sku = "TEST-001"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/products",
            createProductDto);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.IsNotNull(result);
        Assert.AreEqual(createProductDto.Name, result!.Name);
        Assert.AreEqual(createProductDto.Price, result.Price);
    }

    /// <summary>
    /// Test tạo sản phẩm với dữ liệu không hợp lệ
    /// </summary>
    [TestMethod]
    public async Task CreateProduct_WithInvalidData_ShouldReturn400()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "", // Invalid - empty name
            Price = -100, // Invalid - negative price
            Stock = 100,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/products",
            createProductDto);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Test lấy sản phẩm theo ID - thành công
    /// </summary>
    [TestMethod]
    public async Task GetProduct_WithValidId_ShouldReturn200()
    {
        // Arrange - tạo sản phẩm trước
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Price = 99.99m,
            Stock = 100,
            CategoryId = Guid.NewGuid()
        };

        var createResponse = await _httpClient.PostAsJsonAsync(
            "/api/v1/products",
            createProductDto);

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/products/{createdProduct!.ProductId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.IsNotNull(result);
        Assert.AreEqual(createdProduct.ProductId, result.ProductId);
    }

    /// <summary>
    /// Test lấy sản phẩm không tồn tại
    /// </summary>
    [TestMethod]
    public async Task GetProduct_WithNonExistentId_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _httpClient.GetAsync(
            $"/api/v1/products/{nonExistentId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Test cập nhật sản phẩm - thành công
    /// </summary>
    [TestMethod]
    public async Task UpdateProduct_WithValidData_ShouldReturn200()
    {
        // Arrange - tạo sản phẩm trước
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Price = 99.99m,
            Stock = 100,
            CategoryId = Guid.NewGuid()
        };

        var createResponse = await _httpClient.PostAsJsonAsync(
            "/api/v1/products",
            createProductDto);

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        // Update product
        var updateProductDto = new UpdateProductDto
        {
            ProductId = createdProduct!.ProductId,
            Name = "Updated Product",
            Price = 149.99m
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/products/{createdProduct.ProductId}",
            updateProductDto);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        Assert.IsNotNull(result);
        Assert.AreEqual("Updated Product", result.Name);
        Assert.AreEqual(149.99m, result.Price);
    }

    /// <summary>
    /// Test xóa sản phẩm - thành công
    /// </summary>
    [TestMethod]
    public async Task DeleteProduct_WithValidId_ShouldReturn204()
    {
        // Arrange - tạo sản phẩm trước
        var createProductDto = new CreateProductDto
        {
            Name = "Test Product",
            Price = 99.99m,
            Stock = 100,
            CategoryId = Guid.NewGuid()
        };

        var createResponse = await _httpClient.PostAsJsonAsync(
            "/api/v1/products",
            createProductDto);

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        // Act
        var response = await _httpClient.DeleteAsync(
            $"/api/v1/products/{createdProduct!.ProductId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}