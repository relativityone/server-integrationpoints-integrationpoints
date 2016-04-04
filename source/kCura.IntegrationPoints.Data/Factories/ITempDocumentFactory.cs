using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Factories
{
	public interface ITempDocumentFactory
	{
		ITempDocTableHelper GetTableCreationHelper(ICoreContext context, string tableName, string tableSuffix);
		ITempDocTableHelper GetDocTableHelper(IDBContext context, string tableName, string tableSuffix = "");
	}
}
