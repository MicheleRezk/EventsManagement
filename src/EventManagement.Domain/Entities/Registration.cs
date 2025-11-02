namespace EventManagement.Domain.Entities;

public class Registration : EntityBase
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public virtual Event Event { get; set; } = default!;
}
