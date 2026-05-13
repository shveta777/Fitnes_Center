using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FitnessCenterApp.Views;

public partial class ClientMainWindow : Window
{
    private readonly FitnessCenterDbContext _context;
    private readonly int _clientId;
    private string _section = "Welcome";
    private string _clientName = "клиент";

    public ClientMainWindow(FitnessCenterDbContext context, int clientId)
    {
        _context = context;
        _clientId = clientId;
        InitializeComponent();
        SetClientName();
        SelectSection("Welcome");
    }

    private void SetClientName()
    {
        var client = _context.Clients.FirstOrDefault(c => c.IdClients == _clientId);
        _clientName = client == null ? "клиент" : $"{client.Firstname} {client.Lastname}".Trim();
        TxtClientName.Text = _clientName;
    }

    private void SelectSection(string section)
    {
        _section = section;
        TxtSearch.Text = "";
        BtnMainAction.Visibility = Visibility.Visible;
        TxtSearch.Visibility = Visibility.Visible;

        switch (section)
        {
            case "Welcome":
                TxtTitle.Text = "🏠  Главная";
                BtnMainAction.Visibility = Visibility.Collapsed;
                TxtSearch.Visibility = Visibility.Collapsed;
                LoadWelcome();
                break;
            case "Trainings":
                TxtTitle.Text = "📋  Все тренировки";
                BtnMainAction.Content = "Записаться";
                TxtSearch.Text = "";
                TxtSearch.ToolTip = "Поиск тренировки";
                LoadTrainings();
                break;
            case "Records":
                TxtTitle.Text = "📝  Мои записи";
                BtnMainAction.Content = "Выписаться";
                LoadRecords();
                break;
            case "Rates":
                TxtTitle.Text = "🏷  Тарифы";
                BtnMainAction.Content = "Купить";
                LoadRates();
                break;
            case "Subscriptions":
                TxtTitle.Text = "💳  Мои абонементы";
                BtnMainAction.Visibility = Visibility.Collapsed;
                LoadSubscriptions();
                break;
        }
    }

    private void LoadWelcome()
    {
        ContentStack.Children.Clear();

        var container = new StackPanel { Margin = new Thickness(40, 30, 40, 0) };
        container.Children.Add(new TextBlock
        {
            Text = "Добро пожаловать в FitnessCenter",
            FontSize = 30,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 24)
        });

        var topCards = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 22) };
        topCards.Children.Add(CreateSmallInfoCard("🏋", "Тренировки", "Выбор и запись"));
        topCards.Children.Add(CreateSmallInfoCard("💳", "Абонементы", "Покупка и просмотр"));
        topCards.Children.Add(CreateSmallInfoCard("▦", "Календарь", "Ближайшие записи"));
        container.Children.Add(topCards);

        var lower = new Grid();
        lower.ColumnDefinitions.Add(new ColumnDefinition());
        lower.ColumnDefinitions.Add(new ColumnDefinition());
        lower.Children.Add(CreateCalendarBlock());

        var upcoming = CreateUpcomingBlock();
        Grid.SetColumn(upcoming, 1);
        lower.Children.Add(upcoming);

        container.Children.Add(lower);
        ContentStack.Children.Add(container);
    }

    private Border CreateSmallInfoCard(string icon, string title, string text)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(new TextBlock { Text = icon, FontSize = 25, Margin = new Thickness(0, 0, 14, 0) });
        panel.Children.Add(new StackPanel
        {
            Children =
            {
                new TextBlock { Text = title, FontSize = 17, FontWeight = FontWeights.Bold },
                new TextBlock { Text = text, FontSize = 14 }
            }
        });

        return new Border
        {
            Style = (Style)FindResource("CardBorder"),
            Margin = new Thickness(0, 0, 14, 0),
            Child = panel
        };
    }

    private Border CreateCalendarBlock()
    {
        var grid = new UniformGrid { Columns = 7, Rows = 6, Margin = new Thickness(0, 8, 20, 0) };
        string[] days = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
        foreach (var d in days)
            grid.Children.Add(new TextBlock { Text = d, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });

        var today = DateTime.Today;
        for (int i = 1; i <= 35; i++)
        {
            int day = i <= DateTime.DaysInMonth(today.Year, today.Month) ? i : 0;
            grid.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(14),
                Background = day == today.Day ? (Brush)FindResource("AccentBrush") : Brushes.Transparent,
                Margin = new Thickness(3),
                Child = new TextBlock
                {
                    Text = day == 0 ? "" : day.ToString(),
                    FontSize = 13,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "Май 2026", FontWeight = FontWeights.Bold, FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center });
        stack.Children.Add(grid);

        return new Border
        {
            Background = (Brush)FindResource("LightYellowBrush"),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 12, 0),
            Child = stack
        };
    }

    private Border CreateUpcomingBlock()
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = "Ближайшие записи",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 12)
        });

        var records = _context.ClientRecords
            .Where(r => r.IdClients == _clientId)
            .Include(r => r.Training)
            .OrderByDescending(r => r.RecordDate)
            .Take(3)
            .ToList();

        if (records.Count == 0)
            stack.Children.Add(CreateMiniRecord("Записей пока нет", "Откройте все тренировки"));
        else
            foreach (var r in records)
                stack.Children.Add(CreateMiniRecord(r.Training.TrainingName, FormatDateTime(r.RecordDate)));

        return new Border
        {
            Background = (Brush)FindResource("LightYellowBrush"),
            Padding = new Thickness(20),
            Margin = new Thickness(12, 0, 0, 0),
            Child = stack
        };
    }

    private Border CreateMiniRecord(string title, string info)
    {
        return new Border
        {
            Style = (Style)FindResource("CardBorder"),
            MinHeight = 0,
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 0, 10),
            Cursor = Cursors.Hand,
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = title, FontWeight = FontWeights.Bold, FontSize = 15 },
                    new TextBlock { Text = info, FontSize = 13, Foreground = Brushes.DimGray }
                }
            }
        };
    }

    private void LoadTrainings()
    {
        ContentStack.Children.Clear();
        string search = TxtSearch.Text.Trim().ToLower();

        var recordedIds = _context.ClientRecords
            .Where(r => r.IdClients == _clientId)
            .Select(r => r.IdTraining)
            .ToList();

        var items = _context.Schedules.Include(s => s.Training)
            .Where(s => s.Status != "Отменено")
            .AsEnumerable()
            .Where(s => Match(search, s.Training.TrainingName, s.Training.Category, s.Training.Place))
            .OrderBy(s => s.TrainingDate)
            .ThenBy(s => s.StartTime)
            .ToList();

        if (items.Count == 0) AddNoData("Нет доступных тренировок.");
        foreach (var s in items)
        {
            bool recorded = recordedIds.Contains(s.IdTraining);
            AddCard(s.Training.TrainingName,
                $"Дата: {FormatDate(s.TrainingDate)}",
                recorded ? "Вы записаны" : "Можно записаться",
                () => ShowTrainingDetails(s));
        }
    }

    private void LoadRecords()
    {
        ContentStack.Children.Clear();
        string search = TxtSearch.Text.Trim().ToLower();

        var records = _context.ClientRecords
            .Where(r => r.IdClients == _clientId)
            .Include(r => r.Training)
            .AsEnumerable()
            .Where(r => Match(search, r.Training.TrainingName, r.Training.Category, r.Presence))
            .OrderByDescending(r => r.RecordDate)
            .ToList();

        if (records.Count == 0) AddNoData("У вас пока нет записей на тренировки.");
        foreach (var r in records)
            AddCard(r.Training.TrainingName, $"Дата записи: {FormatDateTime(r.RecordDate)}",
                r.Presence ?? "Не отмечено", () => ShowRecordDetails(r));
    }

    private void LoadRates()
    {
        ContentStack.Children.Clear();
        string search = TxtSearch.Text.Trim().ToLower();

        var rates = _context.RateSubscriptions
            .AsEnumerable()
            .Where(r => Match(search, r.Name, r.Type))
            .OrderBy(r => r.Cost)
            .ToList();

        if (rates.Count == 0) AddNoData("Тарифы не найдены.");
        foreach (var r in rates)
            AddCard(r.Name, $"Тип: {r.Type}", $"Посещений: {r.NumberOfVisits}",
                () => ConfirmBuyRate(r));
    }

    private void LoadSubscriptions()
    {
        ContentStack.Children.Clear();
        string search = TxtSearch.Text.Trim().ToLower();

        var subs = _context.Subscriptions
            .Where(s => s.IdClients == _clientId)
            .Include(s => s.RateSubscription)
            .AsEnumerable()
            .Where(s => Match(search, s.RateSubscription.Name, s.RateSubscription.Type))
            .OrderByDescending(s => s.EndDate)
            .ToList();

        if (subs.Count == 0) AddNoData("Купленных абонементов пока нет.");
        foreach (var s in subs)
            AddCard(s.RateSubscription.Name, $"Даты: {FormatDate(s.StartDate)} — {FormatDate(s.EndDate)}",
                IsSubscriptionActive(s) ? "Активен" : "Истёк", () => ShowSubscriptionDetails(s));
    }

    private void AddCard(string title, string info, string status, Action action)
    {
        var border = new Border
        {
            Style = (Style)FindResource("CardBorder"),
            Cursor = Cursors.Hand
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });

        var text = new StackPanel();
        text.Children.Add(new TextBlock { Text = title, FontSize = 16, FontWeight = FontWeights.Bold });
        text.Children.Add(new TextBlock { Text = info, FontSize = 13, Foreground = Brushes.DimGray, Margin = new Thickness(0, 6, 0, 0) });
        text.Children.Add(new TextBlock { Text = status, FontSize = 13, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 6, 0, 0) });

        var btn = new Button { Content = "Подробнее", Style = (Style)FindResource("RoundedButton"), Width = 120, Height = 34 };
        btn.Click += (_, e) => { e.Handled = true; action(); };

        Grid.SetColumn(btn, 1);
        grid.Children.Add(text);
        grid.Children.Add(btn);

        border.Child = grid;
        border.MouseLeftButtonUp += (_, _) => action();
        ContentStack.Children.Add(border);
    }

    private void AddNoData(string text)
    {
        ContentStack.Children.Add(new Border
        {
            Style = (Style)FindResource("CardBorder"),
            Child = new TextBlock { Text = text, FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }
        });
    }

    private void ShowTrainingDetails(Schedule s)
    {
        string details =
            $"Категория: {s.Training.Category}\n" +
            $"Интенсивность: {s.Training.Intensity ?? "-"}\n" +
            $"Дата: {FormatDate(s.TrainingDate)}\n" +
            $"Время: {FormatTime(s.StartTime)} - {FormatTime(s.EndTime)}\n" +
            $"Место: {s.Training.Place ?? "-"}\n" +
            $"Статус: {s.Status ?? "-"}\n\n" +
            "Записаться на эту тренировку?";

        var win = new InfoWindow("Информация о тренировке", s.Training.TrainingName, details, "💪", "Да", "Отмена") { Owner = this };
        if (win.ShowDialog() == true)
        {
            if (MessageBox.Show("Вы уверены, что хотите записаться на эту тренировку?",
                    "Подтверждение записи",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                TryRecord(s.IdTraining);
            }
        }
    }

    private void ShowRecordDetails(ClientRecordsTraining r)
    {
        string details =
            $"Категория: {r.Training.Category}\n" +
            $"Дата записи: {FormatDateTime(r.RecordDate)}\n" +
            $"Посещение: {r.Presence ?? "Не отмечено"}\n\n" +
            "Выписаться с этой тренировки?";

        var win = new InfoWindow("Моя запись", r.Training.TrainingName, details, "?", "Да", "Отмена") { Owner = this };
        if (win.ShowDialog() == true)
        {
            if (MessageBox.Show("Вы уверены, что хотите выписаться с этой тренировки?",
                    "Подтверждение отмены",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CancelRecord(r.IdClientRecordsTraining);
            }
        }
    }

    private void ShowSubscriptionDetails(Subscription s)
    {
        string details =
            $"Тип: {s.RateSubscription.Type}\n" +
            $"Стоимость: {s.RateSubscription.Cost:0.00} руб.\n" +
            $"Посещений: {s.RateSubscription.NumberOfVisits}\n" +
            $"Начало: {FormatDate(s.StartDate)}\n" +
            $"Конец: {FormatDate(s.EndDate)}";

        new InfoWindow("Абонемент", s.RateSubscription.Name, details, "💳", "", "Закрыть") { Owner = this }.ShowDialog();
    }

    private void TryRecord(int trainingId)
    {
        if (_context.ClientRecords.Any(r => r.IdTraining == trainingId && r.IdClients == _clientId))
        {
            MessageBox.Show("Вы уже записаны на эту тренировку.", "Внимание");
            return;
        }

        _context.ClientRecords.Add(new ClientRecordsTraining
        {
            IdTraining = trainingId,
            IdClients = _clientId,
            RecordDate = DateTime.Now,
            Presence = null
        });

        _context.SaveChanges();
        MessageBox.Show("Запись создана.", "Успех");
        SelectSection("Records");
    }

    private void CancelRecord(int id)
    {
        var record = _context.ClientRecords.Find(id);
        if (record == null) return;

        _context.ClientRecords.Remove(record);
        _context.SaveChanges();
        SelectSection("Records");
    }

    private void ConfirmBuyRate(RateSubscription rate)
    {
        string details =
            $"Тип: {rate.Type}\nСтоимость: {rate.Cost:0.00} руб.\nПосещений: {rate.NumberOfVisits}\n\nКупить этот абонемент?";
        var win = new InfoWindow("Покупка абонемента", rate.Name, details, "💳", "Да", "Отмена") { Owner = this };
        if (win.ShowDialog() == true)
            BuySubscription(rate);
    }

    private void BuySubscription(RateSubscription rate)
    {
        _context.Subscriptions.Add(new Subscription
        {
            IdRateSubscription = rate.IdRateSubscription,
            IdClients = _clientId,
            StartDate = DateTime.Today,
            EndDate = CalculateEndDate(DateTime.Today, rate)
        });
        _context.SaveChanges();
        SelectSection("Subscriptions");
    }

    private void BtnMainAction_Click(object sender, RoutedEventArgs e)
    {
        if (_section == "Trainings")
        {
            OpenTrainingChoiceDialog();
            return;
        }

        if (_section == "Records")
        {
            OpenCancelRecordChoiceDialog();
            return;
        }

        if (_section == "Rates")
        {
            OpenRateChoiceDialog();
            return;
        }
    }


    private void OpenTrainingChoiceDialog()
    {
        var schedules = _context.Schedules
            .Include(s => s.Training)
            .Where(s => s.Status != "Отменено")
            .AsEnumerable()
            .OrderBy(s => s.TrainingDate)
            .ThenBy(s => s.StartTime)
            .ToList();

        if (schedules.Count == 0)
        {
            MessageBox.Show("Нет доступных тренировок для записи.", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selected = ShowChoiceWindow(
            "Запись на тренировку",
            "Выберите тренировку:",
            schedules,
            s => $"{s.Training.TrainingName} | {FormatDate(s.TrainingDate)} {FormatTime(s.StartTime)}");

        if (selected != null)
            ShowTrainingDetails(selected);
    }

    private void OpenCancelRecordChoiceDialog()
    {
        var records = _context.ClientRecords
            .Where(r => r.IdClients == _clientId)
            .Include(r => r.Training)
            .AsEnumerable()
            .OrderByDescending(r => r.RecordDate)
            .ToList();

        if (records.Count == 0)
        {
            MessageBox.Show("У вас нет записей для отмены.", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selected = ShowChoiceWindow(
            "Отмена записи",
            "Выберите запись:",
            records,
            r => $"{r.Training.TrainingName} | {FormatDateTime(r.RecordDate)}");

        if (selected != null)
            ShowRecordDetails(selected);
    }

    private void OpenRateChoiceDialog()
    {
        var rates = _context.RateSubscriptions
            .AsEnumerable()
            .OrderBy(r => r.Cost)
            .ToList();

        if (rates.Count == 0)
        {
            MessageBox.Show("Тарифы не найдены.", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var selected = ShowChoiceWindow(
            "Покупка абонемента",
            "Выберите тариф:",
            rates,
            r => $"{r.Name} | {r.Type} | {r.Cost:0.00} руб.");

        if (selected != null)
            ConfirmBuyRate(selected);
    }

    private T? ShowChoiceWindow<T>(string title, string label, List<T> items, Func<T, string> display)
        where T : class
    {
        var window = new Window
        {
            Title = title,
            Width = 520,
            Height = 230,
            MinWidth = 520,
            MinHeight = 230,
            MaxWidth = 520,
            MaxHeight = 230,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            Background = (Brush)FindResource("PageBrush")
        };

        var root = new StackPanel
        {
            Margin = new Thickness(24)
        };

        var text = new TextBlock
        {
            Text = label,
            FontSize = 17,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 12)
        };

        var comboBox = new ComboBox
        {
            Height = 36,
            FontSize = 14,
            ItemsSource = items.Select(display).ToList(),
            SelectedIndex = 0,
            Margin = new Thickness(0, 0, 0, 22)
        };

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var ok = new Button
        {
            Content = "Выбрать",
            Width = 140,
            Height = 38,
            Style = (Style)FindResource("RoundedButton"),
            Margin = new Thickness(0, 0, 12, 0)
        };

        var cancel = new Button
        {
            Content = "Отмена",
            Width = 140,
            Height = 38,
            Style = (Style)FindResource("RoundedButton")
        };

        T? result = null;

        ok.Click += (_, _) =>
        {
            if (comboBox.SelectedIndex >= 0)
                result = items[comboBox.SelectedIndex];

            window.DialogResult = true;
            window.Close();
        };

        cancel.Click += (_, _) =>
        {
            result = null;
            window.DialogResult = false;
            window.Close();
        };

        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);

        root.Children.Add(text);
        root.Children.Add(comboBox);
        root.Children.Add(buttons);
        window.Content = root;

        window.ShowDialog();
        return result;
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_section == "Trainings") LoadTrainings();
        if (_section == "Records") LoadRecords();
        if (_section == "Rates") LoadRates();
        if (_section == "Subscriptions") LoadSubscriptions();
    }

    private void BtnWelcome_Click(object sender, RoutedEventArgs e) => SelectSection("Welcome");
    private void BtnTrainings_Click(object sender, RoutedEventArgs e) => SelectSection("Trainings");
    private void BtnRecords_Click(object sender, RoutedEventArgs e) => SelectSection("Records");
    private void BtnRates_Click(object sender, RoutedEventArgs e) => SelectSection("Rates");
    private void BtnSubscriptions_Click(object sender, RoutedEventArgs e) => SelectSection("Subscriptions");
    private void BtnLogout_Click(object sender, RoutedEventArgs e) => Close();

    private bool Match(string search, params string?[] values)
    {
        if (string.IsNullOrWhiteSpace(search)) return true;
        return values.Any(v => (v ?? "").ToLower().Contains(search));
    }

    private bool IsSubscriptionActive(Subscription s) => (s.EndDate ?? DateTime.MinValue) >= DateTime.Today;

    private DateTime CalculateEndDate(DateTime start, RateSubscription rate)
    {
        string text = (rate.Type + " " + rate.Name).ToLower();
        if (text.Contains("год")) return start.AddYears(1);
        if (text.Contains("раз")) return start.AddDays(1);
        return start.AddMonths(1);
    }

    private static string FormatDate(DateTime? d) => d?.ToString("dd.MM.yyyy") ?? "-";
    private static string FormatDateTime(DateTime? d) => d?.ToString("dd.MM.yyyy HH:mm") ?? "-";
    private static string FormatTime(TimeSpan? t) => t?.ToString(@"hh\:mm") ?? "-";
}
