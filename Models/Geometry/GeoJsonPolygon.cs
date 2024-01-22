using System.ComponentModel.DataAnnotations.Schema;
using NetBackend.Models.Geometry;

public class GeoJsonPolygon : GeoJsonGeometry
{
    [NotMapped]
    public List<List<List<double>>> Coordinates { get; set; }

    public GeoJsonPolygon()
    {
        Type = "Polygon";
        Coordinates = new List<List<List<double>>>();
    }
}