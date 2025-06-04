using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

public class BookReservation : BaseEntity
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid BookId { get; set; }

    [Required]
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; } // Reservierung läuft ab

    public bool IsNotified { get; set; } = false; // Schüler benachrichtigt?

    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    // Navigation Properties
    public virtual Student Student { get; set; } = null!;
    public virtual Book Book { get; set; } = null!;

    // Computed Properties
    public bool IsActive => CancelledAt == null && ExpiresAt > DateTime.UtcNow;
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow && CancelledAt == null;
}
