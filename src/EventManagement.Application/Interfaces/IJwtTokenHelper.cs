using EventManagement.Domain.Entities;

namespace EventManagement.Application.Interfaces;

public interface IJwtTokenHelper
{
    string GenerateToken(User user);
}
