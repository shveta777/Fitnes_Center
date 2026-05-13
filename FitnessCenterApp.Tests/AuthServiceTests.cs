using FitnessCenterApp.Models;
using FitnessCenterApp.Services;
using Xunit;

namespace FitnessCenterApp.Tests;

/// <summary>
/// Тесты проверяют авторизацию пользователей по ролям.
/// </summary>
public class AuthServiceTests
{
    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void Authenticate_AdminLogin_ReturnsAdminRole()
    {
        using var db = TestDbFactory.Create();
        var authService = new AuthService(db);

        var result = authService.Authenticate("admin@fitness.ru", "Admin@123");

        Assert.True(result.Success);
        Assert.Equal("admin", result.Role);
        Assert.Equal("Администратор", result.DisplayName);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void Authenticate_ClientLogin_ReturnsClientRole()
    {
        using var db = TestDbFactory.Create();
        db.Clients.Add(new Client
        {
            Firstname = "Иван",
            Lastname = "Петров",
            Email = "client@test.ru",
            Password = "Client@123"
        });
        db.SaveChanges();

        var authService = new AuthService(db);

        var result = authService.Authenticate("client@test.ru", "Client@123");

        Assert.True(result.Success);
        Assert.Equal("client", result.Role);
        Assert.Equal("Иван Петров", result.DisplayName);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void Authenticate_TrainerLogin_ReturnsTrainerRole()
    {
        using var db = TestDbFactory.Create();
        db.Trainers.Add(new Trainer
        {
            Firstname = "Алексей",
            Lastname = "Волков",
            Specialization = "Йога",
            Email = "trainer@test.ru",
            Password = "Trainer@123"
        });
        db.SaveChanges();

        var authService = new AuthService(db);

        var result = authService.Authenticate("trainer@test.ru", "Trainer@123");

        Assert.True(result.Success);
        Assert.Equal("trainer", result.Role);
        Assert.Equal("Алексей Волков", result.DisplayName);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void Authenticate_WrongPassword_ReturnsFalse()
    {
        using var db = TestDbFactory.Create();
        db.Clients.Add(new Client
        {
            Firstname = "Иван",
            Lastname = "Петров",
            Email = "client@test.ru",
            Password = "Client@123"
        });
        db.SaveChanges();

        var authService = new AuthService(db);

        var result = authService.Authenticate("client@test.ru", "wrong");

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Role);
    }
}
