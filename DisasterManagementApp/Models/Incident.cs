
public class Incident
{
    public int IncidentId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime DateReported { get; set; }
}
