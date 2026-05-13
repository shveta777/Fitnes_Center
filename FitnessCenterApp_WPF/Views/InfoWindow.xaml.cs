using System.Windows;

namespace FitnessCenterApp.Views;

public partial class InfoWindow : Window
{
    public string Caption { get; set; }
    public string Header { get; set; }
    public string Details { get; set; }
    public string Icon { get; set; }
    public string ConfirmText { get; set; }
    public string CancelText { get; set; }
    public bool ResultConfirmed { get; private set; }

    public InfoWindow(string caption, string header, string details,
        string icon = "ℹ", string confirmText = "Да", string cancelText = "Закрыть")
    {
        Caption = caption;
        Header = header;
        Details = details;
        Icon = icon;
        ConfirmText = confirmText;
        CancelText = cancelText;

        InitializeComponent();
        DataContext = this;

        if (confirmText == "")
            BtnYes.Visibility = Visibility.Collapsed;
    }

    private void BtnYes_Click(object sender, RoutedEventArgs e)
    {
        ResultConfirmed = true;
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        ResultConfirmed = false;
        DialogResult = false;
        Close();
    }
}
