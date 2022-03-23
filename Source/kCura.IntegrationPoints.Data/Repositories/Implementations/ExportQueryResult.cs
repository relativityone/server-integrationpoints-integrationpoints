using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	/// <inheritdoc />
	public class ExportQueryResult : IExportQueryResult
	{
		private readonly IObjectManagerFacadeFactory _objectManagerFacadeFactory;
		private readonly Action<Exception, int, int> _exceptionHandler;

		internal readonly ExportInitializationResults _exportResult;
		internal readonly int _workspaceArtifactId;
		internal readonly ExecutionIdentity _executionIdentity;

		internal ExportQueryResult(IObjectManagerFacadeFactory objectManagerFacadeFactory, ExportInitializationResults exportResult, int workspaceArtifactId,
			ExecutionIdentity executionIdentity, Action<Exception, int, int> exceptionHandler)
		{
			_objectManagerFacadeFactory = objectManagerFacadeFactory;
			_exportResult = exportResult;
			_workspaceArtifactId = workspaceArtifactId;
			_executionIdentity = executionIdentity;
			_exceptionHandler = exceptionHandler;
		}

		/// <summary>
		/// Deletes the export table
		/// </summary>
		public void Dispose()
		{
			// delete the export table
			using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(_executionIdentity))
			{
				client
					.RetrieveResultsBlockFromExportAsync(_workspaceArtifactId, _exportResult.RunID, 0, (int)_exportResult.RecordCount)
					.GetAwaiter().GetResult();
			}
		}

		/// <inheritdoc />
		public async Task<IEnumerable<RelativityObjectSlim>> GetNextBlockAsync(int startIndex, int resultsBlockSize = 1000)
		{
            
            return await GetNextBlockAsync(startIndex, default(CancellationToken), resultsBlockSize);
            
		}

        public async Task<IEnumerable<RelativityObjectSlim>> GetNextBlockAsync(int startIndex, CancellationToken token,
            int resultsBlockSize = 1000)
        {
            if (startIndex >= _exportResult.RecordCount)
            {
                return Enumerable.Empty<RelativityObjectSlim>();
            }
            using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(_executionIdentity))
            {
                RelativityObjectSlim[] results = (await GetBlockFromExportInternalAsync(resultsBlockSize, startIndex, client, token).ConfigureAwait(false)).ToArray();

                return results;
            }
		}


		/// <inheritdoc />
		public async Task<IEnumerable<RelativityObjectSlim>> GetAllResultsAsync(CancellationToken token = default(CancellationToken))
		{
			using (IObjectManagerFacade client = _objectManagerFacadeFactory.Create(_executionIdentity))
			{
				return (await GetBlockFromExportInternalAsync((int)_exportResult.RecordCount, 0, client, token).ConfigureAwait(false)).ToArray();
			}
		}

		/// <inheritdoc />
		public ExportInitializationResults ExportResult => _exportResult;

		private async Task<IEnumerable<RelativityObjectSlim>> GetBlockFromExportInternalAsync(int resultsBlockSize,
			int startIndex, IObjectManagerFacade client, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (_exportResult.RecordCount == 0)
			{
				return new List<RelativityObjectSlim>();
			}

			try
			{
				var results = new List<RelativityObjectSlim>(resultsBlockSize);
				int remainingObjectsCount = resultsBlockSize;

				RelativityObjectSlim[] partialResults;
				do
				{
					cancellationToken.ThrowIfCancellationRequested();
					
					partialResults = await client
						.RetrieveResultsBlockFromExportAsync(_workspaceArtifactId, _exportResult.RunID,
							remainingObjectsCount, startIndex)
						.ConfigureAwait(false);

					results.AddRange(partialResults);
					remainingObjectsCount -= partialResults.Length;
					startIndex += partialResults.Length;
				}
				while (remainingObjectsCount > 0 && startIndex < _exportResult.RecordCount && partialResults.Any());

				return results;
			}
			catch (Exception ex)
			{
				_exceptionHandler(ex, resultsBlockSize, startIndex);
				throw;
			}
		}
	}
}