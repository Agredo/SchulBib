using Microsoft.EntityFrameworkCore;
using SchulBib.Models.Entities;
using SchulBib.Models.Entities.Enums;
using System.Linq;
using System.Linq.Expressions;

namespace SchulBib.Data.Extensions;

/// <summary>
/// Provides extension methods for querying the SchulBib database entities.
/// These methods extend standard Entity Framework queries with specialized, reusable filtering operations.
/// </summary>
public static class SchulBibQueryExtensions
{
    /// <summary>
    /// Retrieves only active students from the database.
    /// </summary>
    /// <param name="students">The DbSet of Student entities to filter</param>
    /// <returns>An IQueryable of Student entities that are both active and not deleted</returns>
    public static IQueryable<Student> ActiveStudents(this DbSet<Student> students)
    {
        return students.Where(s => s.IsActive && !s.IsDeleted);
    }

    /// <summary>
    /// Filters student records to those belonging to a specific class.
    /// </summary>
    /// <param name="students">The IQueryable of Student entities to filter</param>
    /// <param name="classCode">The class code to filter by (e.g., "5A", "Q1")</param>
    /// <returns>An IQueryable of Student entities filtered by the specified class code</returns>
    public static IQueryable<Student> InClass(this IQueryable<Student> students, string classCode)
    {
        return students.Where(s => s.ClassCode == classCode);
    }

    /// <summary>
    /// Retrieves books that are available for borrowing from the database.
    /// </summary>
    /// <param name="books">The DbSet of Book entities to filter</param>
    /// <returns>An IQueryable of Book entities that are available and not deleted</returns>
    public static IQueryable<Book> AvailableBooks(this DbSet<Book> books)
    {
        return books.Where(b => b.Status == BookStatus.Available && !b.IsDeleted);
    }

    /// <summary>
    /// Filters book records to those that have an ISBN number.
    /// </summary>
    /// <param name="books">The IQueryable of Book entities to filter</param>
    /// <returns>An IQueryable of Book entities that have a non-null, non-empty ISBN</returns>
    public static IQueryable<Book> WithISBN(this IQueryable<Book> books)
    {
        return books.Where(b => b.ISBN != null && b.ISBN != "");
    }

    /// <summary>
    /// Retrieves active loans from the database with related Student and Book entities.
    /// </summary>
    /// <param name="loans">The DbSet of Loan entities to filter</param>
    /// <returns>An IQueryable of Loan entities that are active and not deleted, including Student and Book navigation properties</returns>
    public static IQueryable<Loan> ActiveLoans(this DbSet<Loan> loans)
    {
        return loans
            .Where(l => l.Status == LoanStatus.Active && !l.IsDeleted)
            .Include(l => l.Student)
            .Include(l => l.Book);
    }

    /// <summary>
    /// Filters loan records to those that are overdue.
    /// </summary>
    /// <param name="loans">The IQueryable of Loan entities to filter</param>
    /// <returns>An IQueryable of Loan entities that have a past due date and are still active</returns>
    public static IQueryable<Loan> OverdueLoans(this IQueryable<Loan> loans)
    {
        return loans.Where(l => l.DueDate < DateTime.Now && l.Status == LoanStatus.Active);
    }

    /// <summary>
    /// Retrieves loans that are due soon (within a specified number of days).
    /// </summary>
    /// <param name="loans">The IQueryable of Loan entities to filter</param>
    /// <param name="daysBeforeDue">Number of days before due date to consider a loan as due soon (default: 3 days)</param>
    /// <returns>An IQueryable of active Loan entities that are due within the specified number of days</returns>
    public static IQueryable<Loan> DueSoon(this IQueryable<Loan> loans, int daysBeforeDue = 3)
    {
        var dueDate = DateTime.Now.AddDays(daysBeforeDue);
        return loans.Where(l => l.DueDate <= dueDate && l.Status == LoanStatus.Active);
    }

    /// <summary>
    /// Retrieves active book reservations with related Student and Book entities.
    /// </summary>
    /// <param name="reservations">The DbSet of BookReservation entities to filter</param>
    /// <returns>An IQueryable of BookReservation entities that are not cancelled, not expired and not deleted, including Student and Book navigation properties</returns>
    public static IQueryable<BookReservation> ActiveReservations(this DbSet<BookReservation> reservations)
    {
        return reservations
            .Where(r => r.CancelledAt == null && r.ExpiresAt > DateTime.UtcNow && !r.IsDeleted)
            .Include(r => r.Student)
            .Include(r => r.Book);
    }

    /// <summary>
    /// Filters book reservation records to those that have expired but not been cancelled.
    /// </summary>
    /// <param name="reservations">The IQueryable of BookReservation entities to filter</param>
    /// <returns>An IQueryable of BookReservation entities that have expired and have not been cancelled</returns>
    public static IQueryable<BookReservation> ExpiredReservations(this IQueryable<BookReservation> reservations)
    {
        return reservations.Where(r => r.ExpiresAt <= DateTime.UtcNow && r.CancelledAt == null);
    }

    /// <summary>
    /// Checks if a student has reached their loan limit.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="maxLoans">Maximum number of loans allowed per student (default: 3)</param>
    /// <returns>True if the student has reached or exceeded the loan limit, otherwise false</returns>
    public static async Task<bool> HasReachedLoanLimit(
        this SchulBibDbContext context,
        Guid studentId,
        int maxLoans = 3)
    {
        var activeLoansCount = await context.Loans
            .CountAsync(l => l.StudentId == studentId && l.Status == LoanStatus.Active);

        return activeLoansCount >= maxLoans;
    }

    /// <summary>
    /// Checks if a book is currently reserved.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="bookId">The unique identifier of the book</param>
    /// <returns>True if the book has an active reservation, otherwise false</returns>
    public static async Task<bool> IsBookReserved(
        this SchulBibDbContext context,
        Guid bookId)
    {
        return await context.BookReservations
            .AnyAsync(r => r.BookId == bookId &&
                          r.CancelledAt == null &&
                          r.ExpiresAt > DateTime.UtcNow);
    }

    /// <summary>
    /// Retrieves a paginated list of books with optional filtering and ordering.
    /// </summary>
    /// <param name="books">The IQueryable of Book entities to paginate</param>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="filter">Optional lambda expression to filter the books</param>
    /// <param name="orderBy">Optional lambda expression to order the books</param>
    /// <returns>A tuple containing the list of books for the requested page and the total count of matching books</returns>
    public static async Task<(List<Book> Items, int TotalCount)> GetBooksPaginated(
        this IQueryable<Book> books,
        int page,
        int pageSize,
        Expression<Func<Book, bool>>? filter = null,
        Expression<Func<Book, object>>? orderBy = null)
    {
        var query = books.AsQueryable();

        if (filter != null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync();

        if (orderBy != null)
            query = query.OrderBy(orderBy);
        else
            query = query.OrderBy(b => b.Title);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Filters audit log records to those related to a specific entity.
    /// </summary>
    /// <param name="auditLogs">The IQueryable of AuditLog entities to filter</param>
    /// <param name="entityType">The type of entity (e.g., "Student", "Book", "Loan")</param>
    /// <param name="entityId">The unique identifier of the entity</param>
    /// <returns>An IQueryable of AuditLog entities for the specified entity, ordered by creation date descending</returns>
    public static IQueryable<AuditLog> ForEntity(
        this IQueryable<AuditLog> auditLogs,
        string entityType,
        Guid entityId)
    {
        return auditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt);
    }

    /// <summary>
    /// Filters application settings by category.
    /// </summary>
    /// <param name="settings">The IQueryable of AppSetting entities to filter</param>
    /// <param name="category">The category to filter by</param>
    /// <returns>An IQueryable of AppSetting entities for the specified category</returns>
    public static IQueryable<AppSetting> ByCategory(
        this IQueryable<AppSetting> settings,
        SettingCategory category)
    {
        return settings.Where(s => s.Category == category);
    }

    /// <summary>
    /// Gets the value of a specific application setting by key.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="key">The key of the setting to retrieve</param>
    /// <param name="defaultValue">The default value to return if the setting is not found</param>
    /// <returns>The value of the setting if found, otherwise the default value</returns>
    public static async Task<string?> GetSettingValue(
        this SchulBibDbContext context,
        string key,
        string? defaultValue = null)
    {
        var setting = await context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == key);

        return setting?.Value ?? defaultValue;
    }

    /// <summary>
    /// Retrieves book titles with at least one available copy.
    /// </summary>
    /// <param name="bookTitles">The IQueryable of BookTitle entities to filter</param>
    /// <returns>An IQueryable of BookTitle entities that have at least one available book</returns>
    public static IQueryable<BookTitle> WithAvailableCopies(this IQueryable<BookTitle> bookTitles)
    {
        return bookTitles.Where(bt => bt.Books.Any(b => b.Status == BookStatus.Available && !b.IsDeleted));
    }

    /// <summary>
    /// Filters book titles by language.
    /// </summary>
    /// <param name="bookTitles">The IQueryable of BookTitle entities to filter</param>
    /// <param name="language">The language code (e.g., "de", "en")</param>
    /// <returns>An IQueryable of BookTitle entities in the specified language</returns>
    public static IQueryable<BookTitle> InLanguage(this IQueryable<BookTitle> bookTitles, string language)
    {
        return bookTitles.Where(bt => bt.Language == language);
    }

    /// <summary>
    /// Filters book titles by genre.
    /// </summary>
    /// <param name="bookTitles">The IQueryable of BookTitle entities to filter</param>
    /// <param name="genre">The genre to filter by</param>
    /// <returns>An IQueryable of BookTitle entities of the specified genre</returns>
    public static IQueryable<BookTitle> ByGenre(this IQueryable<BookTitle> bookTitles, string genre)
    {
        return bookTitles.Where(bt => bt.Genre == genre);
    }

    /// <summary>
    /// Filters book titles by subject/discipline.
    /// </summary>
    /// <param name="bookTitles">The IQueryable of BookTitle entities to filter</param>
    /// <param name="subject">The subject to filter by</param>
    /// <returns>An IQueryable of BookTitle entities of the specified subject</returns>
    public static IQueryable<BookTitle> BySubject(this IQueryable<BookTitle> bookTitles, string subject)
    {
        return bookTitles.Where(bt => bt.Subject == subject);
    }

    /// <summary>
    /// Performs an optimized search for book titles based on a search term.
    /// </summary>
    /// <param name="bookTitles">The IQueryable of BookTitle entities to search</param>
    /// <param name="searchTerm">The search term to look for in title, author, ISBN, or description</param>
    /// <returns>An IQueryable of BookTitle entities matching the search term</returns>
    public static IQueryable<BookTitle> SearchBookTitles(
        this IQueryable<BookTitle> bookTitles,
        string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return bookTitles;

        searchTerm = searchTerm.ToLower();

        return bookTitles.Where(bt =>
            bt.Title.ToLower().Contains(searchTerm) ||
            bt.Author != null && bt.Author.ToLower().Contains(searchTerm) ||
            bt.ISBN != null && bt.ISBN.Contains(searchTerm) ||
            bt.Description != null && bt.Description.ToLower().Contains(searchTerm) ||
            bt.Publisher != null && bt.Publisher.ToLower().Contains(searchTerm));
    }

    /// <summary>
    /// Gets all book titles with their book copies eagerly loaded.
    /// </summary>
    /// <param name="bookTitles">The IQueryable of BookTitle entities</param>
    /// <returns>An IQueryable of BookTitle entities with Books included</returns>
    public static IQueryable<BookTitle> IncludeBooks(this IQueryable<BookTitle> bookTitles)
    {
        return bookTitles.Include(bt => bt.Books);
    }

    /// <summary>
    /// Gets statistics for a book title.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="bookTitleId">The unique identifier of the book title</param>
    /// <returns>A tuple containing total copies, available copies, borrowed copies, and reserved copies</returns>
    public static async Task<(int Total, int Available, int Borrowed, int Reserved)> GetBookTitleStatistics(
        this SchulBibDbContext context,
        Guid bookTitleId)
    {
        var books = await context.Books
            .Where(b => b.BookTitleId == bookTitleId && !b.IsDeleted)
            .Select(b => b.Status)
            .ToListAsync();

        return (
            Total: books.Count,
            Available: books.Count(s => s == BookStatus.Available),
            Borrowed: books.Count(s => s == BookStatus.Borrowed),
            Reserved: books.Count(s => s == BookStatus.Reserved)
        );
    }

    /// <summary>
    /// Finds the best available copy of a book title (based on condition).
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="bookTitleId">The unique identifier of the book title</param>
    /// <returns>The best available book copy, or null if none available</returns>
    public static async Task<Book?> FindBestAvailableCopy(
        this SchulBibDbContext context,
        Guid bookTitleId)
    {
        return await context.Books
            .Where(b => b.BookTitleId == bookTitleId &&
                       b.Status == BookStatus.Available &&
                       !b.IsDeleted)
            .OrderBy(b => b.Condition) // Excellent = 0, Good = 1, etc.
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets popular book titles based on loan count.
    /// </summary>
    /// <param name="bookTitles">The DbSet of BookTitle entities</param>
    /// <param name="topCount">Number of top books to return</param>
    /// <param name="daysBack">Number of days to look back for loans</param>
    /// <returns>An IQueryable of popular BookTitle entities ordered by loan count</returns>
    public static IQueryable<BookTitle> GetPopularTitles(
        this DbSet<BookTitle> bookTitles,
        int topCount = 10,
        int daysBack = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-daysBack);

        return bookTitles
            .Select(bt => new
            {
                BookTitle = bt,
                LoanCount = bt.Books.SelectMany(b => b.Loans)
                    .Count(l => l.BorrowedAt >= startDate)
            })
            .OrderByDescending(x => x.LoanCount)
            .Take(topCount)
            .Select(x => x.BookTitle);
    }

    /// <summary>
    /// Retrieves the loan history for a specific student.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="includeActive">Whether to include active loans (default: true)</param>
    /// <returns>An IQueryable of Loan entities for the specified student, including Book navigation property and ordered by borrow date descending</returns>
    public static IQueryable<Loan> GetLoanHistory(
        this SchulBibDbContext context,
        Guid studentId,
        bool includeActive = true)
    {
        var query = context.Loans
            .Where(l => l.StudentId == studentId)
            .Include(l => l.Book);

        if (!includeActive)
            query = query
                .Where(l => l.Status != LoanStatus.Active)
                .Include(l => l.Book);

        return query.OrderByDescending(l => l.BorrowedAt);
    }
}