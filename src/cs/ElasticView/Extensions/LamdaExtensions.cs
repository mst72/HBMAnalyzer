using System;
using System.Linq.Expressions;

namespace ElasticView.Extensions
{
    public static class LamdaExtensions
    {
        public static string GetPropertyName(this Expression<Func<object>> propertyExpression)
        {
            var unaryExpression = propertyExpression.Body as UnaryExpression;
            var memberExpression = unaryExpression == null ? (MemberExpression)propertyExpression.Body : (MemberExpression)unaryExpression.Operand;

            var propertyName = memberExpression.Member.Name;

            return propertyName;
        }

        public static string GetPropertyName<T>(this Expression<Func<T, object>> propertyExpression)
        {
            var unaryExpression = propertyExpression.Body as UnaryExpression;
            var memberExpression = unaryExpression == null ? (MemberExpression)propertyExpression.Body : (MemberExpression)unaryExpression.Operand;

            var propertyName = memberExpression.Member.Name;

            return propertyName;
        }
    }
}