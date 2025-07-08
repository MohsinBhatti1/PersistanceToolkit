using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceToolkit.Persistance
{
    internal class Helper
    {
        internal static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return expression.Body switch
            {
                MemberExpression member => member.Member.Name,
                UnaryExpression { Operand: MemberExpression inner } => inner.Member.Name,
                _ => throw new ArgumentException("Invalid navigation expression")
            };
        }
        internal static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression member)
            {
                return member.Member.Name;
            }

            if (propertyExpression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberOperand)
            {
                return memberOperand.Member.Name;
            }

            throw new ArgumentException("Invalid property expression", nameof(propertyExpression));
        }
    }
}
