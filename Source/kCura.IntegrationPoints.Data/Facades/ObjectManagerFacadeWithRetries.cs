using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Facades.Internals;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades
{
	internal class ObjectManagerFacadeWithRetries : IDisposable
	{
		private bool _disposedValue;

		private const ushort _MAX_NUMBER_OF_RETRIES = 3;
		private const ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC = 3;

		private readonly IObjectManager _objectManager;
		private readonly RetryHandler _retryHandler;

		public ObjectManagerFacadeWithRetries(IObjectManager objectManager, IAPILog logger)
		{
			_objectManager = objectManager;
			_retryHandler = new RetryHandler(
				logger,
				_MAX_NUMBER_OF_RETRIES,
				_EXPONENTIAL_WAIT_TIME_BASE_IN_SEC);
		}

		public Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest)
		{
			return _retryHandler.ExecuteWithRetriesAsync(
				() => _objectManager.CreateAsync(workspaceArtifactID, createRequest));
		}

		public Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request)
		{
			return _retryHandler.ExecuteWithRetriesAsync(
				() => _objectManager.ReadAsync(workspaceArtifactID, request));
		}

		public Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request)
		{
			return _retryHandler.ExecuteWithRetriesAsync(
				() => _objectManager.UpdateAsync(workspaceArtifactID, request));
		}

		public Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request)
		{
			return _retryHandler.ExecuteWithRetriesAsync(
				() => _objectManager.DeleteAsync(workspaceArtifactID, request));
		}

		public Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
		{
			return _retryHandler.ExecuteWithRetriesAsync(
				() => _objectManager.QueryAsync(
					workspaceArtifactID,
					request,
					start,
					length));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposedValue)
			{
				return;
			}
			if (disposing)
			{
				_objectManager?.Dispose();
			}

			_disposedValue = true;
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
