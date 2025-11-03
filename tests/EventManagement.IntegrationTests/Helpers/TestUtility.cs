using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EventManagement.Application.DTOs;
using EventManagement.Application.Features.User.DTOs;
using EventManagement.IntegrationTests.Constants;
using FluentAssertions;

namespace EventManagement.IntegrationTests.Helpers;

public static class TestUtility
{
    private static async Task<string> RegisterAndLoginUserAsync(HttpClient client, string email = "test@example.com", string name = "Test User", string password = "password123")
    {
        // Register user
        var registerDto = new RegisterUserDto(email, name, password);
        var registerContent = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
        var registerResponse = await client.PostAsync(ApiRoutes.Auth.Register, registerContent);
        registerResponse.EnsureSuccessStatusCode();

        // Login user
        var loginDto = new LoginUserDto(email, password);
        var loginContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var loginResponse = await client.PostAsync(ApiRoutes.Auth.Login, loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<LoginResponseDto>(loginResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        loginResult.Should().NotBeNull();
        loginResult!.Token.Should().NotBeNullOrEmpty();

        return loginResult.Token;
    }

    public static void AddJwt(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static void ClearAuthorizationHeader(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    public static async Task<string> CreateAuthenticatedUserAsync(
        HttpClient baseClient, string email = "test@example.com",
        string name = "Test User", string password = "password123")
    {
        var token = await RegisterAndLoginUserAsync(baseClient, email, name, password);
        baseClient.AddJwt(token);
        return token;
    }
}
