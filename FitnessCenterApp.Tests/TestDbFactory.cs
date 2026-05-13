using FitnessCenterApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Tests;

/// <summary>
/// Фабрика создает отдельную тестовую InMemory-базу для каждого теста.
/// </summary>
public static class TestDbFactory
{
    public static FitnessCenterDbContext Create()
    {
        var options = new DbContextOptionsBuilder<FitnessCenterDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new FitnessCenterDbContext(options);
    }
}
