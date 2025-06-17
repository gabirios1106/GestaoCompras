using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.Utils.Extentions;
public static class QueryableExtension
{
    public static IQueryable<T> OrderByField<T>(this IQueryable<T> source, string orderBy)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (string.IsNullOrEmpty(orderBy))
            throw new ArgumentNullException(nameof(source));

        if (!orderBy.Contains('-'))
            throw new ArgumentException("Parâmetro com formato inválido", nameof(orderBy));

        var orderByParams = orderBy.Split('-');

        if (orderByParams.Length != 2)
            throw new ArgumentException("Parâmetro com formato inválido", nameof(orderBy));

        var method = orderByParams[0].Equals("ASC", StringComparison.CurrentCultureIgnoreCase) ? "OrderBy" : "OrderByDescending";

        var parameter = Expression.Parameter(typeof(T), "param");
        var property = Expression.Property(parameter, orderByParams[1]);
        var expression = Expression.Lambda(property, parameter);

        var types = new Type[] { source.ElementType, expression.Body.Type };

        var methodCallExpression = Expression.Call(typeof(Queryable), method, types, source.Expression, expression);

        return source.Provider.CreateQuery<T>(methodCallExpression);
    }
}
