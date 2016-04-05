using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Factories
{
	public class TempDocumentFactory : ITempDocumentFactory
	{
		public ITempDocTableHelper GetTableCreationHelper(ICoreContext context, string tableName, string tableSuffix)
		{
			return new TempDocTableHelper(context, tableName, tableSuffix);
		}

		public ITempDocTableHelper GetDocTableHelper(IDBContext context, string tableName, string tableSuffix = "")
		{
			return new TempDocTableHelper(context, tableName, tableSuffix);
		}
	}
}
