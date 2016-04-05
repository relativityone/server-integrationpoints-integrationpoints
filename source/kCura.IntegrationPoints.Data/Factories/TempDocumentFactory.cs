using Relativity.API;
using Relativity.Core;


namespace kCura.IntegrationPoints.Data.Factories
{
	public class TempDocumentFactory : ITempDocumentFactory
	{
		public ITempDocTableHelper GetDocTableHelper(IHelper helper, string tableName, string tableSuffix,
			int sourceWorkspaceId)
		{
			return new TempDocTableHelper(helper, tableName, tableSuffix, sourceWorkspaceId);
		}
	}
}
