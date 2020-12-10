using System.Collections.Generic;

namespace Relativity.Sync.Storage
{
	/// <summary>
	/// 
	/// </summary>
	public interface IFieldsMappingBuilder
	{
		/// <summary>
		/// 
		/// </summary>
		List<FieldMap> FieldsMapping { get; }
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IFieldsMappingBuilder WithIdentifier();
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFieldId"></param>
		/// <param name="destinationFieldId"></param>
		/// <returns></returns>
		IFieldsMappingBuilder WithField(int sourceFieldId, int destinationFieldId);
	}
}
