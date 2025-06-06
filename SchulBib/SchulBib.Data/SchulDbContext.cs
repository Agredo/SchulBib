using AgredoApplication.MVVM.Services.Abstractions.IO;
using AgredoApplication.MVVM.Services.Abstractions.Storage;
using Microsoft.EntityFrameworkCore;
using SchulBib.Models.Entities;

namespace SchulBib.Data;

/// <summary>
/// Database context for the SchulBib (School Library) application.
/// This class manages access to all entity collections and provides methods for data operations.
/// It serves as the main entry point for database interactions using Entity Framework Core.
/// </summary>
public class SchulBibDbContext : DbContext
{
    private readonly string DATABASE_PATH_PREFERENCES_KEY = "databasePath";

    /// <summary>
    /// Gets or sets the database file path for SQLite.
    /// This property is used when creating database migrations or when using a local SQLite database.
    /// </summary>
    public string DatabasePath { get; private set; } = "schulbib.db";

    /// <summary>
    /// Gets or sets the Students entity collection.
    /// Provides access to student records in the database.
    /// </summary>
    public DbSet<Student> Students { get; set; }

    /// <summary>
    /// Gets or sets the Teachers entity collection.
    /// Provides access to teacher/staff records in the database.
    /// </summary>
    public DbSet<Teacher> Teachers { get; set; }

    /// <summary>
    /// Gets or sets the BookTitles entity collection.
    /// Provides access to book title records (general book information) in the database.
    /// </summary>
    public DbSet<BookTitle> BookTitles { get; set; }

    /// <summary>
    /// Gets or sets the Books entity collection.
    /// Provides access to book records in the database.
    /// </summary>
    public DbSet<Book> Books { get; set; }

    /// <summary>
    /// Gets or sets the Loans entity collection.
    /// Provides access to book loan records in the database.
    /// </summary>
    public DbSet<Loan> Loans { get; set; }

    /// <summary>
    /// Gets or sets the AuditLogs entity collection.
    /// Provides access to audit trail records for tracking changes in the database.
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; }

    /// <summary>
    /// Gets or sets the AppSettings entity collection.
    /// Provides access to application configuration settings stored in the database.
    /// </summary>
    public DbSet<AppSetting> AppSettings { get; set; }

    /// <summary>
    /// Gets or sets the BookReservations entity collection.
    /// Provides access to book reservation records in the database.
    /// </summary>
    public DbSet<BookReservation> BookReservations { get; set; }
    public DbContextOptions<SchulBibDbContext> Options { get; }
    public IFileSystem FileSystem { get; }
    public IPreferences Preferences { get; }

    /// <summary>
    /// Initializes a new instance of the SchulBibDbContext class.
    /// This constructor is typically used for design-time operations such as migrations.
    /// </summary>
    public SchulBibDbContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the SchulBibDbContext class with the specified options.
    /// This constructor is typically used when the context is configured by dependency injection.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public SchulBibDbContext(DbContextOptions<SchulBibDbContext> options, IFileSystem fileSystem, IPreferences preferences) : base(options)
    {
        FileSystem = fileSystem;
        Preferences = preferences;

        DatabasePath = preferences.Get(DATABASE_PATH_PREFERENCES_KEY, Path.Combine(fileSystem.AppDataDirectory, "SchulBib.sqlite"), DATABASE_PATH_PREFERENCES_KEY);

    }

    /// <summary>
    /// Configures the database connection and other options for this context.
    /// This method is called for each instance of the context that is created.
    /// </summary>
    /// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        ////Needed to create Migrations and update the database
        ////Comment this line out after creating a migration. The connection string will be set in the MauiProgram.cs
        ////A schulbib.db file will be created within the project SchulBib.Data. Make sure to delete it.
        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");

        // Development configuration
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif

        // Lazy Loading Proxies disabled for better performance
        //optionsBuilder.UseLazyLoadingProxies(false);

        // Query Tracking Behavior for Read-Only Queries
        //optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
    }

    /// <summary>
    /// Configures the model that was discovered by convention from the entity types.
    /// Override this method to further configure the model that was discovered by convention.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations defined in the SchulBibDbContextConfiguration class
        SchulBibDbContextConfiguration.ApplyAllConfigurations(modelBuilder);
    }

    /// <summary>
    /// Migrates the database to the latest version and ensures it is created.
    /// This method should be called when the application starts to ensure the database is up-to-date.
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        var path = Database.GetConnectionString();
        Database.Migrate();
    }

    /// <summary>
    /// Overrides SaveChanges to automatically update timestamps and perform audit logging.
    /// This ensures that all entities have their UpdatedAt timestamp set correctly when saved.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Overrides SaveChangesAsync to automatically update timestamps and perform audit logging.
    /// This asynchronous version ensures that all entities have their UpdatedAt timestamp set correctly when saved.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the UpdatedAt timestamps for all modified entities.
    /// This is called internally by SaveChanges and SaveChangesAsync methods.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Creates an audit log entry to track changes made by users.
    /// This helps maintain a comprehensive audit trail of all significant actions in the system.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher/user who performed the action.</param>
    /// <param name="action">The type of action performed (e.g., CREATE, UPDATE, DELETE, LOGIN).</param>
    /// <param name="entityType">The type of entity affected (e.g., Student, Book, Loan).</param>
    /// <param name="entityId">The ID of the affected entity, if applicable.</param>
    /// <param name="details">Optional details about the action, often in JSON format.</param>
    /// <param name="ipAddress">The IP address of the user who performed the action.</param>
    public void CreateAuditLog(Guid teacherId, string action, string entityType, Guid? entityId = null, string? details = null, string ipAddress = "Unknown")
    {
        var auditLog = new AuditLog
        {
            TeacherId = teacherId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = "MAUI App"
        };

        AuditLogs.Add(auditLog);
    }

    /// <summary>
    /// Performs a soft delete on an entity.
    /// Instead of physically removing the record, it marks it as deleted, which preserves data for compliance and audit purposes.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to delete, which must derive from BaseEntity.</typeparam>
    /// <param name="entity">The entity to mark as deleted.</param>
    public void SoftDelete<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        Entry(entity).State = EntityState.Modified;
    }

    /// <summary>
    /// Restores a previously soft-deleted entity.
    /// This makes the entity visible again in standard queries.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to restore, which must derive from BaseEntity.</typeparam>
    /// <param name="entity">The entity to restore from soft-deletion.</param>
    public void Restore<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        entity.IsDeleted = false;
        entity.UpdatedAt = DateTime.UtcNow;
        Entry(entity).State = EntityState.Modified;
    }
}
