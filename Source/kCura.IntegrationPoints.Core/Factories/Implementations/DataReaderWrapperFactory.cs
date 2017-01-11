using System;
using System.Data;
using System.Collections.Generic;

using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class DataReaderWrapperFactory : IDataReaderWrapperFactory
	{
		public IDataReader GetWrappedDataReader(
			IDataSourceProvider sourceProvider,
			FieldMap[] fieldMaps,
			List<FieldEntry> sourceFields,
			ImportSettings destinationSettings,
			List<string> entryIDs,
			string sourceConfiguration
			)
		{
			if (destinationSettings.ImageImport)
			{
				return sourceProvider.GetData(sourceFields, entryIDs, sourceConfiguration);
			}
			else
			{
				ImportDataReader importDataReader = new ImportDataReader(
						sourceProvider,
						sourceFields,
						entryIDs,
						sourceConfiguration);
				importDataReader.Setup(fieldMaps);
				return importDataReader;
			}
		}
	}
}
