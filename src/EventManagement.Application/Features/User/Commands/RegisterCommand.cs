using EventManagement.Application.Features.User.DTOs;
using MediatR;

namespace EventManagement.Application.Features.User.Commands;

public record RegisterCommand(string Email, string Name, string Password) : IRequest<RegisterResponseDto>;
