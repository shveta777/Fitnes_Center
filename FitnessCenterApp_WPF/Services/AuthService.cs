using FitnessCenterApp.Data;
using System.Text.RegularExpressions;

namespace FitnessCenterApp.Services;

public class AuthService
{
    private readonly FitnessCenterDbContext _context;
    
    public AuthService(FitnessCenterDbContext context) => _context = context;
    
    public AuthResult Authenticate(string email, string password)
    {
        email = email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
            return new AuthResult(false, "error", 0, "Введите email.");

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return new AuthResult(false, "error", 0, "Некорректный формат email.");

        if (string.IsNullOrWhiteSpace(password))
            return new AuthResult(false, "error", 0, "Введите пароль.");

        if (email == "admin@fitness.ru" && password == "Admin@123")
            return new AuthResult(true, "admin", 0, "Администратор");

        var client = _context.Clients
            .FirstOrDefault(c => c.Email == email && c.Password == password);
        if (client != null)
            return new AuthResult(true, "client", client.IdClients,
                $"{client.Firstname} {client.Lastname}");

        var trainer = _context.Trainers
            .FirstOrDefault(t => t.Email == email && t.Password == password);
        if (trainer != null)
            return new AuthResult(true, "trainer", trainer.IdTrainer,
                $"{trainer.Firstname} {trainer.Lastname}");

        return new AuthResult(false, "error", 0, "Неверный email или пароль.");
    }
}

public class AuthResult
{
    public bool Success { get; }
    public string Role { get; }
    public int UserId { get; }
    public string DisplayName { get; }
    
    public AuthResult(bool success, string role, int userId, string displayName)
    {
        Success = success;
        Role = role;
        UserId = userId;
        DisplayName = displayName;
    }
}
