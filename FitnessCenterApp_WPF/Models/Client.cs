using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;


[Table("Clients")]
public class Client
{
    [Key]
    [Column("ID_Clients")]
    public int IdClients { get; set; }

    [Column("Firstname")]
    [MaxLength(50)]
    public string Firstname { get; set; } = string.Empty;

    [Column("Lastname")]
    [MaxLength(50)]
    public string Lastname { get; set; } = string.Empty;

    [Column("Password")]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    [Column("Email")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Column("Phone_number")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Column("Date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [Column("Gender")]
    [MaxLength(10)]
    public string? Gender { get; set; }

    [Column("Weight")]
    public decimal? Weight { get; set; }

    [Column("Height")]
    public decimal? Height { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<ClientRecordsTraining> ClientRecords { get; set; } = new List<ClientRecordsTraining>();
}
