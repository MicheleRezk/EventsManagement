using EventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Event> Events { get; }
    DbSet<Registration> Registrations { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}