using System.ComponentModel.DataAnnotations.Schema;
using NetBackend.Models.Geometry;

public class GeoJsonPoint : GeoJsonGeometry
{
    [NotMapped]
    public List<double> Coordinates { get; set; }

    public GeoJsonPoint()
    {
        Type = "Point";
        Coordinates = new List<double>();
    }
}