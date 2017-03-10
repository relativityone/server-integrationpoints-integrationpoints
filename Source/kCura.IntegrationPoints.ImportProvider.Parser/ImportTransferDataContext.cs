using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class ImportTransferDataContext : IDataTransferContext, IDisposable
	{
		public ImportTransferDataContext(IDataReaderFactory dataReaderFactory,
			ImportSettings settings,
			string providerSettings,
			List<FieldMap> mappedFields)
		{
			DataReader = dataReaderFactory.GetDataReader(mappedFields.ToArray(), providerSettings);
		}

		public IDataReader DataReader { get; set; }

		public int? TotalItemsFound { get; set; }

		public void UpdateTransferStatus(){}

		public void Dispose()
		{
			DataReader?.Dispose();
		}
	}
}
