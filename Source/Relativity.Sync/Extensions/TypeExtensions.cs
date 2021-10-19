using System;

namespace Relativity.Sync.Extensions
{
    internal static class TypeExtensions
    {
        public static Type ExtractTypeIfNullable(this Type type) =>
            !type.IsGenericType || !(type.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? type
                : type.GetGenericArguments()[0];
    }
}