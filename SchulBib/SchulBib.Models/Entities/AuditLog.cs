using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

public class AuditLog : BaseEntity
{
    [Required]
    public Guid TeacherId { get; set; }

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN

    [Required, MaxLength(50)]
    public string EntityType { get; set; } = string.Empty; // Student, Book, Loan

    public Guid? EntityId { get; set; }

    [MaxLength(2000)]
    public string? Details { get; set; } // JSON mit Änderungsdetails

    [Required, MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // Navigation Properties
    public virtual Teacher Teacher { get; set; } = null!;
}
