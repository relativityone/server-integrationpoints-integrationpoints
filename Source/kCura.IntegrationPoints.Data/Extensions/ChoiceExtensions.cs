using System.Linq;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Data.Extensions
{
    public static class ChoiceExtensions
    {
        public static bool EqualsToChoice(this ChoiceRef obj0, ChoiceRef obj1)
        {
            if (obj0 == null && obj1 == null)
            {
                return true;
            }

            if (obj0 == null || obj1 == null)
            {
                return false;
            }

            return obj0.Name == obj1.Name || obj0.Guids.SequenceEqual(obj1.Guids);
        }

        public static bool EqualsToAnyChoice(this ChoiceRef value, params ChoiceRef[] inclusions)
        {
            return inclusions?.Any(value.EqualsToChoice) ?? false;
        }
    }
}
