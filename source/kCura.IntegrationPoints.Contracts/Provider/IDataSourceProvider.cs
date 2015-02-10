using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Contracts.Provider
{
	/// <summary>
	/// Represents an integration point where the data will flow from into the system.
	/// </summary>
	public interface IDataSourceProvider : IFieldProvider
	{
		/// <summary>
		/// Determins the Data from the source that will flow into the system.
		/// </summary>
		/// <param name="fields">The fields that are requested from the data source.</param>
		/// <param name="entryIds">The requested IDs that are expected to be read.</param>
		/// <param name="options">The source providers options that have been previously set.</param>
		/// <returns>A stream that represents the data based on the requested fields and the requested entryIds.</returns>
		IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options);
		
		/// <summary>
		/// Determins the identifer for the current ids.
		/// </summary>
		/// <param name="identifier">The identifier field used to batch up jobs.</param>
		/// <param name="options">The source providers options that have been previously set.</param>
		/// <returns>A stream that represents all the ids with just the identifier field.</returns>
		IDataReader GetBatchableIds(FieldEntry identifier, string options);
	}
}
