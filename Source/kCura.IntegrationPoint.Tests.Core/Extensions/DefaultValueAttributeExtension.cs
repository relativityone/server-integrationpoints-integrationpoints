namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class DefaultValueAttributeExtension
    {
        public static TValue GetValueOrDefault<TObject, TValue>(this TObject @this, Expression<Func<TObject, TValue?>> propertyFunc) where TValue : struct
        {
            TValue? propertyValue = propertyFunc.Compile()(@this);
            if (propertyValue.HasValue)
            {
                return propertyValue.Value;
            }

            return GetValueFromDefaultAttribute<TValue>(propertyFunc);
        }

        public static TValue GetValueOrDefault<TObject, TValue>(this TObject @this, Expression<Func<TObject, TValue>> propertyFunc)
        {
            TValue propertyValue = propertyFunc.Compile()(@this);
            if (!EqualityComparer<TValue>.Default.Equals(propertyValue, default(TValue)))
            {
                return propertyValue;
            }

            return GetValueFromDefaultAttribute<TValue>(propertyFunc);
        }

        private static T GetValueFromDefaultAttribute<T>(LambdaExpression propertyFunc)
        {
            MemberInfo propertyInfo = GetPropertyInfo(propertyFunc);
            DefaultValueAttribute attribute = GetAttribute<DefaultValueAttribute>(propertyInfo);
            if (attribute != null)
            {
                return (T)attribute.Value;
            }

            return default(T);
        }

        private static MemberInfo GetPropertyInfo(LambdaExpression expression)
        {
            var member = (MemberExpression)expression.Body;
            return member.Member;
        }

        private static TAttribute GetAttribute<TAttribute>(MemberInfo memberInfo) where TAttribute : Attribute
        {
            return memberInfo.GetCustomAttribute<TAttribute>();
        }
    }
}