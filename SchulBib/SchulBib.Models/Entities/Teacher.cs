using SchulBib.Models.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

public class Teacher : BaseEntity
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty; // BCrypt Hash

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    public TeacherRole Role { get; set; } = TeacherRole.Librarian;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    // Computed Property
    public string FullName => $"{FirstName} {LastName}";
}
