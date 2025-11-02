namespace EventManagement.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Name { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
