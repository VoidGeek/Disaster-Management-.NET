public class User
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; } // Correct nullable syntax
    public required string Password { get; set; } // Store hashed passwords

    // Foreign key for Role
    public int? RoleId { get; set; }

    // Navigation property for Role
    public Role? Role { get; set; }
}
