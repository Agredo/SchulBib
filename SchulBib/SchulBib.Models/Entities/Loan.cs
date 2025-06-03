using SchulBib.Models.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchulBib.Models.Entities;

public class Loan : BaseEntity
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid BookId { get; set; }

    [Required]
    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime DueDate { get; set; } // Standardmäßig +14 Tage

    public DateTime? ReturnedAt { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Active;

    [MaxLength(500)]
    public string? Notes { get; set; } // Für Beschädigungen, Verlängerungen etc.

    // Automatische Reminder-Flags
    public bool FirstReminderSent { get; set; } = false;
    public bool SecondReminderSent { get; set; } = false;
    public bool OverdueReminderSent { get; set; } = false;

    // Navigation Properties
    public virtual Student Student { get; set; } = null!;
    public virtual Book Book { get; set; } = null!;

    // Computed Properties
    public bool IsOverdue => DueDate < DateTime.Now && Status == LoanStatus.Active;
    public int DaysUntilDue => (DueDate.Date - DateTime.Now.Date).Days;
    public bool NeedsReminder => DaysUntilDue <= 3 && Status == LoanStatus.Active;
}
