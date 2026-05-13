using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;

[Table("Rate_Subscription")]
public class RateSubscription
{
    [Key][Column("ID_Rate_Subscription")] public int IdRateSubscription { get; set; }
    [Column("Name")][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Column("Type")][MaxLength(50)] public string Type { get; set; } = string.Empty;
    [Column("Cost")] public decimal Cost { get; set; }
    [Column("Number_of_visits")] public int NumberOfVisits { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
