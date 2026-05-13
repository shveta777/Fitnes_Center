using FitnessCenterApp.Data;
using FitnessCenterApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FitnessCenterApp.Views;

public partial class AdminMainWindow : Window
{
    private readonly FitnessCenterDbContext _context;
    private string _entityType = "Clients";

    public AdminMainWindow(FitnessCenterDbContext context)
    {
        _context = context;
        InitializeComponent();
        LoadData();
    }

    private void SelectSection(string type, string title)
    {
        _entityType = type;
        TxtTitle.Text = title;
        TxtSearch.Text = "";
        LoadData();
    }

    private void LoadData()
    {
        ContentStack.Children.Clear();
        string search = TxtSearch.Text.Trim().ToLower();

        switch (_entityType)
        {
            case "Clients":
                foreach (var c in _context.Clients.AsEnumerable().Where(c => Match(search, c.Firstname, c.Lastname, c.Email, c.PhoneNumber)))
                    AddCard($"{c.Firstname} {c.Lastname}", $"Email: {c.Email}", $"Телефон: {c.PhoneNumber ?? "-"}",
                        $"Имя: {c.Firstname}\nФамилия: {c.Lastname}\nEmail: {c.Email}\nТелефон: {c.PhoneNumber ?? "-"}\nДата рождения: {FormatDate(c.DateOfBirth)}\nПол: {c.Gender ?? "-"}\nВес: {c.Weight}\nРост: {c.Height}", c.IdClients);
                break;

            case "Trainers":
                foreach (var t in _context.Trainers.AsEnumerable().Where(t => Match(search, t.Firstname, t.Lastname, t.Specialization, t.Email)))
                    AddCard($"{t.Firstname} {t.Lastname}", $"Специализация: {t.Specialization ?? "-"}", $"Email: {t.Email ?? "-"}",
                        $"Имя: {t.Firstname}\nФамилия: {t.Lastname}\nСпециализация: {t.Specialization ?? "-"}\nEmail: {t.Email ?? "-"}", t.IdTrainer);
                break;

            case "Trainings":
                foreach (var t in _context.Trainings.AsEnumerable().Where(t => Match(search, t.TrainingName, t.Category, t.Intensity, t.Place)))
                    AddCard(t.TrainingName, $"Категория: {t.Category}", $"Место: {t.Place ?? "-"}",
                        $"Название: {t.TrainingName}\nКатегория: {t.Category}\nИнтенсивность: {t.Intensity ?? "-"}\nМаксимум участников: {t.MaxParticipants}\nМесто: {t.Place ?? "-"}\nОписание: {t.Description ?? "-"}", t.IdTraining);
                break;

            case "Schedules":
                foreach (var s in _context.Schedules.Include(s => s.Training).AsEnumerable().Where(s => Match(search, s.Training.TrainingName, s.Status, FormatDate(s.TrainingDate))))
                    AddCard(s.Training.TrainingName, $"Дата: {FormatDate(s.TrainingDate)}", $"{FormatTime(s.StartTime)} - {FormatTime(s.EndTime)}",
                        $"Тренировка: {s.Training.TrainingName}\nДата: {FormatDate(s.TrainingDate)}\nНачало: {FormatTime(s.StartTime)}\nКонец: {FormatTime(s.EndTime)}\nСтатус: {s.Status ?? "-"}", s.IdSchedule);
                break;

            case "ScheduleTrainers":
                foreach (var st in _context.ScheduleTrainers.Include(st => st.Schedule).ThenInclude(s => s.Training).Include(st => st.Trainer).AsEnumerable()
                    .Where(st => Match(search, st.Trainer.Firstname, st.Trainer.Lastname, st.Schedule.Training.TrainingName)))
                    AddCard($"{st.Trainer.Firstname} {st.Trainer.Lastname}", $"Тренировка: {st.Schedule.Training.TrainingName}", $"Дата: {FormatDate(st.Schedule.TrainingDate)}",
                        $"Тренер: {st.Trainer.Firstname} {st.Trainer.Lastname}\nТренировка: {st.Schedule.Training.TrainingName}\nДата: {FormatDate(st.Schedule.TrainingDate)}\nВремя: {FormatTime(st.Schedule.StartTime)} - {FormatTime(st.Schedule.EndTime)}", st.IdScheduleTrainer);
                break;

            case "TrainerAvailability":
                foreach (var a in _context.TrainerAvailabilities.Include(a => a.Trainer).AsEnumerable().Where(a => Match(search, a.Trainer.Firstname, a.Trainer.Lastname, FormatDate(a.Date))))
                    AddCard($"{a.Trainer.Firstname} {a.Trainer.Lastname}", $"Дата: {FormatDate(a.Date)}", $"{FormatTime(a.WorkStartTime)} - {FormatTime(a.WorkEndTime)}",
                        $"Тренер: {a.Trainer.Firstname} {a.Trainer.Lastname}\nДата: {FormatDate(a.Date)}\nНачало: {FormatTime(a.WorkStartTime)}\nКонец: {FormatTime(a.WorkEndTime)}", a.IdTrainerAvailability);
                break;

            case "Subscriptions":
                foreach (var s in _context.Subscriptions.Include(s => s.Client).Include(s => s.RateSubscription).AsEnumerable()
                    .Where(s => Match(search, s.Client.Firstname, s.Client.Lastname, s.RateSubscription.Name)))
                    AddCard(s.RateSubscription.Name, $"Клиент: {s.Client.Firstname} {s.Client.Lastname}", $"{FormatDate(s.StartDate)} — {FormatDate(s.EndDate)}",
                        $"Клиент: {s.Client.Firstname} {s.Client.Lastname}\nТариф: {s.RateSubscription.Name}\nТип: {s.RateSubscription.Type}\nСтоимость: {s.RateSubscription.Cost:0.00} руб.\nПосещений: {s.RateSubscription.NumberOfVisits}\nНачало: {FormatDate(s.StartDate)}\nКонец: {FormatDate(s.EndDate)}", s.IdSubscription);
                break;

            case "Rates":
                foreach (var r in _context.RateSubscriptions.AsEnumerable().Where(r => Match(search, r.Name, r.Type)))
                    AddCard(r.Name, $"Тип: {r.Type}", $"Посещений: {r.NumberOfVisits}",
                        $"Название: {r.Name}\nТип: {r.Type}\nСтоимость: {r.Cost:0.00} руб.\nПосещений: {r.NumberOfVisits}", r.IdRateSubscription);
                break;

            case "ClientRecords":
                foreach (var r in _context.ClientRecords.Include(r => r.Client).Include(r => r.Training).AsEnumerable()
                    .Where(r => Match(search, r.Client.Firstname, r.Client.Lastname, r.Training.TrainingName, r.Presence)))
                    AddCard(r.Training.TrainingName, $"Клиент: {r.Client.Firstname} {r.Client.Lastname}", $"Дата: {FormatDateTime(r.RecordDate)}",
                        $"Клиент: {r.Client.Firstname} {r.Client.Lastname}\nТренировка: {r.Training.TrainingName}\nКатегория: {r.Training.Category}\nДата записи: {FormatDateTime(r.RecordDate)}\nПосещение: {r.Presence ?? "Не отмечено"}", r.IdClientRecordsTraining);
                break;
        }

        if (ContentStack.Children.Count == 0)
            AddNoData("Нет данных для отображения.");
    }

    private void AddCard(string title, string info, string status, string details, int id)
    {
        var border = new Border
        {
            Style = (Style)FindResource("CardBorder"),
            Cursor = Cursors.Hand
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });

        var text = new StackPanel { Margin = new Thickness(0, 0, 20, 0) };
        text.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        text.Children.Add(new TextBlock
        {
            Text = info,
            FontSize = 13,
            Foreground = Brushes.DimGray,
            Margin = new Thickness(0, 6, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        text.Children.Add(new TextBlock
        {
            Text = status,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 6, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        var edit = new Button
        {
            Content = "Изменить",
            Style = (Style)FindResource("RoundedButton"),
            Width = 115,
            Height = 34,
            Margin = new Thickness(0, 0, 12, 0)
        };

        var del = new Button
        {
            Content = "Удалить",
            Style = (Style)FindResource("RoundedButton"),
            Width = 105,
            Height = 34
        };

        edit.Click += (_, e) =>
        {
            e.Handled = true;
            EditEntity(id);
        };

        del.Click += (_, e) =>
        {
            e.Handled = true;
            DeleteEntity(id);
        };

        buttons.Children.Add(edit);
        buttons.Children.Add(del);

        Grid.SetColumn(buttons, 1);
        grid.Children.Add(text);
        grid.Children.Add(buttons);

        border.Child = grid;
        border.MouseLeftButtonUp += (_, _) => ShowDetails(title, details);
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

    private void ShowDetails(string title, string details)
    {
        new InfoWindow(GetEntityTitle(), title, details, "ℹ", "", "Закрыть") { Owner = this }.ShowDialog();
    }

    private void AddEntity()
    {
        var win = new EditDetailWindow(_context, _entityType, null) { Owner = this };
        if (win.ShowDialog() == true) LoadData();
    }

    private void EditEntity(int id)
    {
        var win = new EditDetailWindow(_context, _entityType, id) { Owner = this };
        if (win.ShowDialog() == true) LoadData();
    }

    private void DeleteEntity(int id)
    {
        if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            switch (_entityType)
            {
                case "Clients": DeleteClientWithRelations(id); break;
                case "Trainers": DeleteTrainerWithRelations(id); break;
                case "Trainings": DeleteTrainingWithRelations(id); break;
                case "Schedules": DeleteScheduleWithRelations(id); break;
                case "ScheduleTrainers": RemoveIfFound(_context.ScheduleTrainers.Find(id)); break;
                case "TrainerAvailability": RemoveIfFound(_context.TrainerAvailabilities.Find(id)); break;
                case "Subscriptions": RemoveIfFound(_context.Subscriptions.Find(id)); break;
                case "Rates": DeleteRateWithRelations(id); break;
                case "ClientRecords": RemoveIfFound(_context.ClientRecords.Find(id)); break;
            }

            _context.SaveChanges();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления:\n{ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteClientWithRelations(int id)
    {
        _context.Subscriptions.RemoveRange(_context.Subscriptions.Where(s => s.IdClients == id));
        _context.ClientRecords.RemoveRange(_context.ClientRecords.Where(r => r.IdClients == id));
        RemoveIfFound(_context.Clients.Find(id));
    }

    private void DeleteTrainerWithRelations(int id)
    {
        _context.ScheduleTrainers.RemoveRange(_context.ScheduleTrainers.Where(st => st.IdTrainer == id));
        _context.TrainerAvailabilities.RemoveRange(_context.TrainerAvailabilities.Where(a => a.IdTrainer == id));
        RemoveIfFound(_context.Trainers.Find(id));
    }

    private void DeleteScheduleWithRelations(int id)
    {
        _context.ScheduleTrainers.RemoveRange(_context.ScheduleTrainers.Where(st => st.IdSchedule == id));
        RemoveIfFound(_context.Schedules.Find(id));
    }

    private void DeleteRateWithRelations(int id)
    {
        _context.Subscriptions.RemoveRange(_context.Subscriptions.Where(s => s.IdRateSubscription == id));
        RemoveIfFound(_context.RateSubscriptions.Find(id));
    }

    private void DeleteTrainingWithRelations(int id)
    {
        var schedules = _context.Schedules.Where(s => s.IdTraining == id).ToList();
        var ids = schedules.Select(s => s.IdSchedule).ToList();
        _context.ScheduleTrainers.RemoveRange(_context.ScheduleTrainers.Where(st => ids.Contains(st.IdSchedule)));
        _context.ClientRecords.RemoveRange(_context.ClientRecords.Where(r => r.IdTraining == id));
        _context.Schedules.RemoveRange(schedules);
        RemoveIfFound(_context.Trainings.Find(id));
    }

    private void RemoveIfFound<T>(T? entity) where T : class
    {
        if (entity != null) _context.Set<T>().Remove(entity);
    }

    private string GetEntityTitle() => _entityType switch
    {
        "Clients" => "Клиент",
        "Trainers" => "Тренер",
        "Trainings" => "Тренировка",
        "Schedules" => "Расписание",
        "ScheduleTrainers" => "Тренер в расписании",
        "TrainerAvailability" => "Занятость тренера",
        "Subscriptions" => "Абонемент",
        "Rates" => "Тариф",
        "ClientRecords" => "Запись клиента",
        _ => "Информация"
    };

    private bool Match(string search, params string?[] values)
    {
        if (string.IsNullOrWhiteSpace(search)) return true;
        return values.Any(v => (v ?? "").ToLower().Contains(search));
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e) => AddEntity();
    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadData();
    private void BtnClients_Click(object sender, RoutedEventArgs e) => SelectSection("Clients", "👥  Клиенты");
    private void BtnTrainers_Click(object sender, RoutedEventArgs e) => SelectSection("Trainers", "🏋  Тренеры");
    private void BtnTrainings_Click(object sender, RoutedEventArgs e) => SelectSection("Trainings", "📋  Тренировки");
    private void BtnSchedules_Click(object sender, RoutedEventArgs e) => SelectSection("Schedules", "📅  Расписание");
    private void BtnScheduleTrainers_Click(object sender, RoutedEventArgs e) => SelectSection("ScheduleTrainers", "🔗  Тренеры в расписании");
    private void BtnAvailability_Click(object sender, RoutedEventArgs e) => SelectSection("TrainerAvailability", "🕘  Занятость");
    private void BtnSubscriptions_Click(object sender, RoutedEventArgs e) => SelectSection("Subscriptions", "💳  Абонементы");
    private void BtnRates_Click(object sender, RoutedEventArgs e) => SelectSection("Rates", "🏷  Тарифы");
    private void BtnRecords_Click(object sender, RoutedEventArgs e) => SelectSection("ClientRecords", "📝  Записи");
    private void BtnLogout_Click(object sender, RoutedEventArgs e) => Close();

    private void BtnReport_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            FileName = "FitnessCenter_Report.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            new ExcelReportService(_context).ExportClients(dialog.FileName);
            MessageBox.Show("Отчёт сохранён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private static string FormatDate(DateTime? d) => d?.ToString("dd.MM.yyyy") ?? "-";
    private static string FormatDateTime(DateTime? d) => d?.ToString("dd.MM.yyyy HH:mm") ?? "-";
    private static string FormatTime(TimeSpan? t) => t?.ToString(@"hh\:mm") ?? "-";
}
