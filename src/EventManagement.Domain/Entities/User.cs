namespace EventManagement.Domain.Entities;

public class User : EntityBase
{
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
}
