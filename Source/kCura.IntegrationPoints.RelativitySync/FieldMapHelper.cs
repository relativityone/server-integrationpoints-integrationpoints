using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal static class FieldMapHelper
	{
		public static string FixMappings(string fieldMappings, ISerializer serializer)
		{
			List<FieldMap> fields = serializer.Deserialize<List<FieldMap>>(fieldMappings);
			if (fields == null)
			{
				return fieldMappings;
			}
			fields = RemoveSpecialFieldMappings(fields);
			fields = FixControlNumberFieldName(fields);

			return serializer.Serialize(fields);
		}

		private static List<FieldMap> RemoveSpecialFieldMappings(List<FieldMap> fields)
		{
			var redundantMapTypes = new[] { FieldMapTypeEnum.FolderPathInformation, FieldMapTypeEnum.NativeFilePath};
			return fields.Where(f => !redundantMapTypes.Contains(f.FieldMapType)).ToList();
		}

		private static List<FieldMap> FixControlNumberFieldName(List<FieldMap> fields)
		{
			FieldMap identifier = fields.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.Identifier);
			if (identifier != null)
			{
				FixControlNumberField(identifier.SourceField);
				FixControlNumberField(identifier.DestinationField);
			}

			return fields;
		}

		private static void FixControlNumberField(FieldEntry fieldEntry)
		{
			fieldEntry.DisplayName = fieldEntry.DisplayName.Replace("[Object Identifier]", string.Empty).Trim();
		}
	}
}