using System.Collections.Generic;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Storage
{
	internal interface IFieldMappings
	{
		List<FieldMap> GetFieldMappings();
	}
}