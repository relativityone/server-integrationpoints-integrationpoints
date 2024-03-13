using System.Collections.Generic;
using System.Data;
using Relativity.IntegrationPoints.Contracts.Models;

namespace Relativity.IntegrationPoints.Contracts.Provider
{
	/// <summary>
	/// Represents a source provider used for importing data into the system.
	/// </summary>
	public interface IDataSourceProvider : IFieldProvider
	{
		/// <summary>
		/// Retrieves data from a data source and imports it into the system.
		/// </summary>
		/// <param name="fields">The fields requested from the data source.</param>
		/// <param name="entryIds">The IDs for the requested fields.</param>
		/// <param name="providerConfiguration">A group of configuration settings that the control source provider behavior.</param>
		/// <returns>A stream containing data from based on requested fields and their IDs.</returns>
		IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration);

		/// <summary>
		/// Retrieves the IDs from the source data used as identifiers for workspace fields.
		/// </summary>
		/// <param name="identifier">The identifier field used for batching jobs.</param>
		/// <param name="providerConfiguration">A group of configuration settings that the control source provider behavior.</param>
		/// <returns>A stream containing only IDs retrieved from the identifier fields.</returns>
		IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration);
	}
}
