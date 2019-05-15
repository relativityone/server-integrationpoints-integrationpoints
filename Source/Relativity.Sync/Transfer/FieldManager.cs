using System.Collections.Generic;
using System.Linq;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Represents the mapping between different fields and properties in the source and destination
	/// workspaces. This class should be the source of truth for what fields are mapped and how between
	/// the various Relativity APIs.
	/// </summary>
	internal sealed class FieldManager : IFieldManager
	{
		public IList<ISpecialFieldBuilder> SpecialFieldBuilders { get; }

		public FieldManager(IEnumerable<ISpecialFieldBuilder> specialFieldBuilders)
		{
			SpecialFieldBuilders = specialFieldBuilders.OrderBy(b => b.GetType().FullName).ToList();
		}

		public IEnumerable<FieldInfo> GetAllFields(IEnumerable<FieldMap> fieldMappings)
		{
			IEnumerable<FieldInfo> specialFields = SpecialFieldBuilders.SelectMany(b => b.BuildColumns());
			IEnumerable<FieldInfo> allFields = GetMappedDocumentFields(fieldMappings).Concat(specialFields);
			return EnrichDocumentFieldsWithIndex(allFields);
		}

		private IEnumerable<FieldInfo> EnrichDocumentFieldsWithIndex(IEnumerable<FieldInfo> allFields)
		{
			int currentIndex = 0;
			foreach (var field in allFields)
			{
				if (field.IsDocumentField)
				{
					field.DocumentFieldIndex = currentIndex;
					currentIndex++;
				}
				yield return field;
			}
		}

		public IEnumerable<FieldInfo> GetDocumentFields(IEnumerable<FieldMap> fieldMappings)
		{
			return GetAllFields(fieldMappings).Where(f => f.IsDocumentField);
		}

		private IEnumerable<FieldInfo> GetMappedDocumentFields(IEnumerable<FieldMap> fieldMappings)
		{
			foreach (FieldMap fieldMap in fieldMappings)
			{
				yield return new FieldInfo {DisplayName = fieldMap.SourceField.DisplayName, FieldIdentifier = fieldMap.SourceField.FieldIdentifier, IsDocumentField = true};
			}
		}
	}
}
