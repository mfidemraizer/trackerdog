using System;
using System.Linq.Expressions;
using System.Reflection;
using TrackerDog.Contracts;

namespace TrackerDog
{
    internal static class ExpressionExtensions
    {
        public static PropertyInfo ExtractProperty<T, TProperty>(this Expression<Func<T, TProperty>> propertySelector)
        {
            Contract.Requires(() => propertySelector != null, "Given property selector must not be null");

            MemberExpression propertyAccessExpr = propertySelector.Body as MemberExpression;

            if (propertyAccessExpr == null)
            {
                UnaryExpression convertExpr = propertySelector.Body as UnaryExpression;

                if (convertExpr != null)
                    propertyAccessExpr = convertExpr.Operand as MemberExpression;
            }

            Contract.Assert(() => propertyAccessExpr != null, "Given expression is not supported as property selector");

            PropertyInfo selectedProperty = propertyAccessExpr.Member as PropertyInfo;

            Contract.Assert(() => selectedProperty != null, "Selected member is not a property");

            return selectedProperty;
        }
    }
}