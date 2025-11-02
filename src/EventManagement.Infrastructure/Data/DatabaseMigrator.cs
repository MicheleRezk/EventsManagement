using EventManagement.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Data;

public class DatabaseMigrator: IDatabaseUpgradeStrategy
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseMigrator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Upgrade()
    {
        _dbContext.Database.Migrate();
    }
}