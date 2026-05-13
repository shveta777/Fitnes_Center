using FitnessCenterApp.Data;
using FitnessCenterApp.Services;
using System.Windows;

namespace FitnessCenterApp.Views;

public partial class LoginWindow : Window
{
    private readonly FitnessCenterDbContext _context;
    private readonly AuthService _authService;
    private bool _passwordVisible;

    public LoginWindow()
    {
        InitializeComponent();
        _context = DbContextProvider.Create();
        _authService = new AuthService(_context);
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        var password = _passwordVisible ? TxtPasswordVisible.Text : TxtPassword.Password;
        var result = _authService.Authenticate(TxtEmail.Text, password);

        if (!result.Success)
        {
            MessageBox.Show(result.DisplayName, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Window window = result.Role switch
        {
            "admin" => new AdminMainWindow(_context),
            "trainer" => new TrainerMainWindow(_context, result.UserId),
            "client" => new ClientMainWindow(_context, result.UserId),
            _ => throw new InvalidOperationException()
        };

        window.Closed += (_, _) =>
        {
            TxtEmail.Clear();
            TxtPassword.Clear();
            TxtPasswordVisible.Clear();
            Show();
        };

        window.Show();
        Hide();
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        var window = new RegisterWindow(_context);
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnShowPassword_Click(object sender, RoutedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;

        if (_passwordVisible)
        {
            TxtPasswordVisible.Text = TxtPassword.Password;
            TxtPasswordVisible.Visibility = Visibility.Visible;
            TxtPassword.Visibility = Visibility.Collapsed;
            BtnShowPassword.Content = "🙈";
            TxtPasswordVisible.Focus();
            TxtPasswordVisible.CaretIndex = TxtPasswordVisible.Text.Length;
        }
        else
        {
            TxtPassword.Password = TxtPasswordVisible.Text;
            TxtPassword.Visibility = Visibility.Visible;
            TxtPasswordVisible.Visibility = Visibility.Collapsed;
            BtnShowPassword.Content = "👁";
            TxtPassword.Focus();
        }
    }
}
