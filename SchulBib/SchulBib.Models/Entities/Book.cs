using SchulBib.Models.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

public class Book : BaseEntity
{
    [Required, MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(13)] // ISBN-13 or ISBN-10
    public string? ISBN { get; set; }

    [MaxLength(1000)]
    public string? Author { get; set; }

    [MaxLength(200)]
    public string? Publisher { get; set; }

    public int? PublicationYear { get; set; }

    [MaxLength(50)]
    public string? Language { get; set; } = "de"; // Default Deutsch

    [Required, MaxLength(200)]
    public string QrCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CoverImagePath { get; set; } // Lokaler Pfad zum Cover

    [MaxLength(2000)]
    public string? Description { get; set; }

    public BookStatus Status { get; set; } = BookStatus.Available;

    public BookCondition Condition { get; set; } = BookCondition.Good; // Zustand des Buches

    [MaxLength(50)]
    public string? Location { get; set; } // Regal/Standort in der Bibliothek

    // ISBN-Lookup Metadaten als JSON
    [MaxLength(4000)]
    public string? ExternalMetadata { get; set; } // JSON für Open Library Data

    public DateTime? LastISBNLookup { get; set; }

    // Navigation Properties
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public virtual ICollection<BookReservation> Reservations { get; set; } = new List<BookReservation>();

    // Computed Properties
    public bool IsAvailable => Status == BookStatus.Available;
    public bool HasISBN => !string.IsNullOrEmpty(ISBN);
}