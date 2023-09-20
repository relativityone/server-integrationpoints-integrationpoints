using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Utils
{
    internal static class FieldMapExtensions
    {
        public static IndexedFieldMap GetIdentifier(this List<IndexedFieldMap> fieldsMapping)
        {
            return fieldsMapping.FirstOrDefault(x => x.FieldMap.DestinationField.IsIdentifier);
        }
    }
}
