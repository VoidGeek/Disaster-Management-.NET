public class Alert
{
    public int AlertId { get; set; }
    public required string Message { get; set; } // Use 'required' to enforce initialization
    public DateTime AlertDate { get; set; }
    public int IncidentId { get; set; }
    public required Incident Incident { get; set; }
}
