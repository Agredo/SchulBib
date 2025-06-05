using Microsoft.EntityFrameworkCore;
using SchulBib.Models.Entities;
using SchulBib.Models.Entities.Enums;

namespace SchulBib.Data;

public static class SchulBibDbContextConfiguration
{
    public static void ApplyAllConfigurations(this ModelBuilder modelBuilder)
    {
        ConfigureBaseEntityBehavior(modelBuilder);
        ConfigureStudentEntity(modelBuilder);
        ConfigureTeacherEntity(modelBuilder);
        ConfigureBookEntity(modelBuilder);
        ConfigureLoanEntity(modelBuilder);
        ConfigureBookReservationEntity(modelBuilder);
        ConfigureAuditLogEntity(modelBuilder);
        ConfigureAppSettingEntity(modelBuilder);
        ConfigureRelationships(modelBuilder);
        ApplyGlobalQueryFilters(modelBuilder);
        SeedInitialData(modelBuilder);
    }

    private static void ConfigureBaseEntityBehavior(ModelBuilder modelBuilder)
    {
        // Konfiguration für alle Entitäten, die von BaseEntity erben
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsSubclassOf(typeof(BaseEntity)))
            {
                // Automatisches Setzen von UpdatedAt bei Änderungen
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("UpdatedAt")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // CreatedAt soll nur beim Erstellen gesetzt werden
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("CreatedAt")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Index auf IsDeleted für Performance bei Soft-Delete-Queries
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("IsDeleted")
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_IsDeleted");
            }
        }
    }

    private static void ConfigureStudentEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            // Tabellen-Konfiguration
            entity.ToTable("Students");

            // Unique Constraint für QR-Code
            entity.HasIndex(s => s.QrCode)
                .IsUnique()
                .HasDatabaseName("UX_Students_QrCode");

            // Index für Klassencode-Suche
            entity.HasIndex(s => s.ClassCode)
                .HasDatabaseName("IX_Students_ClassCode");

            // Kombinierter Index für aktive Schüler einer Klasse
            entity.HasIndex(s => new { s.ClassCode, s.IsActive })
                .HasDatabaseName("IX_Students_ClassCode_IsActive")
                .HasFilter("[IsActive] = 1");

            // Index für Namenssuche
            entity.HasIndex(s => s.FirstName)
                .HasDatabaseName("IX_Students_FirstName");

            // Check Constraint für Klassencode-Format
            entity.HasCheckConstraint("CK_Students_ClassCode",
                "LEN([ClassCode]) >= 2 AND LEN([ClassCode]) <= 10");
        });
    }

    private static void ConfigureTeacherEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.ToTable("Teachers");

            // Unique Constraint für Username
            entity.HasIndex(t => t.Username)
                .IsUnique()
                .HasDatabaseName("UX_Teachers_Username");

            // Index für Login-Performance
            entity.HasIndex(t => new { t.Username, t.IsActive })
                .HasDatabaseName("IX_Teachers_Username_IsActive");

            // Index für Email-Suche
            entity.HasIndex(t => t.Email)
                .HasDatabaseName("IX_Teachers_Email")
                .HasFilter("[Email] IS NOT NULL");

            // Index für Rollen-basierte Abfragen
            entity.HasIndex(t => t.Role)
                .HasDatabaseName("IX_Teachers_Role");
        });
    }

    private static void ConfigureBookEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");

            // Unique Constraint für QR-Code
            entity.HasIndex(b => b.QrCode)
                .IsUnique()
                .HasDatabaseName("UX_Books_QrCode");

            // Index für ISBN-Suche (nicht unique, da ISBN optional)
            entity.HasIndex(b => b.ISBN)
                .HasDatabaseName("IX_Books_ISBN")
                .HasFilter("[ISBN] IS NOT NULL");

            // Index für Titelsuche
            entity.HasIndex(b => b.Title)
                .HasDatabaseName("IX_Books_Title");

            // Index für Autorsuche
            entity.HasIndex(b => b.Author)
                .HasDatabaseName("IX_Books_Author")
                .HasFilter("[Author] IS NOT NULL");

            // Index für Statusabfragen
            entity.HasIndex(b => b.Status)
                .HasDatabaseName("IX_Books_Status");

            // Kombinierter Index für verfügbare Bücher
            entity.HasIndex(b => new { b.Status, b.Location })
                .HasDatabaseName("IX_Books_Status_Location")
                .HasFilter("[Status] = 0"); // 0 = Available

            // JSON-Spalte für externe Metadaten
            entity.Property(b => b.ExternalMetadata)
                .HasColumnType("nvarchar(max)");

            // Check Constraint für Publikationsjahr
            entity.HasCheckConstraint("CK_Books_PublicationYear",
                "[PublicationYear] IS NULL OR ([PublicationYear] >= 1450 AND [PublicationYear] <= YEAR(GETDATE()) + 1)");
        });
    }

    private static void ConfigureLoanEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");

            // Index für aktive Ausleihen eines Schülers
            entity.HasIndex(l => new { l.StudentId, l.Status })
                .HasDatabaseName("IX_Loans_StudentId_Status");

            // Index für Buchstatus-Abfragen
            entity.HasIndex(l => new { l.BookId, l.Status })
                .HasDatabaseName("IX_Loans_BookId_Status");

            // Index für Fälligkeitsdatum-Abfragen
            entity.HasIndex(l => new { l.DueDate, l.Status })
                .HasDatabaseName("IX_Loans_DueDate_Status")
                .HasFilter("[Status] = 0"); // 0 = Active

            // Index für Rückgabedatum
            entity.HasIndex(l => l.ReturnedAt)
                .HasDatabaseName("IX_Loans_ReturnedAt")
                .HasFilter("[ReturnedAt] IS NOT NULL");

            // Computed Column für überfällige Bücher
            entity.Property(l => l.IsOverdue)
                .HasComputedColumnSql("CASE WHEN [DueDate] < GETDATE() AND [Status] = 0 THEN 1 ELSE 0 END");

            // Check Constraint für Datum-Logik
            entity.HasCheckConstraint("CK_Loans_DateLogic",
                "[BorrowedAt] <= [DueDate] AND ([ReturnedAt] IS NULL OR [ReturnedAt] >= [BorrowedAt])");
        });
    }

    private static void ConfigureBookReservationEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookReservation>(entity =>
        {
            entity.ToTable("BookReservations");

            // Index für aktive Reservierungen eines Schülers
            entity.HasIndex(r => new { r.StudentId, r.CancelledAt })
                .HasDatabaseName("IX_BookReservations_StudentId_Active")
                .HasFilter("[CancelledAt] IS NULL");

            // Index für Buchreservierungen
            entity.HasIndex(r => new { r.BookId, r.CancelledAt })
                .HasDatabaseName("IX_BookReservations_BookId_Active")
                .HasFilter("[CancelledAt] IS NULL");

            // Index für ablaufende Reservierungen
            entity.HasIndex(r => new { r.ExpiresAt, r.IsNotified })
                .HasDatabaseName("IX_BookReservations_ExpiresAt_IsNotified")
                .HasFilter("[CancelledAt] IS NULL");

            // Check Constraint für Reservierungsdaten
            entity.HasCheckConstraint("CK_BookReservations_DateLogic",
                "[ReservedAt] < [ExpiresAt]");
        });
    }

    private static void ConfigureAuditLogEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            // Index für Lehrer-Aktivitäten
            entity.HasIndex(a => new { a.TeacherId, a.CreatedAt })
                .HasDatabaseName("IX_AuditLogs_TeacherId_CreatedAt");

            // Index für Entity-bezogene Suchen
            entity.HasIndex(a => new { a.EntityType, a.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");

            // Index für Aktionstyp-Filterung
            entity.HasIndex(a => a.Action)
                .HasDatabaseName("IX_AuditLogs_Action");

            // Index für zeitbasierte Abfragen
            entity.HasIndex(a => a.CreatedAt)
                .HasDatabaseName("IX_AuditLogs_CreatedAt")
                .IsDescending();

            // JSON-Spalte für Details
            entity.Property(a => a.Details)
                .HasColumnType("nvarchar(max)");
        });
    }

    private static void ConfigureAppSettingEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.ToTable("AppSettings");

            // Unique Constraint für Key
            entity.HasIndex(s => s.Key)
                .IsUnique()
                .HasDatabaseName("UX_AppSettings_Key");

            // Index für Kategorie-basierte Abfragen
            entity.HasIndex(s => s.Category)
                .HasDatabaseName("IX_AppSettings_Category");

            // Index für System-Settings
            entity.HasIndex(s => s.IsSystemSetting)
                .HasDatabaseName("IX_AppSettings_IsSystemSetting");
        });
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Student -> Loans
        modelBuilder.Entity<Student>()
            .HasMany(s => s.Loans)
            .WithOne(l => l.Student)
            .HasForeignKey(l => l.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Student -> BookReservations
        modelBuilder.Entity<Student>()
            .HasMany(s => s.Reservations)
            .WithOne(r => r.Student)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Book -> Loans
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Loans)
            .WithOne(l => l.Book)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        // Book -> BookReservations
        modelBuilder.Entity<Book>()
            .HasMany(b => b.Reservations)
            .WithOne(r => r.Book)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        // Teacher -> AuditLogs
        modelBuilder.Entity<Teacher>()
            .HasMany(t => t.AuditLogs)
            .WithOne(a => a.Teacher)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Global Query Filter für Soft Delete
        modelBuilder.Entity<Student>()
            .HasQueryFilter(s => !s.IsDeleted);

        modelBuilder.Entity<Teacher>()
            .HasQueryFilter(t => !t.IsDeleted);

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

    private static void SeedInitialData(ModelBuilder modelBuilder)
    {
        //// Standard-Einstellungen
        //var defaultSettings = new[]
        //{
        //    new AppSetting
        //    {
        //        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        //        Key = "LoanDurationDays",
        //        Value = "14",
        //        Description = "Standard-Ausleihdauer in Tagen",
        //        Category = SettingCategory.General,
        //        IsSystemSetting = true,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow
        //    },
        //    new AppSetting
        //    {
        //        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        //        Key = "MaxActiveLoansPerStudent",
        //        Value = "3",
        //        Description = "Maximale Anzahl gleichzeitiger Ausleihen pro Schüler",
        //        Category = SettingCategory.General,
        //        IsSystemSetting = true,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow
        //    },
        //    new AppSetting
        //    {
        //        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        //        Key = "ReservationDurationDays",
        //        Value = "3",
        //        Description = "Gültigkeitsdauer einer Reservierung in Tagen",
        //        Category = SettingCategory.General,
        //        IsSystemSetting = true,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow
        //    },
        //    new AppSetting
        //    {
        //        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        //        Key = "OpenLibraryApiEnabled",
        //        Value = "true",
        //        Description = "Open Library API für ISBN-Lookup aktivieren",
        //        Category = SettingCategory.ISBN,
        //        IsSystemSetting = true,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow
        //    }
        //};

        //modelBuilder.Entity<AppSetting>().HasData(defaultSettings);

        //// Admin-Benutzer (Passwort sollte beim ersten Start geändert werden)
        //var adminTeacher = new Teacher
        //{
        //    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
        //    Username = "admin",
        //    PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
        //    FirstName = "System",
        //    LastName = "Administrator",
        //    Email = "admin@schulbib.local",
        //    Role = TeacherRole.Admin,
        //    IsActive = true,
        //    CreatedAt = DateTime.UtcNow,
        //    UpdatedAt = DateTime.UtcNow
        //};

        //modelBuilder.Entity<Teacher>().HasData(adminTeacher);
    }
}