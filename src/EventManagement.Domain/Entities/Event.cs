namespace EventManagement.Domain.Entities;

public class Event : EntityBase
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Location { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid CreatedByUserId { get; set; }
    public virtual User CreatedBy { get; set; } = default!;
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
