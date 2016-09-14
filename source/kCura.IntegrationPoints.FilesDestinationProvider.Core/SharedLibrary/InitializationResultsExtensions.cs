using kCura.EDDS.WebAPI.ExportManagerBase;
using Relativity.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public static class InitializationResultsExtensions
	{
		public static InitializationResults ToInitializationResults(this Export.InitializationResults result)
		{
			return new InitializationResults
			{
				ColumnNames = result.ColumnNames,
				RowCount = result.RowCount,
				RunId = result.RunId
			};
		}
	}
}