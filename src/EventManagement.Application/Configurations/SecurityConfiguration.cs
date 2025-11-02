namespace EventManagement.Application.Configurations;

public record SecurityConfiguration(string JwtKey, string JwtIssuer, string JwtAudience, int JwtExpiryInMinutes);