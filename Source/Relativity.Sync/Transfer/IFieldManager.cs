using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal interface IFieldManager
	{
		IList<ISpecialFieldBuilder> SpecialFieldBuilders { get; }
		IEnumerable<FieldInfo> GetAllFields(IEnumerable<FieldMap> fieldMappings);
		IEnumerable<FieldInfo> GetDocumentFields(IEnumerable<FieldMap> fieldMappings);
		IEnumerable<FieldInfo> GetSpecialFields(IEnumerable<FieldMap> fieldMappings);
	}
}