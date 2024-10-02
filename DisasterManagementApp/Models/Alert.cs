using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Alert
{
    [Key]
    public int AlertId { get; set; }
    
    public required string Message { get; set; } // Use 'required' to enforce initialization
    
    public DateTime AlertDate { get; set; }
    
    // Foreign Key for Incident
    [ForeignKey("Incident")]
    public int? IncidentId { get; set; }

    // Navigation Property
    public Incident? Incident { get; set; }
}
