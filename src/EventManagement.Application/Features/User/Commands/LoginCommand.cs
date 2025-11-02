using EventManagement.Application.Features.User.DTOs;
using MediatR;

namespace EventManagement.Application.Features.User.Commands;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponseDto>;
