using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Core.Services.Syncronizer
{
	public interface IDataSyncronizer: IFieldProvider
	{
		void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options);

	}
}
