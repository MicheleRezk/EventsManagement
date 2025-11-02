namespace EventManagement.Application.DTOs;

public record RegisterRequestDto(
    string Name,
    string Email,
    string Password
);
