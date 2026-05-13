using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using FitnessCenterApp.Services;
using System.Text.RegularExpressions;
using System.Windows;

namespace FitnessCenterApp.Views;

public partial class RegisterWindow : Window
{
    private readonly FitnessCenterDbContext _context;

    public RegisterWindow(FitnessCenterDbContext context)
    {
        _context = context;
        InitializeComponent();
    }


    private bool _isFormattingPhone;

    private void TxtPhone_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_isFormattingPhone)
            return;

        var digits = new string(TxtPhone.Text.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("8"))
            digits = "7" + digits[1..];

        if (!digits.StartsWith("7"))
            digits = "7" + digits;

        if (digits.Length > 11)
            digits = digits[..11];

        _isFormattingPhone = true;
        TxtPhone.Text = FormatPhone(digits);
        TxtPhone.CaretIndex = TxtPhone.Text.Length;
        _isFormattingPhone = false;
    }

    private static string FormatPhone(string digits)
    {
        if (string.IsNullOrWhiteSpace(digits))
            return "";

        string result = "+7";

        if (digits.Length > 1)
            result += " (" + digits.Substring(1, Math.Min(3, digits.Length - 1));

        if (digits.Length >= 4)
            result += ")";

        if (digits.Length > 4)
            result += " " + digits.Substring(4, Math.Min(3, digits.Length - 4));

        if (digits.Length > 7)
            result += "-" + digits.Substring(7, Math.Min(2, digits.Length - 7));

        if (digits.Length > 9)
            result += "-" + digits.Substring(9, Math.Min(2, digits.Length - 9));

        return result;
    }

    private static string PhoneDigits(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.StartsWith("7") ? digits : "7" + digits;
    }


    private bool _isFormattingBirthDate;

    private void TxtBirth_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_isFormattingBirthDate)
            return;

        var digits = new string(TxtBirth.Text.Where(char.IsDigit).ToArray());

        if (digits.Length > 8)
            digits = digits[..8];

        var result = "";

        for (int i = 0; i < digits.Length; i++)
        {
            if (i == 2 || i == 4)
                result += ".";

            result += digits[i];
        }

        _isFormattingBirthDate = true;
        TxtBirth.Text = result;
        TxtBirth.CaretIndex = TxtBirth.Text.Length;
        _isFormattingBirthDate = false;
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        var firstname = TxtFirstname.Text.Trim();
        var lastname = TxtLastname.Text.Trim();
        var email = TxtEmail.Text.Trim();
        var password = TxtPassword.Password;
        var repeat = TxtRepeatPassword.Password;

        if (string.IsNullOrWhiteSpace(firstname) ||
            string.IsNullOrWhiteSpace(lastname) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Заполните обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtBirth.Text))
        {
            MessageBox.Show("Дата рождения обязательна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!DateTime.TryParse(TxtBirth.Text, out var birthDate))
        {
            MessageBox.Show("Введите дату рождения в формате дд.мм.гггг.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (birthDate.Date > DateTime.Today)
        {
            MessageBox.Show("Дата рождения не может быть в будущем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CbGender.SelectedItem == null)
        {
            MessageBox.Show("Выберите пол.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var phoneDigits = PhoneDigits(TxtPhone.Text);
        if (phoneDigits.Length != 11)
        {
            MessageBox.Show("Введите телефон полностью в формате +7 (999) 999-99-99.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            MessageBox.Show("Некорректный email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (password != repeat)
        {
            MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var validation = PasswordValidator.Validate(password);
        if (!validation.IsValid)
        {
            MessageBox.Show(validation.ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_context.Clients.Any(c => c.Email == email))
        {
            MessageBox.Show("Пользователь с таким email уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!string.IsNullOrWhiteSpace(TxtWeight.Text) &&
            (!decimal.TryParse(TxtWeight.Text, out var w) || w <= 0 || w > 300))
        {
            MessageBox.Show("Вес должен быть положительным числом не больше 300.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!string.IsNullOrWhiteSpace(TxtHeight.Text) &&
            (!decimal.TryParse(TxtHeight.Text, out var h) || h <= 0 || h > 250))
        {
            MessageBox.Show("Рост должен быть положительным числом не больше 250.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        decimal? weight = decimal.TryParse(TxtWeight.Text, out w) ? w : null;
        decimal? height = decimal.TryParse(TxtHeight.Text, out h) ? h : null;

        var client = new Client
        {
            Firstname = firstname,
            Lastname = lastname,
            Email = email,
            Password = password,
            PhoneNumber = phoneDigits,
            DateOfBirth = birthDate,
            Gender = (CbGender.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString(),
            Weight = weight,
            Height = height
        };

        try
        {
            _context.Clients.Add(client);
            _context.SaveChanges();
            MessageBox.Show("Регистрация выполнена успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(GetFriendlyRegistrationError(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string GetFriendlyRegistrationError(Exception ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;

        if (message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("повторяющийся ключ", StringComparison.OrdinalIgnoreCase))
            return "Пользователь с таким email уже существует. Введите другой email.";

        if (message.Contains("Cannot insert the value NULL", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("значение NULL", StringComparison.OrdinalIgnoreCase))
            return "Заполните все обязательные поля.";

        if (message.Contains("CHECK", StringComparison.OrdinalIgnoreCase))
            return "Проверьте введённые данные. Некоторые значения не соответствуют ограничениям базы.";

        return "Не удалось выполнить регистрацию. Проверьте данные и попробуйте ещё раз.";
    }

}
