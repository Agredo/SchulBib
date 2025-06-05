using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SchulBib.Data.Extensions;

public static class IQueryableExtension
{
    /// <summary>
    /// Extends the query to include multiple related entities specified by the provided expressions.
    /// </summary>
    /// <remarks>This method is typically used in Entity Framework queries to eagerly load related entities.
    /// Each expression in <paramref name="includes"/> should specify a navigation property to include.</remarks>
    /// <typeparam name="T">The type of the entity being queried.</typeparam>
    /// <param name="query">The <see cref="IQueryable{T}"/> instance to apply the includes to.</param>
    /// <param name="includes">An array of expressions specifying the related entities to include.</param>
    /// <returns>A new <see cref="IQueryable{T}"/> with the specified related entities included. If <paramref name="includes"/>
    /// is null or empty, the original query is returned unchanged.</returns>
    public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, params Expression<Func<T, object>>[] includes)
    where T : class
    {
        if (includes != null && includes.Length > 0)
        {
            query = includes.Aggregate(query,
                      (current, include) => current.Include(include));
        }

        return query;
    }
}