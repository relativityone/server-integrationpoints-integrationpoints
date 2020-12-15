using System.Collections.Generic;
using Relativity.Sync.Storage;

#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration
{

	public interface IFieldsMappingBuilder
	{
		List<FieldMap> FieldsMapping { get; }
		
		IFieldsMappingBuilder WithIdentifier();
		
		IFieldsMappingBuilder WithField(int sourceFieldId, int destinationFieldId);

		IFieldsMappingBuilder WithField(string sourceFieldName, string destinationFieldName);
	}
}
