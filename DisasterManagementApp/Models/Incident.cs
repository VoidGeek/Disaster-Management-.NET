public class Incident
{
    public int IncidentId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime DateReported { get; set; }
    public int? UserId { get; set; }

    // Property to store the URL of the uploaded image/video
    public string? FileUrl { get; set; }

    // Navigation property to User
    public User? User { get; set; }
}
