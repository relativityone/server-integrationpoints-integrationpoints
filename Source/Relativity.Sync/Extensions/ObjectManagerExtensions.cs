using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Extensions
{
	internal static class ObjectManagerExtensions
	{
		public static async Task<List<RelativityObjectSlim>> QueryAllAsync(this IObjectManager objectManager, int workspaceId, QueryRequest queryRequest)
		{
			int retrievedRecordCount = 0;
			List<RelativityObjectSlim> result = new List<RelativityObjectSlim>();

			ExportInitializationResults exportInitializationResults = await objectManager.InitializeExportAsync(workspaceId, queryRequest, 1).ConfigureAwait(false);
			int exportedRecordsCount = (int)exportInitializationResults.RecordCount;

			RelativityObjectSlim[] exportResultsBlock = await objectManager
				.RetrieveResultsBlockFromExportAsync(workspaceId, exportInitializationResults.RunID, exportedRecordsCount - retrievedRecordCount, retrievedRecordCount)
				.ConfigureAwait(false);

			while (exportResultsBlock != null && exportResultsBlock.Any())
			{
				result.AddRange(exportResultsBlock);

				retrievedRecordCount += exportResultsBlock.Length;

				exportResultsBlock = await objectManager
					.RetrieveResultsBlockFromExportAsync(workspaceId, exportInitializationResults.RunID, exportedRecordsCount - retrievedRecordCount, retrievedRecordCount)
					.ConfigureAwait(false);
			}

			return result;
		}
	}
}
