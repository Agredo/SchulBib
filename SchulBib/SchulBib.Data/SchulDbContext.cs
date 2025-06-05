using Microsoft.EntityFrameworkCore;
using SchulBib.Models.Entities;

namespace SchulBib.Data;

public class SchulBibDbContext : DbContext
{
    public string DatabasePath { get; private set; } = "schulbib.db";
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }
    public DbSet<BookReservation> BookReservations { get; set; }

    public SchulBibDbContext()
    {
    }

    public SchulBibDbContext(DbContextOptions<SchulBibDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        ////Needed to create Migrations and update the database
        ////Comment this line out after creating a migration. The connection string will be set in the MauiProgram.cs
        ////A schulbib.db file will be created within the project SchulBib.Data. Make sure to delete it.
        //optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Migrates the database to the latest version and ensures it is created.
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        var path = Database.GetConnectionString();
        Database.Migrate();
    }
}
