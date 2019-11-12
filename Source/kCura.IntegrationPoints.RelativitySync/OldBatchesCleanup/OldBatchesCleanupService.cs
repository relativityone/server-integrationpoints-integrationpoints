using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using Relativity.API;
using Relativity.Sync.Storage;


namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
#pragma warning disable CA1031
	internal class OldBatchesCleanupService : IOldBatchesCleanupService
	{
		private const int BATCH_EXPIRATION_IN_DAYS = 7;
		private readonly IBatchRepository _batchRepository;
		private const string _DELETE_OLD_BATCH_FAILED_MESSAGE = "Delete old batch failed";
		private readonly Lazy<IErrorService> _errorService;
		private readonly IAPILog _apiLog;

		public OldBatchesCleanupService(IBatchRepository batchRepository, Lazy<IErrorService> errorService, IAPILog apiLog)
		{
			_batchRepository = batchRepository;
			_errorService = errorService;
			_apiLog = apiLog;
		}

		public async Task DeleteOldBatchesInWorkspaceAsync(int workspaceArtifactId)
		{
			try
			{
				await _batchRepository.DeleteAllOlderThanAsync(workspaceArtifactId,
					TimeSpan.FromDays(BATCH_EXPIRATION_IN_DAYS)).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_apiLog.LogError(e, $"{_DELETE_OLD_BATCH_FAILED_MESSAGE}");

				var errorModel = new ErrorModel(
					e,
					addToErrorTab: true,
					message: _DELETE_OLD_BATCH_FAILED_MESSAGE)
				{
					WorkspaceId = workspaceArtifactId
				};

				_errorService.Value.Log(errorModel);
			}
		}
	}
#pragma warning restore CA1031
}