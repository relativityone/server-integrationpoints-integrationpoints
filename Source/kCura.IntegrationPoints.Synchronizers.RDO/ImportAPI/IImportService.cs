using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public interface IImportService
	{
		void AddRow(Dictionary<string, object> fields);
		bool PushBatchIfFull(bool forcePush);
		void Initialize();
		void KickOffImport(IDataReader dataSource);
		ImportSettings Settings { get; }
	}
}
