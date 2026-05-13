using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;

[Table("Schedule")]
public class Schedule
{
    [Key]
    [Column("ID_Schedule")]
    public int IdSchedule { get; set; }

    [Column("ID_Training")]
    public int IdTraining { get; set; }

    [Column("Training_date")]
    public DateTime? TrainingDate { get; set; }

    [Column("Start_time")]
    public TimeSpan? StartTime { get; set; }

    [Column("End_time")]
    public TimeSpan? EndTime { get; set; }

    [Column("Status")]
    [MaxLength(20)]
    public string? Status { get; set; }

    [ForeignKey(nameof(IdTraining))]
    public Training Training { get; set; } = null!;

    public ICollection<ScheduleTrainer> ScheduleTrainers { get; set; } = new List<ScheduleTrainer>();
}
