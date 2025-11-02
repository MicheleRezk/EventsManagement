namespace EventManagement.Application.Features.User.DTOs;

public record RegisterResponseDto(Guid UserId, string Name, string Email);
