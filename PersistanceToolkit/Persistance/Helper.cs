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
    }
}
