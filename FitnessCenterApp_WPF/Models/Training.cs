using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;

[Table("Training")]
public class Training
{
    [Key]
    [Column("ID_Training")]
    public int IdTraining { get; set; }

    [Column("Training_name")]
    [MaxLength(100)]
    public string TrainingName { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    [Column("Category")]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Column("Intensity")]
    [MaxLength(20)]
    public string? Intensity { get; set; }

    [Column("Max_participants")]
    public int? MaxParticipants { get; set; }

    [Column("Place")]
    [MaxLength(100)]
    public string? Place { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<ClientRecordsTraining> ClientRecords { get; set; } = new List<ClientRecordsTraining>();
}
