using FitnessCenterApp.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessCenterApp.Tests;

/// <summary>
/// Тесты проверяют добавление, редактирование и удаление клиента.
/// </summary>
public class ClientCrudTests
{
    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void AddClient_ValidData_ClientSaved()
    {
        using var db = TestDbFactory.Create();

        db.Clients.Add(new Client
        {
            Firstname = "Мария",
            Lastname = "Иванова",
            Email = "maria@test.ru",
            Password = "Maria@123",
            DateOfBirth = new DateTime(2000, 1, 1),
            Gender = "Ж",
            Weight = 60,
            Height = 170
        });
        db.SaveChanges();

        Assert.Equal(1, db.Clients.Count());
        Assert.Equal("Мария", db.Clients.Single().Firstname);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void EditClient_ExistingClient_ClientUpdated()
    {
        using var db = TestDbFactory.Create();
        var client = new Client
        {
            Firstname = "Мария",
            Lastname = "Иванова",
            Email = "maria@test.ru",
            Password = "Maria@123"
        };
        db.Clients.Add(client);
        db.SaveChanges();

        client.PhoneNumber = "79000000000";
        client.Weight = 58;
        db.SaveChanges();

        var savedClient = db.Clients.Single();
        Assert.Equal("79000000000", savedClient.PhoneNumber);
        Assert.Equal(58, savedClient.Weight);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void DeleteClient_ExistingClient_ClientRemoved()
    {
        using var db = TestDbFactory.Create();
        var client = new Client
        {
            Firstname = "Мария",
            Lastname = "Иванова",
            Email = "maria@test.ru",
            Password = "Maria@123"
        };
        db.Clients.Add(client);
        db.SaveChanges();

        db.Clients.Remove(client);
        db.SaveChanges();

        Assert.Empty(db.Clients);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public async Task LoadClient_NullOptionalFields_DoesNotThrow()
    {
        using var db = TestDbFactory.Create();
        db.Clients.Add(new Client
        {
            Firstname = "Петр",
            Lastname = "Сидоров",
            Email = "petr@test.ru",
            Password = "Petr@123",
            PhoneNumber = null,
            DateOfBirth = null,
            Gender = null,
            Weight = null,
            Height = null
        });
        db.SaveChanges();

        var exception = await Record.ExceptionAsync(async () =>
        {
            var client = await db.Clients.FirstAsync();
            _ = client.PhoneNumber ?? string.Empty;
            _ = client.DateOfBirth?.ToShortDateString() ?? string.Empty;
            _ = client.Weight?.ToString() ?? string.Empty;
            _ = client.Height?.ToString() ?? string.Empty;
        });

        Assert.Null(exception);
    }
}
