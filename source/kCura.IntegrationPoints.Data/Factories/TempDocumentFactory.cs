using Relativity.API;
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

		public ITempDocTableHelper GetDeleteFromTableHelper(IDBContext context, string tableName)
		{
			return new TempDocTableHelper(context, tableName);
		}
	}
}
