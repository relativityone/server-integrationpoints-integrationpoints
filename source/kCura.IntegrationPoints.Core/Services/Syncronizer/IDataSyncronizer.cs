using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	public interface IDataSyncronizer: IFieldProvider
	{
		/// <summary>
		/// Syncs the data to a destination source
		/// </summary>
		/// <param name="data">The data to be synced</param>
		/// <param name="fieldMap">The list of fields that are expected to be mapped</param>
		void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap);
	}
}
