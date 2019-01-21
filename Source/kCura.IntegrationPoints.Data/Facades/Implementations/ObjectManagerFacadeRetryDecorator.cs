using kCura.IntegrationPoints.Data.Interfaces;
using Relativity.Services.Objects.DataContracts;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Facades.Implementations
{
	internal class ObjectManagerFacadeRetryDecorator : IObjectManagerFacade
	{
		private bool _disposedValue = false;

		private const ushort _MAX_NUMBER_OF_RETRIES = 3;
		private const ushort _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC = 3;

		private readonly IRetryHandler _retryHandler;
		private readonly IObjectManagerFacade _objectManager;

		/// <summary>
		/// Creates new instance of object manager facade with implemented retries
		/// </summary>
		/// <param name="objectManager">This object will be disposed when Dispose is called</param>
		/// <param name="retryHandlerFactory"></param>
		public ObjectManagerFacadeRetryDecorator(IObjectManagerFacade objectManager, IRetryHandlerFactory retryHandlerFactory)
		{
			_objectManager = objectManager;
			_retryHandler = retryHandlerFactory.Create(_MAX_NUMBER_OF_RETRIES, _EXPONENTIAL_WAIT_TIME_BASE_IN_SEC);
		}

		public async Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest)
		{
			return await _retryHandler.ExecuteWithRetriesAsync(
					() => _objectManager.CreateAsync(workspaceArtifactID, createRequest))
				.ConfigureAwait(false);
		}

		public async Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request)
		{
			return await _retryHandler.ExecuteWithRetriesAsync(
					() => _objectManager.DeleteAsync(workspaceArtifactID, request))
				.ConfigureAwait(false);
		}

		public async Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
		{
			return await _retryHandler.ExecuteWithRetriesAsync(
					() => _objectManager.QueryAsync(workspaceArtifactID, request, start, length))
				.ConfigureAwait(false);
		}

		public async Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request)
		{
			return await _retryHandler.ExecuteWithRetriesAsync(
					() => _objectManager.ReadAsync(workspaceArtifactID, request))
				.ConfigureAwait(false);
		}

		public async Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request)
		{
			return await _retryHandler.ExecuteWithRetriesAsync(
					() => _objectManager.UpdateAsync(workspaceArtifactID, request))
				.ConfigureAwait(false);
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
