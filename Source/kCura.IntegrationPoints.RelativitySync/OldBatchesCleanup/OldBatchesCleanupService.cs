using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using Relativity.API;
using Relativity.Sync.Storage;


namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
	internal class OldBatchesCleanupService : IOldBatchesCleanupService
	{
		private const int _BATCH_EXPIRATION_IN_DAYS = 7;
		private const string _DELETE_OLD_BATCH_FAILED_MESSAGE = "Failed to delete Sync batches that are older than {batchExpirationInDays} days from workspace artifact ID: {workspaceArtifactID}.";
		private readonly IBatchRepository _batchRepository;
		private readonly Lazy<IErrorService> _errorService;
		private readonly IAPILog _apiLog;

		public OldBatchesCleanupService(IBatchRepository batchRepository, Lazy<IErrorService> errorService, IAPILog apiLog)
		{
			_batchRepository = batchRepository;
			_errorService = errorService;
			_apiLog = apiLog;
		}

		public async Task TryToDeleteOldBatchesInWorkspaceAsync(int workspaceArtifactID)
		{
			try
			{
				await _batchRepository.DeleteAllOlderThanAsync(workspaceArtifactID, TimeSpan.FromDays(_BATCH_EXPIRATION_IN_DAYS)).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_apiLog.LogError(ex, _DELETE_OLD_BATCH_FAILED_MESSAGE, _BATCH_EXPIRATION_IN_DAYS, workspaceArtifactID);

				var errorModel = new ErrorModel(
					ex,
					addToErrorTab: true,
					message: _DELETE_OLD_BATCH_FAILED_MESSAGE)
				{
					WorkspaceId = workspaceArtifactID
				};

				_errorService.Value.Log(errorModel);
			}
		}
	}
}