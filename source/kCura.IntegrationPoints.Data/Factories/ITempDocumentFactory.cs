using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Factories
{
	public interface ITempDocumentFactory
	{
		/// <summary>
		/// Returns an instance of ITempDocTableHelper
		/// </summary>
		/// <param name="context">The database context to work with</param>
		/// <param name="tableName">The name of the scratch table</param>
		/// <param name="tableSuffix">The unique suffix to append to the table (optional because sometimes it is set outside the constructor)</param>
		/// <returns></returns>
		ITempDocTableHelper GetDocTableHelper(IHelper helper, string tableName, string tableSuffix,
			int sourceWorkspaceId);
	}
}
