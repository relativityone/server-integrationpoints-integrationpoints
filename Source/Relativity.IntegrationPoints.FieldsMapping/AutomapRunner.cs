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
			AutomapBuilder mappingBuilder = new AutomapBuilder(sourceFields, destinationFields).MapByIsIdentifier();

			if (!matchOnlyIdentifiers)
			{
				mappingBuilder = mappingBuilder.MapBy(x => x.FieldIdentifier).MapBy(x => x.Name);
			}

			return mappingBuilder.Mapping.OrderByDescending(x => x.SourceField.IsIdentifier).ThenBy(x => x.SourceField.DisplayName);
		}

		private class AutomapBuilder
		{
			public IEnumerable<FieldMap> Mapping { get; }
			private readonly IEnumerable<DocumentFieldInfo> _sourceFields;
			private readonly IEnumerable<DocumentFieldInfo> _destinationFields;

			public AutomapBuilder(IEnumerable<DocumentFieldInfo> sourceFields,
				IEnumerable<DocumentFieldInfo> destinationFields, IEnumerable<FieldMap> mapping = null)
			{
				Mapping = mapping ?? new FieldMap[0];
				_sourceFields = sourceFields;
				_destinationFields = destinationFields;
			}

			public AutomapBuilder MapBy<T>(Func<DocumentFieldInfo, T> selector)
			{
				FieldMap[] newMappings = _sourceFields.Join(_destinationFields, selector, selector, (SourceField, DestinationField) => new { SourceField, DestinationField })
					.Where(x => TypesMatchTest(x.SourceField, x.DestinationField))
					.Select(x => new FieldMap
					{
						SourceField = GetFieldEntry(x.SourceField),
						DestinationField = GetFieldEntry(x.DestinationField),
						FieldMapType = (x.SourceField.IsIdentifier && x.DestinationField.IsIdentifier) ? FieldMapTypeEnum.Identifier : FieldMapTypeEnum.None
					}
					).ToArray();

				DocumentFieldInfo[] remainingSourceFields = _sourceFields.Where(x => !newMappings.Any(m => m.SourceField.FieldIdentifier == x.FieldIdentifier)).ToArray();
				DocumentFieldInfo[] remainingDestinationFields = _destinationFields.Where(x => !newMappings.Any(m => m.DestinationField.FieldIdentifier == x.FieldIdentifier)).ToArray();

				return new AutomapBuilder(
					remainingSourceFields,
					remainingDestinationFields,
					Mapping.Concat(newMappings)
					);
			}

			public AutomapBuilder MapByIsIdentifier()
			{
				DocumentFieldInfo sourceIdentifier = _sourceFields.FirstOrDefault(x => x.IsIdentifier);
				DocumentFieldInfo destinationIdentifier = _destinationFields.FirstOrDefault(x => x.IsIdentifier);

				if (sourceIdentifier == null || destinationIdentifier == null)
				{
					return new AutomapBuilder(_sourceFields.ToArray(), _destinationFields.ToArray(), Mapping.ToArray());
				}

				return new AutomapBuilder(_sourceFields.Where(x => x != sourceIdentifier).ToArray(),
					_destinationFields.Where(x => x != destinationIdentifier).ToArray(),
					Mapping.Concat(new FieldMap[]
					{
						new FieldMap
						{
							SourceField = GetFieldEntry(sourceIdentifier),
							DestinationField = GetFieldEntry(destinationIdentifier),
							FieldMapType = FieldMapTypeEnum.Identifier
						}
					})
				);
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
}