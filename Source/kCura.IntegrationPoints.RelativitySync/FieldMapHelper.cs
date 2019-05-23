using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal static class FieldMapHelper
	{
		public static string FixControlNumberFieldName(string fieldMappings, ISerializer serializer)
		{
			List<FieldMap> fields = serializer.Deserialize<List<FieldMap>>(fieldMappings);
			if (fields == null)
			{
				return fieldMappings;
			}

			FieldMap identifier = fields.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.Identifier);
			if (identifier != null)
			{
				FixField(identifier.SourceField);
				FixField(identifier.DestinationField);
			}

			return serializer.Serialize(fields);
		}

		private static void FixField(FieldEntry fieldEntry)
		{
			fieldEntry.DisplayName = fieldEntry.DisplayName.Replace("[Object Identifier]", string.Empty).Trim();
		}
	}
}