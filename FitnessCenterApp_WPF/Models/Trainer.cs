using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;

[Table("Trainer")]
public class Trainer
{
    [Key]
    [Column("ID_Trainer")]
    public int IdTrainer { get; set; }

    [Column("Firstname")]
    [MaxLength(50)]
    public string Firstname { get; set; } = string.Empty;

    [Column("Lastname")]
    [MaxLength(50)]
    public string Lastname { get; set; } = string.Empty;

    [Column("Specialization")]
    [MaxLength(100)]
    public string? Specialization { get; set; }

    [Column("Email")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Column("Password")]
    [MaxLength(255)]
    public string? Password { get; set; }

    public ICollection<ScheduleTrainer> ScheduleTrainers { get; set; } = new List<ScheduleTrainer>();
    public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
}
