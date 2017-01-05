using System;
using System.Data;
using System.Collections.Generic;

using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IDataReaderWrapperFactory
	{
		IDataReader GetWrappedDataReader(
			IDataSourceProvider sourceProvider,
			FieldMap[] fieldMaps,
			List<FieldEntry> sourceFields,
			ImportSettings destinationSettings,
			List<string> entryIDs,
			string sourceConfiguration
			);
	}
}
