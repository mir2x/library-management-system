using System.Linq.Expressions;

namespace LibraryManagementApi.Application.Common.Specifications;

// A reusable, composable query predicate — encapsulates a filter that would otherwise be
// duplicated as an inline .Where(...) clause across multiple handlers. Exposed as an
// Expression<Func<T, bool>> (not a compiled delegate) so it stays translatable by EF Core
// when passed straight into IQueryable<T>.Where(...).
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);
}
