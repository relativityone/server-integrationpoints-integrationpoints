using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Factories
{
	public interface ITempDocumentTableFactory
	{
		/// <summary>
		/// Returns an instance of ITempDocTableHelper
		/// </summary>
		/// <param name="tableSuffix">The unique suffix to append to the table (optional because sometimes it is set outside the constructor)</param>
		/// <param name="sourceWorkspaceId">The Artifact ID of the source workspace</param>
		/// <returns></returns>
		ITempDocTableHelper GetDocTableHelper(string tableSuffix, int sourceWorkspaceId);
	}
}
