using PersistanceToolkit.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceToolkit
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
        internal static void TraverseEntities(BaseEntity entity, Action<BaseEntity> action)
        {
            if (entity == null)
                return;

            action(entity);

            var props = entity.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            foreach (var prop in props)
            {
                var value = prop.GetValue(entity);
                if (value == null) continue;

                switch (value)
                {
                    case BaseEntity childEntity:
                        TraverseEntities(childEntity, action);
                        break;

                    case IEnumerable<BaseEntity> collection:
                        foreach (var item in collection)
                        {
                            if (item != null)
                                TraverseEntities(item, action);
                        }
                        break;

                    case System.Collections.IEnumerable enumerable:
                        foreach (var item in enumerable)
                        {
                            if (item is BaseEntity nested)
                                TraverseEntities(nested, action);
                        }
                        break;
                }
            }
        }
    }
}
