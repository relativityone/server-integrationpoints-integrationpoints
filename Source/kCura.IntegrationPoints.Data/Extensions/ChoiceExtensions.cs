using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class ChoiceExtensions
	{
		public static bool EqualsToChoice(this Choice obj0, Choice obj1)
		{
			return (obj0 != null && obj1 != null && obj0.Name == obj1.Name);
		}
		public static string GetNameOrDefault(this Choice choice, string defaultValue = null)
		{
			if (defaultValue == null)
			{
				defaultValue = string.Empty;
			}
			return choice != null ? choice.Name : defaultValue;
		}
	}
}
