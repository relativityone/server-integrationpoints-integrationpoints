using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

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

			fields = FixFolderPathMapping(fields);
			fields = RemoveSpecialFieldMappings(fields);
			fields = FixControlNumberFieldName(fields);
			fields = Deduplicate(fields);

			return serializer.Serialize(fields);
		}

		private static List<FieldMap> FixFolderPathMapping(List<FieldMap> fields)
		{
			FieldMap mappedFolderPathSpecialField = fields.SingleOrDefault(x => x.FieldMapType == FieldMapTypeEnum.FolderPathInformation);
			if (mappedFolderPathSpecialField?.DestinationField?.ActualName != null)
			{
				fields.Add(new FieldMap()
				{
					SourceField = mappedFolderPathSpecialField.SourceField,
					DestinationField = mappedFolderPathSpecialField.DestinationField,
					FieldMapType = FieldMapTypeEnum.None
				});
			}

			return fields;
		}

		private static List<FieldMap> RemoveSpecialFieldMappings(List<FieldMap> fields)
		{
			var redundantMapTypes = new[] { FieldMapTypeEnum.NativeFilePath, FieldMapTypeEnum.FolderPathInformation };
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

		private static List<FieldMap> Deduplicate(List<FieldMap> fields)
		{
			List<FieldMap> deduplicatedList = new List<FieldMap>();

			foreach (FieldMap fieldMap in fields)
			{
				if (!deduplicatedList.Any(x => x.SourceField.FieldIdentifier == fieldMap.SourceField.FieldIdentifier ||
				                              x.DestinationField.FieldIdentifier == fieldMap.DestinationField.FieldIdentifier))
				{
					deduplicatedList.Add(fieldMap);
				}
			}

			return deduplicatedList;
		}

		private static void FixControlNumberField(FieldEntry fieldEntry)
		{
			fieldEntry.DisplayName = fieldEntry.DisplayName.Replace("[Object Identifier]", string.Empty).Trim();
		}
	}
}