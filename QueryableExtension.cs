using System.Linq.Expressions;

namespace Netcorext.Extensions.Linq;

public static class QueryableExtension
{
    public static IQueryable<TSource> In<TSource, TValue>(this IQueryable<TSource> source, Expression<Func<TSource, TValue>> member, params TValue[] values)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (member == null) throw new ArgumentNullException(nameof(member));

        var p = member.Parameters.Single();

        var equals = values.Select(value => (Expression)Expression.Equal(member.Body, Expression.Constant(value, typeof(TValue))));
        var body = equals.Aggregate(Expression.Or);

        var predicate = Expression.Lambda<Func<TSource, bool>>(body, p);

        return source.Where(predicate);
    }

    public static IQueryable<TSource> NotIn<TSource, TValue>(this IQueryable<TSource> source, Expression<Func<TSource, TValue>> member, params TValue[] values)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (member == null) throw new ArgumentNullException(nameof(member));

        var p = member.Parameters.Single();
        var equals = values.Select(value => (Expression)Expression.NotEqual(member.Body, Expression.Constant(value, typeof(TValue))));
        var body = equals.Aggregate(Expression.AndAlso);
        var predicate = Expression.Lambda<Func<TSource, bool>>(body, p);

        return source.Where(predicate);
    }

    public static IQueryable<TSource> Like<TSource, TValue>(this IQueryable<TSource> source, Expression<Func<TSource, TValue>> member, params TValue[] values)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (typeof(TValue) != typeof(string)) throw new ArgumentException("Must be a string type");

        var p = member.Parameters.Single();
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var equals = values.Select(value => (Expression)Expression.Call(member.Body, containsMethod, Expression.Constant(value, typeof(TValue))));
        var body = equals.Aggregate(Expression.Or);
        var predicate = Expression.Lambda<Func<TSource, bool>>(body, p);

        return source.Where(predicate);
    }

    public static IQueryable<TSource> NotLike<TSource, TValue>(this IQueryable<TSource> source, Expression<Func<TSource, TValue>> member, params TValue[] values)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (member == null) throw new ArgumentNullException(nameof(member));
        if (typeof(TValue) != typeof(string)) throw new ArgumentException("Must be a string type");

        var p = member.Parameters.Single();
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var equals = values.Select(value => (Expression)Expression.Not(Expression.Call(member.Body, containsMethod, Expression.Constant(value, typeof(TValue)))));
        var body = equals.Aggregate(Expression.And);
        var predicate = Expression.Lambda<Func<TSource, bool>>(body, p);

        return source.Where(predicate);
    }

    public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, Func<TOuter, TInner> defaultValue = null)
    {
        return from o in outer
               join i in inner on outerKeySelector(o) equals innerKeySelector(i) into g
               from i in g.DefaultIfEmpty(defaultValue == null ? default : defaultValue.Invoke(o))
               select resultSelector(o, i);
    }

    public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, Func<TOuter, TInner> defaultValue = null)
    {
        return from o in outer
               join i in inner on outerKeySelector(o) equals innerKeySelector(i) into g
               from i in g.DefaultIfEmpty(defaultValue == null ? default : defaultValue.Invoke(o))
               select resultSelector(o, i);
    }

    public static IEnumerable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, Func<TInner, TOuter> defaultValue = null)
    {
        return from i in inner
               join o in outer on innerKeySelector(i) equals outerKeySelector(o) into g
               from o in g.DefaultIfEmpty(defaultValue == null ? default : defaultValue.Invoke(i))
               select resultSelector(o, i);
    }

    public static IQueryable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, Func<TInner, TOuter> defaultValue = null)
    {
        return from i in inner
               join o in outer on innerKeySelector(i) equals outerKeySelector(o) into g
               from o in g.DefaultIfEmpty(defaultValue == null ? default : defaultValue.Invoke(i))
               select resultSelector(o, i);
    }
}