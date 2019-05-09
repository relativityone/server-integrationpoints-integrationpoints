using System;
using System.ComponentModel;

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
				else
				{
					if (field.Name == description)
					{
						return (T)field.GetValue(null);
					}
				}
			}

			throw new InvalidOperationException($"The description could not be converted to the proper enum value: {description}.");
		}
	}
}