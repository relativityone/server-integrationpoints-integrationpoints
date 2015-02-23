using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts.Synchronizer
{
	/// <summary>
	/// Represents and inflow of data into the system.
	/// </summary>
	public interface IDataSynchronizer : IFieldProvider
	{
		/// <summary>
		/// Used to bring data into the system.
		/// </summary>
		/// <param name="data">The records that are expected to be inserted.</param>
		/// <param name="fieldMap">The field mapping that will insert the data into the system.</param>
		/// <param name="options">The options that are provided to sync data to the destination.</param>
		void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options);
	}
}
