using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Readers;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public interface IImportService
	{
		void AddRow(Dictionary<string, object> fields);
		bool PushBatchIfFull(bool forcePush);
		void Initialize();
		void KickOffImport(IDataTransferContext dataSource);
		ImportSettings Settings { get; }
		int TotalRowsProcessed { get; }
	}
}
