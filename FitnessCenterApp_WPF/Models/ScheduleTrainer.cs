using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;

[Table("Schedule_Trainer")]
public class ScheduleTrainer
{
    [Key][Column("ID_Schedule_Trainer")] public int IdScheduleTrainer { get; set; }
    [Column("ID_Schedule")] public int IdSchedule { get; set; }
    [Column("ID_Trainer")] public int IdTrainer { get; set; }
    [ForeignKey("IdSchedule")] public Schedule Schedule { get; set; } = null!;
    [ForeignKey("IdTrainer")] public Trainer Trainer { get; set; } = null!;
}
