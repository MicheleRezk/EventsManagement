using System.Net;
using System.Text;
using System.Text.Json;
using EventManagement.Application.Features.User.DTOs;
using EventManagement.IntegrationTests.Constants;
using FluentAssertions;

namespace EventManagement.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GivenValidUserData_WhenRegisteringUser_ThenShouldReturnSuccess()
    {
        // Arrange
        var registerDto = new RegisterUserDto("newuser@example.com", "New User", "password123");
        var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(ApiRoutes.Auth.Register, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegisterResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        result.Should().NotBeNull();
        result!.Email.Should().Be(registerDto.Email);
        result.Name.Should().Be(registerDto.Name);
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GivenDuplicateEmail_WhenRegisteringUser_ThenShouldReturnBadRequest()
    {
        // Arrange
        var email = "duplicate@example.com";
        var registerDto = new RegisterUserDto(email, "User One", "password123");
        var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");

        await _client.PostAsync(ApiRoutes.Auth.Register, content);

        var duplicateDto = new RegisterUserDto(email, "User Two", "password456");
        var duplicateContent = new StringContent(JsonSerializer.Serialize(duplicateDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(ApiRoutes.Auth.Register, duplicateContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GivenValidCredentials_WhenLoggingIn_ThenShouldReturnJwtToken()
    {
        // Arrange
        var email = "loginuser@example.com";
        var password = "password123";
        var registerDto = new RegisterUserDto(email, "Login User", password);
        var registerContent = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
        await _client.PostAsync(ApiRoutes.Auth.Register, registerContent);

        // Login
        var loginDto = new LoginUserDto(email, password);
        var loginContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(ApiRoutes.Auth.Login, loginContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GivenInvalidCredentials_WhenLoggingIn_ThenShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginUserDto("nonexistent@example.com", "wrongpassword");
        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(ApiRoutes.Auth.Login, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", "Test User", "password123")]
    [InlineData("invalid-email", "Test User", "password123")]
    [InlineData("test@example.com", "", "password123")]
    [InlineData("test@example.com", "Test User", "")]
    public async Task GivenInvalidRegistrationData_WhenRegisteringUser_ThenShouldReturnBadRequest(string email, string name, string password)
    {
        // Arrange
        var registerDto = new RegisterUserDto(email, name, password);
        var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(ApiRoutes.Auth.Register, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
