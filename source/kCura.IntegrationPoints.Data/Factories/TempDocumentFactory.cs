using Relativity.API;
using Relativity.Core;


namespace kCura.IntegrationPoints.Data.Factories
{
	public class TempDocumentFactory : ITempDocumentFactory
	{
		public ITempDocTableHelper GetDocTableHelper(IHelper helper, string tableSuffix,
			int sourceWorkspaceId)
		{
			return new TempDocTableHelper(helper, tableSuffix, sourceWorkspaceId);
		}
	}
}
