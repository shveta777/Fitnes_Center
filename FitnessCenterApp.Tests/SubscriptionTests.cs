using FitnessCenterApp.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessCenterApp.Tests;

/// <summary>
/// Тесты проверяют создание абонемента клиента.
/// </summary>
public class SubscriptionTests
{
    /// <summary>
    /// Проверяет один из функциональных сценариев приложения.
    /// </summary>
    [Fact]
    public void AddSubscription_WithRateAndClient_SubscriptionSaved()
    {
        using var db = TestDbFactory.Create();
        var client = new Client
        {
            Firstname = "Анна",
            Lastname = "Смирнова",
            Email = "anna@test.ru",
            Password = "Anna@123"
        };
        var rate = new RateSubscription
        {
            Name = "Базовый",
            Type = "Месячный",
            Cost = 2500,
            NumberOfVisits = 8
        };
        db.Clients.Add(client);
        db.RateSubscriptions.Add(rate);
        db.SaveChanges();

        db.Subscriptions.Add(new Subscription
        {
            IdClients = client.IdClients,
            IdRateSubscription = rate.IdRateSubscription,
            StartDate = new DateTime(2026, 5, 1),
            EndDate = new DateTime(2026, 6, 1)
        });
        db.SaveChanges();

        var subscription = db.Subscriptions
            .Include(x => x.Client)
            .Include(x => x.RateSubscription)
            .Single();

        Assert.Equal("Анна", subscription.Client.Firstname);
        Assert.Equal("Базовый", subscription.RateSubscription.Name);
    }
}
