using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace Relativity.IntegrationPoints.FieldsMapping
{ 
	public class AutomapRunner : IAutomapRunner
	{
		public IEnumerable<FieldMap> MapFields(IEnumerable<DocumentFieldInfo> sourceFields,
			IEnumerable<DocumentFieldInfo> destinationFields, bool matchOnlyIdentifiers = false)
		{
			if (matchOnlyIdentifiers)
			{
				sourceFields = sourceFields.Where(x => x.IsIdentifier);
				destinationFields = destinationFields.Where(x => x.IsIdentifier);
			}

			List<FieldMap> mapping = sourceFields.Join(destinationFields, x => x.Name, x => x.Name, (SourceField, DestinationField) => new { SourceField, DestinationField })
			.Where(x => TypesMatchTest(x.SourceField, x.DestinationField))
			.Select(x => new FieldMap
			{
				SourceField = GetFieldEntry(x.SourceField),
				DestinationField = GetFieldEntry(x.DestinationField),
				FieldMapType = (x.SourceField.IsIdentifier && x.DestinationField.IsIdentifier) ? FieldMapTypeEnum.Identifier : FieldMapTypeEnum.None
			}
			).ToList();

			EnsureIdentifiersMapped(mapping, sourceFields, destinationFields);

			return mapping
				.OrderByDescending(x => x.FieldMapType)
				.ThenBy(x => x.SourceField.DisplayName)
				.ToArray();
		}

		private void EnsureIdentifiersMapped(List<FieldMap> mappings, IEnumerable<DocumentFieldInfo> sourceFields, IEnumerable<DocumentFieldInfo> destinationFields)
		{
			FieldMap identifierMap =
				mappings.FirstOrDefault(m => m.FieldMapType == FieldMapTypeEnum.Identifier);

			if (identifierMap == null)
			{
				DocumentFieldInfo sourceIdentifier = sourceFields.FirstOrDefault(x => x.IsIdentifier);
				DocumentFieldInfo destinationIdentifier = destinationFields.FirstOrDefault(x => x.IsIdentifier);
				if (sourceIdentifier != null && destinationIdentifier != null)
				{
					mappings.RemoveAll(x =>
					x.SourceField?.FieldIdentifier == sourceIdentifier?.FieldIdentifier ||
					x.DestinationField?.FieldIdentifier == destinationIdentifier?.FieldIdentifier);

					mappings.Add(new FieldMap
					{
						SourceField = GetFieldEntry(sourceIdentifier),
						DestinationField = GetFieldEntry(destinationIdentifier),
						FieldMapType = FieldMapTypeEnum.Identifier
					});
				}
			}
		}

		private bool TypesMatchTest(DocumentFieldInfo sourceField, DocumentFieldInfo destinationField)
		{
			const string fixedLengthText = "Fixed-Length Text";
			const string longText = "Long Text";
			return (sourceField.Type == destinationField.Type)
				   || (sourceField.Type.StartsWith(fixedLengthText) &&
					   (destinationField.Type.StartsWith(fixedLengthText) || destinationField.Type == longText)
					   );
		}

		private FieldEntry GetFieldEntry(DocumentFieldInfo fieldInfo)
		{
			return new FieldEntry
			{
				DisplayName = fieldInfo.Name,
				FieldIdentifier = fieldInfo.FieldIdentifier,
				FieldType = FieldType.String,
				IsIdentifier = fieldInfo.IsIdentifier,
				IsRequired = fieldInfo.IsRequired,
				Type = fieldInfo.Type
			};
		}
	}
}