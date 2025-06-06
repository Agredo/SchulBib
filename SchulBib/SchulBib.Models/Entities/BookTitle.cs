using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

/// <summary>
/// Repräsentiert die allgemeinen Informationen eines Buchtitels.
/// Mehrere physische Exemplare (Book) können auf denselben BookTitle verweisen.
/// </summary>
public class BookTitle : BaseEntity
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

    [MaxLength(500)]
    public string? CoverImagePath { get; set; } // Lokaler Pfad zum Cover

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; } // z.B. "Roman", "Sachbuch", "Lehrbuch"

    [MaxLength(500)]
    public string? Subject { get; set; } // Fachgebiet z.B. "Mathematik", "Geschichte"

    public int? PageCount { get; set; }

    [MaxLength(50)]
    public string? AgeRecommendation { get; set; } // z.B. "ab 12 Jahren"

    // ISBN-Lookup Metadaten als JSON
    [MaxLength(4000)]
    public string? ExternalMetadata { get; set; } // JSON für Open Library Data

    public DateTime? LastISBNLookup { get; set; }

    // Navigation Properties
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    // Computed Properties
    public bool HasISBN => !string.IsNullOrEmpty(ISBN);
    public int TotalCopies => Books.Count;
    public int AvailableCopies => Books.Count(b => b.IsAvailable);
    public bool HasAvailableCopies => AvailableCopies > 0;

    // Display Property
    public string DisplayTitle => Author != null ? $"{Title} - {Author}" : Title;
}
