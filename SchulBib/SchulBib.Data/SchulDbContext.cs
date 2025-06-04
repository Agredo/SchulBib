using Microsoft.EntityFrameworkCore;
using SchulBib.Models.Entities;

namespace SchulBib.Data;

public class SchulBibDbContext : DbContext
{
    public string DataBasePath { get; private set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }
    public DbSet<BookReservation> BookReservations { get; set; }

    public SchulBibDbContext(DbContextOptions<SchulBibDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        DataBasePath = Database.GetConnectionString();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public void EnsureDatabaseCreated()
    {
        Database.Migrate();
    }
}
