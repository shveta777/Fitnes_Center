using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;

[Table("Trainer_Availability")]
public class TrainerAvailability
{
    [Key]
    [Column("ID_Trainer_Availability")]
    public int IdTrainerAvailability { get; set; }

    [Column("ID_Trainer")]
    public int IdTrainer { get; set; }

    [Column("Date")]
    public DateTime? Date { get; set; }

    [Column("Work_start_time")]
    public TimeSpan? WorkStartTime { get; set; }

    [Column("Work_end_time")]
    public TimeSpan? WorkEndTime { get; set; }

    [ForeignKey(nameof(IdTrainer))]
    public Trainer Trainer { get; set; } = null!;
}
