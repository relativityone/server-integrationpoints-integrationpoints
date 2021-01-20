using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class ChoiceExtensions
	{
		public static bool EqualsToChoice(this ChoiceRef obj0, ChoiceRef obj1)
		{
			return (obj0 != null && obj1 != null && obj0.Name == obj1.Name);
		}
	}
}
