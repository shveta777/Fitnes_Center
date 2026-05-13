using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models;

[Table("Client_Records_Training")]
public class ClientRecordsTraining
{
    [Key][Column("ID_Client_Records_Training")] public int IdClientRecordsTraining { get; set; }
    [Column("ID_Training")] public int IdTraining { get; set; }
    [Column("ID_Clients")] public int IdClients { get; set; }
    [Column("Record_date")] public DateTime? RecordDate { get; set; }
    [Column("Presence")][MaxLength(20)] public string? Presence { get; set; }
    [ForeignKey("IdTraining")] public Training Training { get; set; } = null!;
    [ForeignKey("IdClients")] public Client Client { get; set; } = null!;
}
