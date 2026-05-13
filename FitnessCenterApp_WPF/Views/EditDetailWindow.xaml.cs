using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace FitnessCenterApp.Views;

public partial class EditDetailWindow : Window
{
    private readonly FitnessCenterDbContext _context;
    private readonly string _entityType;
    private readonly int? _id;
    private readonly int? _currentTrainerId;
    private readonly Dictionary<string, Control> _fields = new();

    public EditDetailWindow(
        FitnessCenterDbContext context,
        string entityType,
        int? id,
        int? currentTrainerId = null)
    {
        _context = context;
        _entityType = entityType;
        _id = id;
        _currentTrainerId = currentTrainerId;

        InitializeComponent();

        TitleText.Text = id.HasValue ? "Редактирование записи" : "Добавление записи";

        BuildFields();

        if (_id.HasValue)
            LoadData();
    }

    private void BuildFields()
    {
        FieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
        FieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        string[] fields = _entityType switch
        {
            "Clients" => new[] { "Firstname", "Lastname", "Email", "Password", "PhoneNumber", "DateOfBirth", "Gender", "Weight", "Height" },
            "Trainers" => new[] { "Firstname", "Lastname", "Specialization", "Email", "Password" },
            "Trainings" => new[] { "TrainingName", "Description", "Category", "Intensity", "MaxParticipants", "Place" },
            "Rates" => new[] { "Name", "Type", "Cost", "NumberOfVisits" },
            "Schedules" => new[] { "IdTraining", "TrainingDate", "StartTime", "EndTime", "Status" },
            "ScheduleTrainers" => _currentTrainerId.HasValue
                ? new[] { "IdSchedule" }
                : new[] { "IdSchedule", "IdTrainer" },
            "TrainerAvailability" => _currentTrainerId.HasValue
                ? new[] { "Date", "WorkStartTime", "WorkEndTime" }
                : new[] { "IdTrainer", "Date", "WorkStartTime", "WorkEndTime" },
            "Subscriptions" => new[] { "IdClients", "IdRateSubscription", "StartDate", "EndDate" },
            "ClientRecords" => new[] { "IdClients", "IdTraining", "RecordDate", "Presence" },
            _ => Array.Empty<string>()
        };

        for (int i = 0; i < fields.Length; i++)
        {
            FieldsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(46) });

            var label = new TextBlock
            {
                Text = GetFieldTitle(fields[i]) + (IsRequired(fields[i]) ? ":*" : ":"),
                FontSize = 15,
                VerticalAlignment = VerticalAlignment.Center
            };

            Control input = CreateInput(fields[i]);

            Grid.SetRow(label, i);
            Grid.SetColumn(label, 0);
            Grid.SetRow(input, i);
            Grid.SetColumn(input, 1);

            FieldsGrid.Children.Add(label);
            FieldsGrid.Children.Add(input);
            _fields[fields[i]] = input;
        }

        Height = Math.Max(430, Math.Min(760, 190 + fields.Length * 46));
        MinHeight = Height;
    }

    private Control CreateInput(string field)
    {
        if (field == "Presence")
        {
            return new ComboBox
            {
                Height = 34,
                FontSize = 14,
                Padding = new Thickness(8, 4, 8, 4),
                VerticalAlignment = VerticalAlignment.Center,
                ItemsSource = new[] { "Присутствовал", "Отсутствовал", "Не отмечено" },
                SelectedIndex = 2
            };
        }

        if (field == "Gender")
        {
            return new ComboBox
            {
                Height = 34,
                FontSize = 14,
                Padding = new Thickness(8, 4, 8, 4),
                VerticalAlignment = VerticalAlignment.Center,
                ItemsSource = new[] { "Ж", "М" }
            };
        }

        if (field == "Status")
        {
            return CreateTextCombo(new[] { "Запланировано", "Отменено", "Завершено" }, 0);
        }

        if (field == "Category")
        {
            return CreateTextCombo(new[] { "Йога", "Кардио", "Силовая", "Функциональная", "Пилатес", "Танцы", "Фитнес" }, 0);
        }

        if (field == "Intensity")
        {
            return CreateTextCombo(new[] { "Низкая", "Средняя", "Высокая" }, 1);
        }

        if (field == "Place")
        {
            return CreateTextCombo(new[] { "Зал 1", "Зал 2", "Зал 3", "Зал 4", "Бассейн", "Студия" }, 0);
        }

        if (field == "IdTraining")
            return CreateCombo(GetTrainingItems());

        if (field == "IdSchedule")
            return CreateCombo(GetScheduleItems());

        if (field == "IdTrainer")
            return CreateCombo(GetTrainerItems());

        if (field == "IdClients")
            return CreateCombo(GetClientItems());

        if (field == "IdRateSubscription")
            return CreateCombo(GetRateItems());

        var textBox = new TextBox
        {
            Height = 34,
            FontSize = 14,
            Padding = new Thickness(8, 4, 8, 4),
            VerticalAlignment = VerticalAlignment.Center
        };

        if (field == "PhoneNumber")
        {
            textBox.MaxLength = 18;
            textBox.TextChanged += PhoneTextBox_TextChanged;
        }

        if (field == "DateOfBirth" || field.Contains("Date") || field == "Date")
        {
            textBox.MaxLength = field == "RecordDate" ? 16 : 10;
            textBox.TextChanged += DateTextBox_TextChanged;
            textBox.ToolTip = field == "RecordDate" ? "Формат: дд.мм.гггг чч:мм" : "Формат: дд.мм.гггг";
        }

        if (field.Contains("Time"))
        {
            textBox.MaxLength = 5;
            textBox.TextChanged += TimeTextBox_TextChanged;
            textBox.ToolTip = "Формат: чч:мм";
        }

        return textBox;
    }

    private ComboBox CreateCombo(List<SelectItem> items)
    {
        return new ComboBox
        {
            Height = 34,
            FontSize = 14,
            Padding = new Thickness(8, 4, 8, 4),
            VerticalAlignment = VerticalAlignment.Center,
            ItemsSource = items,
            DisplayMemberPath = nameof(SelectItem.Text),
            SelectedValuePath = nameof(SelectItem.Id),
            SelectedIndex = items.Count > 0 ? 0 : -1
        };
    }

    private ComboBox CreateTextCombo(string[] items, int selectedIndex)
    {
        return new ComboBox
        {
            Height = 34,
            FontSize = 14,
            Padding = new Thickness(8, 4, 8, 4),
            VerticalAlignment = VerticalAlignment.Center,
            ItemsSource = items,
            SelectedIndex = selectedIndex
        };
    }

    private List<SelectItem> GetTrainingItems()
    {
        return _context.Trainings
            .OrderBy(t => t.TrainingName)
            .Select(t => new SelectItem
            {
                Id = t.IdTraining,
                Text = t.TrainingName
            })
            .ToList();
    }

    private List<SelectItem> GetTrainerItems()
    {
        return _context.Trainers
            .OrderBy(t => t.Lastname)
            .ThenBy(t => t.Firstname)
            .Select(t => new SelectItem
            {
                Id = t.IdTrainer,
                Text = (t.Firstname + " " + t.Lastname).Trim()
            })
            .ToList();
    }

    private List<SelectItem> GetClientItems()
    {
        return _context.Clients
            .OrderBy(c => c.Lastname)
            .ThenBy(c => c.Firstname)
            .Select(c => new SelectItem
            {
                Id = c.IdClients,
                Text = (c.Firstname + " " + c.Lastname).Trim()
            })
            .ToList();
    }

    private List<SelectItem> GetRateItems()
    {
        return _context.RateSubscriptions
            .OrderBy(r => r.Name)
            .Select(r => new SelectItem
            {
                Id = r.IdRateSubscription,
                Text = r.Name + " | " + r.Type
            })
            .ToList();
    }

    private List<SelectItem> GetScheduleItems()
    {
        return _context.Schedules
            .OrderBy(s => s.TrainingDate)
            .ThenBy(s => s.StartTime)
            .Select(s => new SelectItem
            {
                Id = s.IdSchedule,
                Text =
                    (s.Training.TrainingName ?? "Тренировка") + " | " +
                    (s.TrainingDate.HasValue ? s.TrainingDate.Value.ToString("dd.MM.yyyy") : "-") + " " +
                    (s.StartTime.HasValue ? s.StartTime.Value.ToString(@"hh\:mm") : "")
            })
            .ToList();
    }

    private static string GetFieldTitle(string field)
    {
        return field switch
        {
            "Firstname" => "Имя",
            "Lastname" => "Фамилия",
            "Email" => "Email",
            "Password" => "Пароль",
            "PhoneNumber" => "Телефон",
            "DateOfBirth" => "Дата рождения",
            "Gender" => "Пол",
            "Weight" => "Вес",
            "Height" => "Рост",
            "TrainingName" => "Название",
            "Description" => "Описание",
            "Category" => "Категория",
            "Intensity" => "Интенсивность",
            "MaxParticipants" => "Максимум участников",
            "Place" => "Место",
            "Name" => "Название",
            "Type" => "Тип",
            "Cost" => "Стоимость",
            "NumberOfVisits" => "Посещений",
            "IdTraining" => "Тренировка",
            "TrainingDate" => "Дата тренировки",
            "StartTime" => "Начало",
            "EndTime" => "Конец",
            "Status" => "Статус",
            "IdSchedule" => "Расписание",
            "IdTrainer" => "Тренер",
            "Date" => "Дата",
            "WorkStartTime" => "Начало работы",
            "WorkEndTime" => "Конец работы",
            "IdClients" => "Клиент",
            "IdRateSubscription" => "Тариф",
            "StartDate" => "Дата начала",
            "EndDate" => "Дата окончания",
            "RecordDate" => "Дата записи",
            "Presence" => "Посещение",
            _ => field
        };
    }

    private bool IsRequired(string field)
    {
        return _entityType switch
        {
            "Clients" => field is "Firstname" or "Lastname" or "Email" or "Password" or "PhoneNumber" or "DateOfBirth" or "Gender",
            "Trainers" => field is "Firstname" or "Lastname" or "Email" or "Password",
            "Trainings" => field is "TrainingName" or "Category" or "Place",
            "Rates" => field is "Name" or "Type" or "Cost" or "NumberOfVisits",
            "Schedules" => field is "IdTraining" or "TrainingDate" or "StartTime" or "EndTime" or "Status",
            "ScheduleTrainers" => field is "IdSchedule" or "IdTrainer",
            "TrainerAvailability" => field is "IdTrainer" or "Date" or "WorkStartTime" or "WorkEndTime",
            "Subscriptions" => field is "IdClients" or "IdRateSubscription" or "StartDate" or "EndDate",
            "ClientRecords" => field is "IdClients" or "IdTraining" or "RecordDate",
            _ => false
        };
    }

    private bool _isFormattingPhone;
    private bool _isFormattingDate;
    private bool _isFormattingTime;

    private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isFormattingPhone || sender is not TextBox textBox)
            return;

        var digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("8"))
            digits = "7" + digits[1..];

        if (!digits.StartsWith("7"))
            digits = "7" + digits;

        if (digits.Length > 11)
            digits = digits[..11];

        _isFormattingPhone = true;
        textBox.Text = FormatPhone(digits);
        textBox.CaretIndex = textBox.Text.Length;
        _isFormattingPhone = false;
    }

    private void DateTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isFormattingDate || sender is not TextBox textBox)
            return;

        var digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

        var maxLength = textBox.MaxLength == 16 ? 12 : 8;

        if (digits.Length > maxLength)
            digits = digits[..maxLength];

        var result = "";

        for (int i = 0; i < digits.Length; i++)
        {
            if (i == 2 || i == 4)
                result += ".";

            if (i == 8)
                result += " ";

            if (i == 10)
                result += ":";

            result += digits[i];
        }

        _isFormattingDate = true;
        textBox.Text = result;
        textBox.CaretIndex = textBox.Text.Length;
        _isFormattingDate = false;
    }

    private void TimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isFormattingTime || sender is not TextBox textBox)
            return;

        var digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

        if (digits.Length > 4)
            digits = digits[..4];

        var result = "";

        for (int i = 0; i < digits.Length; i++)
        {
            if (i == 2)
                result += ":";

            result += digits[i];
        }

        _isFormattingTime = true;
        textBox.Text = result;
        textBox.CaretIndex = textBox.Text.Length;
        _isFormattingTime = false;
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

    private string Get(string name)
    {
        if (!_fields.TryGetValue(name, out var c))
            return "";

        if (c is TextBox tb)
            return tb.Text.Trim();

        if (c is ComboBox cb)
            return cb.SelectedValue?.ToString() ?? cb.SelectedItem?.ToString() ?? "";

        return "";
    }

    private void Set(string name, object? value)
    {
        if (!_fields.TryGetValue(name, out var c))
            return;

        if (c is TextBox tb)
        {
            tb.Text = value?.ToString() ?? "";
            return;
        }

        if (c is ComboBox cb)
        {
            if (value == null)
                return;

            if (int.TryParse(value.ToString(), out int id))
            {
                cb.SelectedValue = id;
                return;
            }

            cb.SelectedItem = value.ToString();
        }
    }

    private void LoadData()
    {
        switch (_entityType)
        {
            case "Clients":
                var c = _context.Clients.Find(_id);
                if (c == null) return;
                Set("Firstname", c.Firstname); Set("Lastname", c.Lastname); Set("Email", c.Email);
                Set("Password", c.Password); Set("PhoneNumber", c.PhoneNumber);
                Set("DateOfBirth", FormatDate(c.DateOfBirth)); Set("Gender", c.Gender);
                Set("Weight", c.Weight); Set("Height", c.Height);
                break;

            case "Trainers":
                var t = _context.Trainers.Find(_id);
                if (t == null) return;
                Set("Firstname", t.Firstname); Set("Lastname", t.Lastname);
                Set("Specialization", t.Specialization); Set("Email", t.Email); Set("Password", t.Password);
                break;

            case "Trainings":
                var tr = _context.Trainings.Find(_id);
                if (tr == null) return;
                Set("TrainingName", tr.TrainingName); Set("Description", tr.Description);
                Set("Category", tr.Category); Set("Intensity", tr.Intensity);
                Set("MaxParticipants", tr.MaxParticipants); Set("Place", tr.Place);
                break;

            case "Rates":
                var r = _context.RateSubscriptions.Find(_id);
                if (r == null) return;
                Set("Name", r.Name); Set("Type", r.Type); Set("Cost", r.Cost); Set("NumberOfVisits", r.NumberOfVisits);
                break;

            case "Schedules":
                var s = _context.Schedules.Find(_id);
                if (s == null) return;
                Set("IdTraining", s.IdTraining); Set("TrainingDate", FormatDate(s.TrainingDate));
                Set("StartTime", FormatTime(s.StartTime)); Set("EndTime", FormatTime(s.EndTime)); Set("Status", s.Status);
                break;

            case "ScheduleTrainers":
                var st = _context.ScheduleTrainers.Find(_id);
                if (st == null) return;
                Set("IdSchedule", st.IdSchedule);
                if (!_currentTrainerId.HasValue)
                    Set("IdTrainer", st.IdTrainer);
                break;

            case "TrainerAvailability":
                var a = _context.TrainerAvailabilities.Find(_id);
                if (a == null) return;
                if (!_currentTrainerId.HasValue)
                    Set("IdTrainer", a.IdTrainer);
                Set("Date", FormatDate(a.Date));
                Set("WorkStartTime", FormatTime(a.WorkStartTime)); Set("WorkEndTime", FormatTime(a.WorkEndTime));
                break;

            case "Subscriptions":
                var sub = _context.Subscriptions.Find(_id);
                if (sub == null) return;
                Set("IdClients", sub.IdClients); Set("IdRateSubscription", sub.IdRateSubscription);
                Set("StartDate", FormatDate(sub.StartDate)); Set("EndDate", FormatDate(sub.EndDate));
                break;

            case "ClientRecords":
                var cr = _context.ClientRecords.Find(_id);
                if (cr == null) return;
                Set("IdClients", cr.IdClients); Set("IdTraining", cr.IdTraining);
                Set("RecordDate", FormatDateTime(cr.RecordDate)); Set("Presence", cr.Presence);
                break;
        }
    }

    private bool ValidateFields()
    {
        foreach (var field in _fields.Keys)
        {
            if (IsRequired(field) && string.IsNullOrWhiteSpace(Get(field)))
            {
                ShowWarning($"Заполните поле «{GetFieldTitle(field)}».");
                return false;
            }
        }

        if (_fields.ContainsKey("Email") &&
            !System.Text.RegularExpressions.Regex.IsMatch(Get("Email"), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowWarning("Введите корректный email. Пример: user@mail.com.");
            return false;
        }

        if (_fields.ContainsKey("PhoneNumber"))
        {
            var phone = PhoneDigits(Get("PhoneNumber"));
            if (phone.Length != 11)
            {
                ShowWarning("Введите телефон полностью в формате +7 (999) 999-99-99.");
                return false;
            }
        }

        if (_fields.ContainsKey("DateOfBirth"))
        {
            var birth = ParseDate(Get("DateOfBirth"));
            if (birth == null)
            {
                ShowWarning("Введите дату рождения в формате дд.мм.гггг.");
                return false;
            }

            if (birth.Value.Date > DateTime.Today)
            {
                ShowWarning("Дата рождения не может быть в будущем.");
                return false;
            }
        }

        if (_fields.ContainsKey("Weight") && !string.IsNullOrWhiteSpace(Get("Weight")))
        {
            var weight = ParseDecimal(Get("Weight"));
            if (weight == null || weight <= 0 || weight > 300)
            {
                ShowWarning("Вес должен быть положительным числом не больше 300.");
                return false;
            }
        }

        if (_fields.ContainsKey("Height") && !string.IsNullOrWhiteSpace(Get("Height")))
        {
            var height = ParseDecimal(Get("Height"));
            if (height == null || height <= 0 || height > 250)
            {
                ShowWarning("Рост должен быть положительным числом не больше 250.");
                return false;
            }
        }

        if (_fields.ContainsKey("Cost"))
        {
            var cost = ParseDecimal(Get("Cost"));
            if (cost == null || cost < 0)
            {
                ShowWarning("Стоимость не может быть отрицательной.");
                return false;
            }
        }

        if (_fields.ContainsKey("NumberOfVisits"))
        {
            var visits = ParseInt(Get("NumberOfVisits"));
            if (visits == null || visits <= 0)
            {
                ShowWarning("Количество посещений должно быть положительным целым числом.");
                return false;
            }
        }

        if (_fields.ContainsKey("MaxParticipants"))
        {
            var max = ParseInt(Get("MaxParticipants"));
            if (max == null || max <= 0 || max > 100)
            {
                ShowWarning("Максимум участников должен быть целым числом от 1 до 100.");
                return false;
            }
        }

        foreach (var idField in new[] { "IdTraining", "IdSchedule", "IdTrainer", "IdClients", "IdRateSubscription" })
        {
            if (_fields.ContainsKey(idField) && string.IsNullOrWhiteSpace(Get(idField)))
            {
                ShowWarning($"Выберите значение в поле «{GetFieldTitle(idField)}».");
                return false;
            }

            if (_fields.ContainsKey(idField) && ParseInt(Get(idField)) == null)
            {
                ShowWarning($"Выберите корректное значение в поле «{GetFieldTitle(idField)}».");
                return false;
            }
        }

        foreach (var dateField in new[] { "TrainingDate", "Date", "StartDate", "EndDate", "RecordDate" })
        {
            if (_fields.ContainsKey(dateField) && ParseDate(Get(dateField)) == null)
            {
                ShowWarning($"Поле «{GetFieldTitle(dateField)}» должно быть датой. Пример: 01.03.2024.");
                return false;
            }
        }

        foreach (var futureDateField in new[] { "TrainingDate", "Date", "StartDate", "RecordDate" })
        {
            if (_fields.ContainsKey(futureDateField))
            {
                var date = ParseDate(Get(futureDateField));

                if (date.HasValue && date.Value.Date < DateTime.Today)
                {
                    ShowWarning($"Поле «{GetFieldTitle(futureDateField)}» не может быть раньше сегодняшней даты.");
                    return false;
                }
            }
        }

        foreach (var timeField in new[] { "StartTime", "EndTime", "WorkStartTime", "WorkEndTime" })
        {
            if (_fields.ContainsKey(timeField) && ParseTime(Get(timeField)) == null)
            {
                ShowWarning($"Поле «{GetFieldTitle(timeField)}» должно быть временем. Пример: 09:00.");
                return false;
            }
        }

        if (_fields.ContainsKey("StartTime") && _fields.ContainsKey("EndTime") &&
            ParseTime(Get("StartTime")) >= ParseTime(Get("EndTime")))
        {
            ShowWarning("Время начала должно быть меньше времени окончания.");
            return false;
        }

        if (_fields.ContainsKey("WorkStartTime") && _fields.ContainsKey("WorkEndTime") &&
            ParseTime(Get("WorkStartTime")) >= ParseTime(Get("WorkEndTime")))
        {
            ShowWarning("Начало работы должно быть меньше конца работы.");
            return false;
        }

        if (_fields.ContainsKey("StartDate") && _fields.ContainsKey("EndDate") &&
            ParseDate(Get("StartDate")) > ParseDate(Get("EndDate")))
        {
            ShowWarning("Дата начала не может быть позже даты окончания.");
            return false;
        }

        return true;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateFields())
            return;

        try
        {
            if (_id.HasValue)
                UpdateEntity();
            else
                AddEntity();

            _context.SaveChanges();
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(GetFriendlyError(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddEntity()
    {
        switch (_entityType)
        {
            case "Clients":
                _context.Clients.Add(new Client
                {
                    Firstname = Get("Firstname"),
                    Lastname = Get("Lastname"),
                    Email = Get("Email"),
                    Password = Get("Password"),
                    PhoneNumber = PhoneDigits(Get("PhoneNumber")),
                    DateOfBirth = ParseDate(Get("DateOfBirth")),
                    Gender = Get("Gender"),
                    Weight = ParseDecimal(Get("Weight")),
                    Height = ParseDecimal(Get("Height"))
                });
                break;

            case "Trainers":
                _context.Trainers.Add(new Trainer
                {
                    Firstname = Get("Firstname"),
                    Lastname = Get("Lastname"),
                    Specialization = Get("Specialization"),
                    Email = Get("Email"),
                    Password = Get("Password")
                });
                break;

            case "Trainings":
                _context.Trainings.Add(new Training
                {
                    TrainingName = Get("TrainingName"),
                    Description = Get("Description"),
                    Category = Get("Category"),
                    Intensity = Get("Intensity"),
                    MaxParticipants = ParseInt(Get("MaxParticipants")),
                    Place = Get("Place")
                });
                break;

            case "Rates":
                _context.RateSubscriptions.Add(new RateSubscription
                {
                    Name = Get("Name"),
                    Type = Get("Type"),
                    Cost = ParseDecimal(Get("Cost")) ?? 0,
                    NumberOfVisits = ParseInt(Get("NumberOfVisits")) ?? 0
                });
                break;

            case "Schedules":
                _context.Schedules.Add(new Schedule
                {
                    IdTraining = ParseInt(Get("IdTraining")) ?? 0,
                    TrainingDate = ParseDate(Get("TrainingDate")),
                    StartTime = ParseTime(Get("StartTime")),
                    EndTime = ParseTime(Get("EndTime")),
                    Status = Get("Status")
                });
                break;

            case "ScheduleTrainers":
                _context.ScheduleTrainers.Add(new ScheduleTrainer
                {
                    IdSchedule = ParseInt(Get("IdSchedule")) ?? 0,
                    IdTrainer = _currentTrainerId ?? ParseInt(Get("IdTrainer")) ?? 0
                });
                break;

            case "TrainerAvailability":
                _context.TrainerAvailabilities.Add(new TrainerAvailability
                {
                    IdTrainer = _currentTrainerId ?? ParseInt(Get("IdTrainer")) ?? 0,
                    Date = ParseDate(Get("Date")),
                    WorkStartTime = ParseTime(Get("WorkStartTime")),
                    WorkEndTime = ParseTime(Get("WorkEndTime"))
                });
                break;

            case "Subscriptions":
                _context.Subscriptions.Add(new Subscription
                {
                    IdClients = ParseInt(Get("IdClients")) ?? 0,
                    IdRateSubscription = ParseInt(Get("IdRateSubscription")) ?? 0,
                    StartDate = ParseDate(Get("StartDate")),
                    EndDate = ParseDate(Get("EndDate"))
                });
                break;

            case "ClientRecords":
                _context.ClientRecords.Add(new ClientRecordsTraining
                {
                    IdClients = ParseInt(Get("IdClients")) ?? 0,
                    IdTraining = ParseInt(Get("IdTraining")) ?? 0,
                    RecordDate = ParseDate(Get("RecordDate")),
                    Presence = Get("Presence")
                });
                break;
        }
    }

    private void UpdateEntity()
    {
        switch (_entityType)
        {
            case "Clients":
                var c = _context.Clients.Find(_id);
                if (c == null) return;
                c.Firstname = Get("Firstname"); c.Lastname = Get("Lastname"); c.Email = Get("Email");
                c.Password = Get("Password"); c.PhoneNumber = PhoneDigits(Get("PhoneNumber"));
                c.DateOfBirth = ParseDate(Get("DateOfBirth")); c.Gender = Get("Gender");
                c.Weight = ParseDecimal(Get("Weight")); c.Height = ParseDecimal(Get("Height"));
                break;

            case "Trainers":
                var t = _context.Trainers.Find(_id);
                if (t == null) return;
                t.Firstname = Get("Firstname"); t.Lastname = Get("Lastname");
                t.Specialization = Get("Specialization"); t.Email = Get("Email"); t.Password = Get("Password");
                break;

            case "Trainings":
                var tr = _context.Trainings.Find(_id);
                if (tr == null) return;
                tr.TrainingName = Get("TrainingName"); tr.Description = Get("Description");
                tr.Category = Get("Category"); tr.Intensity = Get("Intensity");
                tr.MaxParticipants = ParseInt(Get("MaxParticipants")); tr.Place = Get("Place");
                break;

            case "Rates":
                var r = _context.RateSubscriptions.Find(_id);
                if (r == null) return;
                r.Name = Get("Name"); r.Type = Get("Type");
                r.Cost = ParseDecimal(Get("Cost")) ?? 0; r.NumberOfVisits = ParseInt(Get("NumberOfVisits")) ?? 0;
                break;

            case "Schedules":
                var s = _context.Schedules.Find(_id);
                if (s == null) return;
                s.IdTraining = ParseInt(Get("IdTraining")) ?? s.IdTraining;
                s.TrainingDate = ParseDate(Get("TrainingDate"));
                s.StartTime = ParseTime(Get("StartTime")); s.EndTime = ParseTime(Get("EndTime")); s.Status = Get("Status");
                break;

            case "ScheduleTrainers":
                var st = _context.ScheduleTrainers.Find(_id);
                if (st == null) return;
                st.IdSchedule = ParseInt(Get("IdSchedule")) ?? st.IdSchedule;
                if (!_currentTrainerId.HasValue)
                    st.IdTrainer = ParseInt(Get("IdTrainer")) ?? st.IdTrainer;
                break;

            case "TrainerAvailability":
                var a = _context.TrainerAvailabilities.Find(_id);
                if (a == null) return;
                if (!_currentTrainerId.HasValue)
                    a.IdTrainer = ParseInt(Get("IdTrainer")) ?? a.IdTrainer;
                a.Date = ParseDate(Get("Date"));
                a.WorkStartTime = ParseTime(Get("WorkStartTime")); a.WorkEndTime = ParseTime(Get("WorkEndTime"));
                break;

            case "Subscriptions":
                var sub = _context.Subscriptions.Find(_id);
                if (sub == null) return;
                sub.IdClients = ParseInt(Get("IdClients")) ?? sub.IdClients;
                sub.IdRateSubscription = ParseInt(Get("IdRateSubscription")) ?? sub.IdRateSubscription;
                sub.StartDate = ParseDate(Get("StartDate")); sub.EndDate = ParseDate(Get("EndDate"));
                break;

            case "ClientRecords":
                var cr = _context.ClientRecords.Find(_id);
                if (cr == null) return;
                cr.IdClients = ParseInt(Get("IdClients")) ?? cr.IdClients;
                cr.IdTraining = ParseInt(Get("IdTraining")) ?? cr.IdTraining;
                cr.RecordDate = ParseDate(Get("RecordDate")); cr.Presence = Get("Presence");
                break;
        }
    }

    private static void ShowWarning(string message)
    {
        MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private static string GetFriendlyError(Exception ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;

        if (message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("повторяющийся ключ", StringComparison.OrdinalIgnoreCase))
            return "Запись с такими данными уже существует. Проверьте email, название или другие уникальные поля.";

        if (message.Contains("CHECK", StringComparison.OrdinalIgnoreCase))
            return "Данные не соответствуют ограничениям базы. Проверьте числовые поля, дату, статус и значения из списков.";

        if (message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            return "Нельзя сохранить запись: выбранные связанные данные не найдены или уже удалены.";

        if (message.Contains("Cannot insert the value NULL", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("значение NULL", StringComparison.OrdinalIgnoreCase))
            return "Заполните все обязательные поля.";

        return "Не удалось сохранить запись. Проверьте введённые данные.";
    }

    private static DateTime? ParseDate(string value) => DateTime.TryParse(value, out var d) ? d : null;
    private static TimeSpan? ParseTime(string value) => TimeSpan.TryParse(value, out var t) ? t : null;
    private static int? ParseInt(string value) => int.TryParse(value, out var i) ? i : null;
    private static decimal? ParseDecimal(string value) => decimal.TryParse(value, out var d) ? d : null;
    private static string FormatDate(DateTime? value) => value?.ToString("dd.MM.yyyy") ?? "";
    private static string FormatDateTime(DateTime? value) => value?.ToString("dd.MM.yyyy HH:mm") ?? "";
    private static string FormatTime(TimeSpan? value) => value?.ToString(@"hh\:mm") ?? "";

    private class SelectItem
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
