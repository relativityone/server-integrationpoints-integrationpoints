using System;
using System.ComponentModel;
using System.Reflection;

namespace kCura.IntegrationPoints.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                return null;
            }

            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (attribute == null)
            {
                return null;
            }

            return attribute.Description;
        }

        public static T GetValue<T>(this string description)
        {
            Type type = typeof(T);

            if (type.IsEnum)
            {
                foreach (var field in type.GetFields())
                {
                    DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

                    if (attribute == null)
                    {
                        if (field.Name == description)
                        {
                            return (T)field.GetValue(null);
                        }
                    }
                    else
                    {
                        if (attribute.Description == description)
                        {
                            return (T)field.GetValue(null);
                        }
                    }
                }
            }

            return default(T);
        }
    }
}
