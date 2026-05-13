using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;

[Table("Subscription")]
public class Subscription
{
    [Key]
    [Column("ID_Subscription")]
    public int IdSubscription { get; set; }

    [Column("ID_Rate_Subscription")]
    public int IdRateSubscription { get; set; }

    [Column("ID_Clients")]
    public int IdClients { get; set; }

    [Column("Start_date")]
    public DateTime? StartDate { get; set; }

    [Column("End_date")]
    public DateTime? EndDate { get; set; }

    [ForeignKey(nameof(IdRateSubscription))]
    public RateSubscription RateSubscription { get; set; } = null!;

    [ForeignKey(nameof(IdClients))]
    public Client Client { get; set; } = null!;
}
