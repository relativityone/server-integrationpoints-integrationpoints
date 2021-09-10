using System;
using System.ComponentModel;
using System.Reflection;

namespace Relativity.Sync
{
	internal static class EnumExtensions
	{
		internal static T GetEnumFromDescription<T>(this string description)
		{
			Type type = typeof(T);
			if (!type.IsEnum)
			{
				throw new InvalidOperationException($"The type specified is not an enum type: {type}.");
			}

			foreach (var field in type.GetFields())
			{
				Attribute customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
				if (customAttribute is DescriptionAttribute attribute)
				{
					if (attribute.Description == description)
					{
						return (T)field.GetValue(null);
					}
				}
			
				if (field.Name == description)
				{
					return (T)field.GetValue(null);
				}
			}

			throw new InvalidOperationException($"The description could not be converted to the proper enum value: {description}.");
		}

		internal static string GetDescription<T>(this T value)
		{
			Type type = typeof(T);
			if (!type.IsEnum)
			{
				throw new InvalidOperationException($"The type specified is not an enum type: {type}.");
			}

			string enumName = type.GetEnumName(value);

			System.Reflection.FieldInfo field = type.GetField(enumName);
			Attribute customAttribute = field.GetCustomAttribute(typeof(DescriptionAttribute));

			string description;
			if (customAttribute is DescriptionAttribute attribute)
			{
				description = attribute.Description;
			}
			else
			{
				description = enumName;
			}

			return description;
		}
	}
}