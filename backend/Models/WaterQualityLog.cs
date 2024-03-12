using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetBackend.Models;

public class WaterQualityLog
{
    [Key]
    public int Id { get; set; }

    [Column("Key_Timestamp")]
    public required string Key { get; set; }
    public required double Ph { get; set; }

    // Turbidity in Nephelometric Turbidity Units (NTU)
    [Column("Turbidity_NTU")]
    public required double Turbidity { get; set; }

    // Temperature in Celsius
    [Column("Temperature_C")]
    public required double Temperature { get; set; }
}
