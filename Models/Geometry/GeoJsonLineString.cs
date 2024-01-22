using System.ComponentModel.DataAnnotations.Schema;
using NetBackend.Models.Geometry;

public class GeoJsonLineString : GeoJsonGeometry
{
    [NotMapped]
    public List<List<double>> Coordinates { get; set; }

    public GeoJsonLineString()
    {
        Type = "LineString";
        Coordinates = new List<List<double>>();
    }
}