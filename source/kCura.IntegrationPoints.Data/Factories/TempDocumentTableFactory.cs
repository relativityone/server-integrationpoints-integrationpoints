using Relativity.API;

namespace kCura.IntegrationPoints.Data.Factories
{
	public class TempDocumentTableFactory : ITempDocumentTableFactory
	{
		private readonly IHelper _helper;

		public TempDocumentTableFactory(IHelper helper)
		{
			_helper = helper;
		}

		public ITempDocTableHelper GetDocTableHelper(string tableSuffix, int sourceWorkspaceId)
		{
			return new TempDocTableHelper(_helper, tableSuffix, sourceWorkspaceId);
		}
	}
}