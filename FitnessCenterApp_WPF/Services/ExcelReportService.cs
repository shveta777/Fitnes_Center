using OfficeOpenXml;
using OfficeOpenXml.Style;
using FitnessCenterApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.IO;

namespace FitnessCenterApp.Services;

public class ExcelReportService
{
    private readonly FitnessCenterDbContext _context;

    public ExcelReportService(FitnessCenterDbContext context) => _context = context;

    public void ExportClients(string filePath)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();

        var clientsSheet = package.Workbook.Worksheets.Add("Клиенты");
        FillClientsSheet(clientsSheet);

        var subscriptionsSheet = package.Workbook.Worksheets.Add("Абонементы");
        FillSubscriptionsSheet(subscriptionsSheet);

        var scheduleSheet = package.Workbook.Worksheets.Add("Расписание");
        FillScheduleSheet(scheduleSheet);

        package.SaveAs(new FileInfo(filePath));
    }

    private void FillClientsSheet(ExcelWorksheet sheet)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Имя";
        sheet.Cells[1, 3].Value = "Фамилия";
        sheet.Cells[1, 4].Value = "Email";
        sheet.Cells[1, 5].Value = "Телефон";
        sheet.Cells[1, 6].Value = "Дата рождения";
        sheet.Cells[1, 7].Value = "Пол";
        sheet.Cells[1, 8].Value = "Вес";
        sheet.Cells[1, 9].Value = "Рост";

        var clients = _context.Clients.ToList();

        for (var i = 0; i < clients.Count; i++)
        {
            var row = i + 2;
            sheet.Cells[row, 1].Value = clients[i].IdClients;
            sheet.Cells[row, 2].Value = clients[i].Firstname;
            sheet.Cells[row, 3].Value = clients[i].Lastname;
            sheet.Cells[row, 4].Value = clients[i].Email;
            sheet.Cells[row, 5].Value = clients[i].PhoneNumber;
            sheet.Cells[row, 6].Value = clients[i].DateOfBirth?.ToString("dd.MM.yyyy") ?? string.Empty;
            sheet.Cells[row, 7].Value = clients[i].Gender;
            sheet.Cells[row, 8].Value = clients[i].Weight;
            sheet.Cells[row, 9].Value = clients[i].Height;
        }

        ApplyTableStyle(sheet, 1, 1, Math.Max(clients.Count + 1, 1), 9);
    }

    private void FillSubscriptionsSheet(ExcelWorksheet sheet)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Клиент";
        sheet.Cells[1, 3].Value = "Тариф";
        sheet.Cells[1, 4].Value = "Тип";
        sheet.Cells[1, 5].Value = "Стоимость";
        sheet.Cells[1, 6].Value = "Дата начала";
        sheet.Cells[1, 7].Value = "Дата окончания";

        var subscriptions = _context.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.RateSubscription)
            .ToList();

        for (var i = 0; i < subscriptions.Count; i++)
        {
            var row = i + 2;
            var s = subscriptions[i];
            sheet.Cells[row, 1].Value = s.IdSubscription;
            sheet.Cells[row, 2].Value = $"{s.Client.Firstname} {s.Client.Lastname}";
            sheet.Cells[row, 3].Value = s.RateSubscription.Name;
            sheet.Cells[row, 4].Value = s.RateSubscription.Type;
            sheet.Cells[row, 5].Value = s.RateSubscription.Cost;
            sheet.Cells[row, 6].Value = s.StartDate?.ToString("dd.MM.yyyy") ?? string.Empty;
            sheet.Cells[row, 7].Value = s.EndDate?.ToString("dd.MM.yyyy") ?? string.Empty;
        }

        ApplyTableStyle(sheet, 1, 1, Math.Max(subscriptions.Count + 1, 1), 7);
    }

    private void FillScheduleSheet(ExcelWorksheet sheet)
    {
        sheet.Cells[1, 1].Value = "ID";
        sheet.Cells[1, 2].Value = "Тренировка";
        sheet.Cells[1, 3].Value = "Дата";
        sheet.Cells[1, 4].Value = "Начало";
        sheet.Cells[1, 5].Value = "Конец";
        sheet.Cells[1, 6].Value = "Статус";

        var schedules = _context.Schedules
            .Include(s => s.Training)
            .OrderBy(s => s.TrainingDate)
            .ThenBy(s => s.StartTime)
            .ToList();

        for (var i = 0; i < schedules.Count; i++)
        {
            var row = i + 2;
            var s = schedules[i];
            sheet.Cells[row, 1].Value = s.IdSchedule;
            sheet.Cells[row, 2].Value = s.Training.TrainingName;
            sheet.Cells[row, 3].Value = s.TrainingDate?.ToString("dd.MM.yyyy") ?? string.Empty;
            sheet.Cells[row, 4].Value = s.StartTime?.ToString(@"hh\:mm") ?? string.Empty;
            sheet.Cells[row, 5].Value = s.EndTime?.ToString(@"hh\:mm") ?? string.Empty;
            sheet.Cells[row, 6].Value = s.Status;
        }

        ApplyTableStyle(sheet, 1, 1, Math.Max(schedules.Count + 1, 1), 6);
    }

    private static void ApplyTableStyle(ExcelWorksheet sheet, int startRow, int startCol, int endRow, int endCol)
    {
        var range = sheet.Cells[startRow, startCol, endRow, endCol];

        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

        range.Style.Border.Top.Color.SetColor(Color.Black);
        range.Style.Border.Bottom.Color.SetColor(Color.Black);
        range.Style.Border.Left.Color.SetColor(Color.Black);
        range.Style.Border.Right.Color.SetColor(Color.Black);

        var header = sheet.Cells[startRow, startCol, startRow, endCol];
        header.Style.Font.Bold = true;
        header.Style.Fill.PatternType = ExcelFillStyle.Solid;
        header.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(223, 238, 228));

        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }
}
