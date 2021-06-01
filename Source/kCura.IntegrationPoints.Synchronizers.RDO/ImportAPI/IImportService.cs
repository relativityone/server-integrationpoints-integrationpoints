using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Domain.Managers;
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
		
		/// <summary>
		/// Returns total amount of processed items (transferred + item level error)
		/// </summary>
		int ProcessedItems { get; }
	}
}
