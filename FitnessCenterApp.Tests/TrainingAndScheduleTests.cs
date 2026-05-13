using FitnessCenterApp.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessCenterApp.Tests;

/// <summary>
/// Тесты проверяют работу с тренировками и расписанием.
/// </summary>
public class TrainingAndScheduleTests
{
    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void AddTraining_WithoutDuration_TrainingSaved()
    {
        using var db = TestDbFactory.Create();

        db.Trainings.Add(new Training
        {
            TrainingName = "Йога",
            Category = "Групповая",
            Intensity = "Низкая",
            MaxParticipants = 12,
            Place = "Зал 1"
        });
        db.SaveChanges();

        var training = db.Trainings.Single();
        Assert.Equal("Йога", training.TrainingName);
        Assert.Equal("Групповая", training.Category);
    }

    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void AddSchedule_WithTraining_ScheduleSaved()
    {
        using var db = TestDbFactory.Create();
        var training = new Training
        {
            TrainingName = "Кардио",
            Category = "Групповая"
        };
        db.Trainings.Add(training);
        db.SaveChanges();

        db.Schedules.Add(new Schedule
        {
            IdTraining = training.IdTraining,
            TrainingDate = new DateTime(2026, 5, 4),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(11, 0, 0),
            Status = "Запланировано"
        });
        db.SaveChanges();

        var schedule = db.Schedules.Include(x => x.Training).Single();
        Assert.Equal("Кардио", schedule.Training.TrainingName);
        Assert.Equal("Запланировано", schedule.Status);
    }
}
