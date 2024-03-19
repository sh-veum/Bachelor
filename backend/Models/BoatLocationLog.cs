using System.ComponentModel.DataAnnotations;

namespace NetBackend.Models;

public class BoatLocationLog
{
    [Key]
    public int Id { get; set; }

    public required DateTimeOffset TimeStamp { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
}
