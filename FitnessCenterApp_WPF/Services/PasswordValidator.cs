using System.Text.RegularExpressions;

namespace FitnessCenterApp.Services;

public static class PasswordValidator
{

    public static (bool IsValid, string ErrorMessage) Validate(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 6)
            return (false, "Пароль должен содержать минимум 6 символов.");

        if (!password.Any(char.IsUpper))
            return (false, "Пароль должен содержать минимум 1 прописную букву.");

        if (!password.Any(char.IsDigit))
            return (false, "Пароль должен содержать минимум 1 цифру.");

        if (!Regex.IsMatch(password, @"[!@#$%^]"))
            return (false, "Пароль должен содержать минимум 1 символ из набора: ! @ # $ % ^.");

        return (true, "");
    }
}
