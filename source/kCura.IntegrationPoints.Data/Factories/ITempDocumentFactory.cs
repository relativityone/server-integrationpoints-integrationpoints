using kCura.IntegrationPoints.Core.Services.ServiceContext;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Factories
{
	public interface ITempDocumentFactory
	{
		ITempDocTableHelper GetTableCreationHelper(ICoreContext context, string tableName, string tableSuffix);
		ITempDocTableHelper GetDeleteFromTableHelper(ICaseServiceContext context, string tableName);
	}
}
