using SchulBib.Models.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

/// <summary>
/// Repräsentiert ein physisches Buchexemplar in der Bibliothek.
/// Jedes Exemplar hat einen eigenen QR-Code und kann individuell ausgeliehen werden.
/// </summary>
public class Book : BaseEntity
{
    [Required]
    public Guid BookTitleId { get; set; }

    [Required, MaxLength(200)]
    public string QrCode { get; set; } = string.Empty;

    public BookStatus Status { get; set; } = BookStatus.Available;

    public BookCondition Condition { get; set; } = BookCondition.Good;

    [MaxLength(50)]
    public string? Location { get; set; } // Regal/Standort in der Bibliothek

    [MaxLength(50)]
    public string? InventoryNumber { get; set; } // Inventarnummer

    public DateTime? AcquisitionDate { get; set; } // Anschaffungsdatum

    public decimal? PurchasePrice { get; set; } // Anschaffungspreis

    [MaxLength(500)]
    public string? Notes { get; set; } // Notizen zum Exemplar

    // Navigation Properties
    public virtual BookTitle BookTitle { get; set; } = null!;
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public virtual ICollection<BookReservation> Reservations { get; set; } = new List<BookReservation>();

    // Computed Properties
    public bool IsAvailable => Status == BookStatus.Available;
    public string DisplayName => BookTitle != null ? $"{BookTitle.Title} (#{InventoryNumber ?? QrCode})" : QrCode;

    // Helper Properties für einfacheren Zugriff (delegiert an BookTitle)
    public string Title => BookTitle?.Title ?? string.Empty;
    public string? Author => BookTitle?.Author;
    public string? ISBN => BookTitle?.ISBN;
    public string? Publisher => BookTitle?.Publisher;
    public int? PublicationYear => BookTitle?.PublicationYear;
}