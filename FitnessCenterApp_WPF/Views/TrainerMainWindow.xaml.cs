using FitnessCenterApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FitnessCenterApp.Views;

public partial class TrainerMainWindow : Window
{
    private readonly FitnessCenterDbContext _context;
    private readonly int _trainerId;
    private string _section = "Schedule";

    public TrainerMainWindow(FitnessCenterDbContext context, int trainerId)
    {
        _context = context;
        _trainerId = trainerId;
        InitializeComponent();
        SelectSection("Schedule");
    }

    private void SelectSection(string section)
    {
        _section = section;
        TxtSearch.Text = "";
        BtnAdd.Visibility = section == "Availability" ? Visibility.Visible : Visibility.Collapsed;

        TxtTitle.Text = section switch
        {
            "Schedule" => "📅  Расписание",
            "Availability" => "🕘  График работы",
            "Clients" => "👥  Клиенты на тренировках",
            "Stats" => "📊  Статистика",
            _ => "Тренер"
        };

        LoadData();
    }

    private void LoadData()
    {
        ContentStack.Children.Clear();
        string search = TxtSearch.Text.Trim().ToLower();

        switch (_section)
        {
            case "Schedule":
                LoadSchedule(search);
                break;
            case "Availability":
                LoadAvailability(search);
                break;
            case "Clients":
                LoadClients(search);
                break;
            case "Stats":
                LoadStats(search);
                break;
        }

        if (ContentStack.Children.Count == 0)
            AddNoData("Нет данных для отображения.");
    }

    private void LoadSchedule(string search)
    {
        var list = _context.ScheduleTrainers
            .Include(st => st.Schedule)
            .ThenInclude(s => s.Training)
            .Where(st => st.IdTrainer == _trainerId)
            .AsEnumerable()
            .Where(st => Match(search, st.Schedule.Training.TrainingName, st.Schedule.Status, FormatDate(st.Schedule.TrainingDate)))
            .OrderBy(st => st.Schedule.TrainingDate)
            .ThenBy(st => st.Schedule.StartTime)
            .ToList();

        foreach (var st in list)
        {
            var s = st.Schedule;
            string details =
                $"Категория: {s.Training.Category}\n" +
                $"Интенсивность: {s.Training.Intensity ?? "-"}\n" +
                $"Дата: {FormatDate(s.TrainingDate)}\n" +
                $"Время: {FormatTime(s.StartTime)} - {FormatTime(s.EndTime)}\n" +
                $"Место: {s.Training.Place ?? "-"}\n" +
                $"Статус: {s.Status ?? "-"}";

            AddCard(s.Training.TrainingName, $"Дата: {FormatDate(s.TrainingDate)}",
                $"{FormatTime(s.StartTime)} - {FormatTime(s.EndTime)}", details);
        }
    }

    private void LoadAvailability(string search)
    {
        var list = _context.TrainerAvailabilities
            .Where(a => a.IdTrainer == _trainerId)
            .AsEnumerable()
            .Where(a => Match(search, FormatDate(a.Date), FormatTime(a.WorkStartTime), FormatTime(a.WorkEndTime)))
            .OrderBy(a => a.Date)
            .ToList();

        foreach (var a in list)
        {
            string details =
                $"Дата: {FormatDate(a.Date)}\n" +
                $"Начало: {FormatTime(a.WorkStartTime)}\n" +
                $"Конец: {FormatTime(a.WorkEndTime)}";

            AddCard("График работы", $"Дата: {FormatDate(a.Date)}",
                $"{FormatTime(a.WorkStartTime)} - {FormatTime(a.WorkEndTime)}", details);
        }
    }

    private void LoadClients(string search)
    {
        var records = _context.ClientRecords
            .Include(r => r.Client)
            .Include(r => r.Training)
            .AsEnumerable()
            .Where(r => Match(search, r.Client.Firstname, r.Client.Lastname, r.Training.TrainingName, r.Presence))
            .OrderByDescending(r => r.RecordDate)
            .ToList();

        foreach (var r in records)
        {
            string client = $"{r.Client.Firstname} {r.Client.Lastname}";
            string details =
                $"Клиент: {client}\n" +
                $"Телефон: {r.Client.PhoneNumber ?? "-"}\n" +
                $"Email: {r.Client.Email ?? "-"}\n" +
                $"Тренировка: {r.Training.TrainingName}\n" +
                $"Категория: {r.Training.Category}\n" +
                $"Дата записи: {FormatDateTime(r.RecordDate)}\n" +
                $"Посещение: {r.Presence ?? "Не отмечено"}";

            AddCard(client, $"Тренировка: {r.Training.TrainingName}",
                $"Посещение: {r.Presence ?? "Не отмечено"}", details);
        }
    }

    private void LoadStats(string search)
    {
        var records = _context.ClientRecords
            .Include(r => r.Training)
            .AsEnumerable()
            .Where(r => Match(search, r.Training.TrainingName, r.Presence))
            .GroupBy(r => r.Training.TrainingName)
            .Select(g => new
            {
                Name = g.Key,
                Total = g.Count(),
                Present = g.Count(x => x.Presence == "Присутствовал"),
                Absent = g.Count(x => x.Presence == "Отсутствовал")
            })
            .ToList();

        foreach (var item in records)
        {
            string details =
                $"Всего записей: {item.Total}\n" +
                $"Присутствовали: {item.Present}\n" +
                $"Отсутствовали: {item.Absent}";

            AddCard(item.Name, $"Всего записей: {item.Total}",
                $"Присутствовали: {item.Present}", details);
        }
    }

    private void AddCard(string title, string info, string status, string details)
    {
        var border = new Border
        {
            Style = (Style)FindResource("CardBorder"),
            Cursor = Cursors.Hand
        };

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        stack.Children.Add(new TextBlock { Text = info, FontSize = 13, Foreground = Brushes.DimGray, Margin = new Thickness(0, 6, 0, 0) });
        stack.Children.Add(new TextBlock { Text = status, FontSize = 13, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 6, 0, 0) });

        border.Child = stack;
        border.MouseLeftButtonUp += (_, _) =>
            new InfoWindow("Информация", title, details, "ℹ", "", "Закрыть") { Owner = this }.ShowDialog();

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

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var win = new EditDetailWindow(_context, "TrainerAvailability", null, _trainerId)
        {
            Owner = this
        };

        if (win.ShowDialog() == true)
            LoadData();
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadData();

    private void BtnSchedule_Click(object sender, RoutedEventArgs e) => SelectSection("Schedule");
    private void BtnAvailability_Click(object sender, RoutedEventArgs e) => SelectSection("Availability");
    private void BtnClients_Click(object sender, RoutedEventArgs e) => SelectSection("Clients");
    private void BtnStats_Click(object sender, RoutedEventArgs e) => SelectSection("Stats");
    private void BtnLogout_Click(object sender, RoutedEventArgs e) => Close();

    private bool Match(string search, params string?[] values)
    {
        if (string.IsNullOrWhiteSpace(search)) return true;
        return values.Any(v => (v ?? "").ToLower().Contains(search));
    }

    private static string FormatDate(DateTime? d) => d?.ToString("dd.MM.yyyy") ?? "-";
    private static string FormatDateTime(DateTime? d) => d?.ToString("dd.MM.yyyy HH:mm") ?? "-";
    private static string FormatTime(TimeSpan? t) => t?.ToString(@"hh\:mm") ?? "-";
}
