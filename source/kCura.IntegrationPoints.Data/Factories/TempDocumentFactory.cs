using kCura.IntegrationPoints.Core.Services.ServiceContext;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Factories
{
	public class TempDocumentFactory : ITempDocumentFactory
	{
		public ITempDocTableHelper GetTableCreationHelper(ICoreContext context, string tableName, string tableSuffix)
		{
			string table = tableName + "_" + tableSuffix;
			return new TempDocTableHelper(context, table);
		}

		public ITempDocTableHelper GetDeleteFromTableHelper(ICaseServiceContext context, string tableName)
		{
			return new TempDocTableHelper(context, tableName);
		}
	}
}
