using FitnessCenterApp.Services;
using Xunit;

namespace FitnessCenterApp.Tests;

/// <summary>
/// Тесты проверяют выполнение требований к паролю.
/// </summary>
public class PasswordValidatorTests
{
    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void Validate_CorrectPassword_ReturnsTrue()
    {
        var result = PasswordValidator.Validate("Qwerty1!");

        Assert.True(result.IsValid);
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("Ab1!")]
    public void Validate_ShortPassword_ReturnsFalse(string password)
    {
        var result = PasswordValidator.Validate(password);

        Assert.False(result.IsValid);
        Assert.Contains("минимум 6", result.ErrorMessage);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void Validate_WithoutUppercase_ReturnsFalse()
    {
        var result = PasswordValidator.Validate("qwerty1!");

        Assert.False(result.IsValid);
        Assert.Contains("прописную", result.ErrorMessage);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void Validate_WithoutDigit_ReturnsFalse()
    {
        var result = PasswordValidator.Validate("Qwerty!");

        Assert.False(result.IsValid);
        Assert.Contains("цифру", result.ErrorMessage);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void Validate_WithoutSpecialSymbol_ReturnsFalse()
    {
        var result = PasswordValidator.Validate("Qwerty1");

        Assert.False(result.IsValid);
        Assert.Contains("символ", result.ErrorMessage);
    }
}
