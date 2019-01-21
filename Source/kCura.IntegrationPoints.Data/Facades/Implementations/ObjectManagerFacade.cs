using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Facades.Implementations
{
	internal class ObjectManagerFacade : IObjectManagerFacade
	{
		private bool _isDisposed = false;

		private readonly Lazy<IObjectManager> _objectManager;

		public ObjectManagerFacade(Func<IObjectManager> objectManager)
		{
			_objectManager = new Lazy<IObjectManager>(objectManager);
		}

		public Task<CreateResult> CreateAsync(int workspaceArtifactID, CreateRequest createRequest)
		{
			return _objectManager.Value.CreateAsync(
					workspaceArtifactID, 
					createRequest);
		}

		public Task<ReadResult> ReadAsync(int workspaceArtifactID, ReadRequest request)
		{
			return _objectManager.Value.ReadAsync(
					workspaceArtifactID, 
					request);
		}

		public Task<UpdateResult> UpdateAsync(int workspaceArtifactID, UpdateRequest request)
		{
			return _objectManager.Value.UpdateAsync(
					workspaceArtifactID,
					request);
		}

		public Task<DeleteResult> DeleteAsync(int workspaceArtifactID, DeleteRequest request)
		{
			return _objectManager.Value.DeleteAsync(
					workspaceArtifactID,
					request);
		}

		public Task<QueryResult> QueryAsync(int workspaceArtifactID, QueryRequest request, int start, int length)
		{
			return _objectManager.Value.QueryAsync(
					workspaceArtifactID,
					request,
					start,
					length);
		}

		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}
			if (disposing && _objectManager.IsValueCreated)
			{
				_objectManager.Value.Dispose();
			}

			_isDisposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
