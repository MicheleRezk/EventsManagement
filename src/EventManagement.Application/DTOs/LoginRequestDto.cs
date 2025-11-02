namespace EventManagement.Application.DTOs;

public record LoginRequestDto(
    string Email,
    string Password
);
