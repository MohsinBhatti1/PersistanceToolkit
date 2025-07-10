namespace PersistanceToolKit.Persistence.Persistance
{
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using PersistanceToolKit.Persistence.Helpers;
    using System.Linq.Expressions;

    public static class EntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder<T> IgnoreOnUpdate<T, TProperty>(
            this EntityTypeBuilder<T> builder,
            Expression<Func<T, TProperty>> navigationProperty)
            where T : class
            where TProperty : class
        {
            var propertyName = ExpressionHelper.GetPropertyName(navigationProperty);
            NavigationIgnoreTracker.MarkIgnored<T>(propertyName);
            return builder;
        }
    }
}
