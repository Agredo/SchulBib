using Microsoft.EntityFrameworkCore;
using SchulBib.Models.Entities;
using SchulBib.Models.Entities.Enums;

namespace SchulBib.Data;

public static class SchulBibDbContextConfiguration
{
    public static void ApplyAllConfigurations(this ModelBuilder modelBuilder)
    {
        ConfigureBaseEntityBehavior(modelBuilder);
        ApplyGlobalQueryFilters(modelBuilder);

        ConfigureIndices(modelBuilder);
        ConfigureCheckConstraints(modelBuilder);
        ConfigurePrecisions(modelBuilder);

    }

    private static void ConfigurePrecisions(ModelBuilder modelBuilder)
    {
        // BookTitle precisions
        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(b => b.PurchasePrice)
                .HasPrecision(10, 2);
        });
    }

    private static void ConfigureCheckConstraints(ModelBuilder modelBuilder)
    {
        // BookTitle check constraints
        modelBuilder.Entity<BookTitle>(entity =>
        {
            entity.HasCheckConstraint("CK_BookTitles_PublicationYear",
                "[PublicationYear] IS NULL OR ([PublicationYear] >= 1450 AND [PublicationYear] <= YEAR(GETDATE()) + 1)");
        });
        // Loan check constraints
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasCheckConstraint("CK_Loans_DateLogic",
                "[BorrowedAt] <= [DueDate] AND ([ReturnedAt] IS NULL OR [ReturnedAt] >= [BorrowedAt])");
        });
        // BookReservation check constraints
        modelBuilder.Entity<BookReservation>(entity =>
        {
            entity.HasCheckConstraint("CK_BookReservations_DateLogic",
                "[ReservedAt] < [ExpiresAt]");
        });
    }

    private static void ConfigureIndices(ModelBuilder modelBuilder)
    {
        // BookTitle indices
        modelBuilder.Entity<BookTitle>(entity =>
        {
            entity.HasIndex(bt => bt.ISBN)
                .IsUnique()
                .HasDatabaseName("UX_BookTitles_ISBN")
                .HasFilter("[ISBN] IS NOT NULL");
            entity.HasIndex(bt => bt.Title)
                .HasDatabaseName("IX_BookTitles_Title");
            entity.HasIndex(bt => bt.Author)
                .HasDatabaseName("IX_BookTitles_Author")
                .HasFilter("[Author] IS NOT NULL");
            entity.HasIndex(bt => bt.Genre)
                .HasDatabaseName("IX_BookTitles_Genre")
                .HasFilter("[Genre] IS NOT NULL");
            entity.HasIndex(bt => bt.Subject)
                .HasDatabaseName("IX_BookTitles_Subject")
                .HasFilter("[Subject] IS NOT NULL");
        });

        // Book indices
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(b => b.BookTitleId)
                .HasDatabaseName("IX_Books_BookTitleId");
            entity.HasIndex(b => b.QrCode)
                .IsUnique()
                .HasDatabaseName("UX_Books_QrCode");
            entity.HasIndex(b => b.Status)
                .HasDatabaseName("IX_Books_Status");
            entity.HasIndex(b => new { b.BookTitleId, b.Status })
                .HasDatabaseName("IX_Books_BookTitleId_Status")
                .HasFilter("[Status] = 0");
            entity.HasIndex(b => b.Location)
                .HasDatabaseName("IX_Books_Location")
                .HasFilter("[Location] IS NOT NULL");
            entity.HasIndex(b => b.InventoryNumber)
                .IsUnique()
                .HasDatabaseName("UX_Books_InventoryNumber")
                .HasFilter("[InventoryNumber] IS NOT NULL");
        });

        // Loan indices
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasIndex(l => new { l.StudentId, l.Status })
                .HasDatabaseName("IX_Loans_StudentId_Status");
            entity.HasIndex(l => new { l.BookId, l.Status })
                .HasDatabaseName("IX_Loans_BookId_Status");
            entity.HasIndex(l => new { l.DueDate, l.Status })
                .HasDatabaseName("IX_Loans_DueDate_Status")
                .HasFilter("[Status] = 0");
            entity.HasIndex(l => l.ReturnedAt)
                .HasDatabaseName("IX_Loans_ReturnedAt")
                .HasFilter("[ReturnedAt] IS NOT NULL");
            entity.Property(l => l.IsOverdue)
                .HasComputedColumnSql("CASE WHEN [DueDate] < GETDATE() AND [Status] = 0 THEN 1 ELSE 0 END");
        });

        // BookReservation indices
        modelBuilder.Entity<BookReservation>(entity =>
        {
            entity.HasIndex(r => new { r.StudentId, r.CancelledAt })
                .HasDatabaseName("IX_BookReservations_StudentId_Active")
                .HasFilter("[CancelledAt] IS NULL");
            entity.HasIndex(r => new { r.BookId, r.CancelledAt })
                .HasDatabaseName("IX_BookReservations_BookId_Active")
                .HasFilter("[CancelledAt] IS NULL");
            entity.HasIndex(r => new { r.ExpiresAt, r.IsNotified })
                .HasDatabaseName("IX_BookReservations_ExpiresAt_IsNotified")
                .HasFilter("[CancelledAt] IS NULL");
        });

        // AuditLog indices
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => new { a.TeacherId, a.CreatedAt })
                .HasDatabaseName("IX_AuditLogs_TeacherId_CreatedAt");
            entity.HasIndex(a => new { a.EntityType, a.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");
            entity.HasIndex(a => a.Action)
                .HasDatabaseName("IX_AuditLogs_Action");
            entity.HasIndex(a => a.CreatedAt)
                .HasDatabaseName("IX_AuditLogs_CreatedAt")
                .IsDescending();
        });

        // AppSetting indices
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasIndex(s => s.Key)
                .IsUnique()
                .HasDatabaseName("UX_AppSettings_Key");
            entity.HasIndex(s => s.Category)
                .HasDatabaseName("IX_AppSettings_Category");
            entity.HasIndex(s => s.IsSystemSetting)
                .HasDatabaseName("IX_AppSettings_IsSystemSetting");
        });
    }

    private static void ConfigureBaseEntityBehavior(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsSubclassOf(typeof(BaseEntity)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("UpdatedAt")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("CreatedAt")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.IsDeleted))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_IsDeleted");
            }
        }
    }

    private static void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Teacher>()
            .HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<BookTitle>()
            .HasQueryFilter(bt => !bt.IsDeleted);
        modelBuilder.Entity<Book>()
            .HasQueryFilter(b => !b.IsDeleted);
        modelBuilder.Entity<Loan>()
            .HasQueryFilter(l => !l.IsDeleted);
        modelBuilder.Entity<BookReservation>()
            .HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<AuditLog>()
            .HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<AppSetting>()
            .HasQueryFilter(s => !s.IsDeleted);
    }
}