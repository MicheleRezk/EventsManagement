namespace EventManagement.Infrastructure.Interfaces;

public interface IDatabaseUpgradeStrategy
{
    void Upgrade();
}