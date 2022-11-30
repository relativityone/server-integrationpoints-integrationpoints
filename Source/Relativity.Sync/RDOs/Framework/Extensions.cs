using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Relativity.Sync.RDOs.Framework
{
    internal static class Extensions
    {
        public static Guid GetTypeGuid<TRdo>(this IRdoGuidProvider provider) where TRdo : IRdoType
        {
            return provider.GetValue<TRdo>().TypeGuid;
        }

        public static Guid GetGuidFromFieldExpression<TRdo, T>(this IRdoGuidProvider provider,
            Expression<Func<TRdo, T>> expression) where TRdo : IRdoType
        {
            var memberExpression =
                ((expression.Body as UnaryExpression)?.Operand as MemberExpression)
                ?? (expression.Body as MemberExpression)
                ?? throw new InvalidExpressionException($"Expression must be a unary member expression or property expression: {expression}");

            if (memberExpression.Member.Name == nameof(IRdoType.ArtifactId))
            {
                throw new InvalidExpressionException($"{nameof(IRdoType.ArtifactId)} member is not valid for Guid query");
            }

            return provider.GetValue<TRdo>().Fields.Values.First(x => x.PropertyInfo.Name == memberExpression.Member.Name).Guid;
        }
    }
}
