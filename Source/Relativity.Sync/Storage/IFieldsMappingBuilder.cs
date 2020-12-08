using System.Collections.Generic;

namespace Relativity.Sync.Storage
{
	public interface IFieldsMappingBuilder
	{
		List<FieldMap> FieldsMapping { get; }
		IFieldsMappingBuilder WithIdentifier();
		IFieldsMappingBuilder WithField(int sourceFieldId, int destinationFieldId);
	}
}
