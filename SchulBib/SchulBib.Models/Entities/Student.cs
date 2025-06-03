using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

public class Student : BaseEntity
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty; // Nur Vorname für Datenschutz

    [Required, MaxLength(10)]
    public string ClassCode { get; set; } = string.Empty; // z.B. "5A", "Q1"

    [Required, MaxLength(200)]
    public string QrCode { get; set; } = string.Empty; // Base64 oder Hash

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    // Computed Property für Display
    public string DisplayName => $"{FirstName} ({ClassCode})";
}
