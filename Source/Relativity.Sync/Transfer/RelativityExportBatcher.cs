using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal sealed class RelativityExportBatcher : IRelativityExportBatcher
	{
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IBatchRepository _batchRepository;
		private readonly IDictionary<string, Job> _tokenMap;

		public RelativityExportBatcher(ISourceServiceFactoryForUser serviceFactory, IBatchRepository batchRepository)
		{
			_serviceFactory = serviceFactory;
			_batchRepository = batchRepository;
			_tokenMap = new Dictionary<string, Job>();
		}

		public string Start(Guid runId, int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			KeyValuePair<string, Job> existing = _tokenMap.FirstOrDefault(kv =>
				kv.Value.WorkspaceArtifactId == workspaceArtifactId && kv.Value.SyncConfigurationArtifactId == syncConfigurationArtifactId);
			if (!existing.Equals(default(KeyValuePair<string, Job>)))
			{
				return existing.Key;
			}

			string newToken = Guid.NewGuid().ToString();
			_tokenMap.Add(newToken, new Job(runId, workspaceArtifactId, syncConfigurationArtifactId));

			return newToken;
		}

		public async Task<RelativityObjectSlim[]> GetNextAsync(string token)
		{
			if (!_tokenMap.ContainsKey(token))
			{
				throw new ArgumentException(
					$"No batch job has been started with token '{token}'. Call {nameof(Start)} to start a batch job.",
					nameof(token));
			}

			Job job = _tokenMap[token];
			Guid runId = job.RunId;
			int workspaceArtifactId = job.WorkspaceArtifactId;
			int syncConfigurationArtifactId = job.SyncConfigurationArtifactId;
			IBatch currentBatch = job.CurrentBatch;

			IBatch nextBatch = currentBatch == null
				? await _batchRepository.GetFirstAsync(workspaceArtifactId, syncConfigurationArtifactId).ConfigureAwait(false)
				: await _batchRepository.GetNextAsync(workspaceArtifactId, syncConfigurationArtifactId, currentBatch.StartingIndex).ConfigureAwait(false);

			if (nextBatch == null)
			{
				_tokenMap.Remove(token);
				return Array.Empty<RelativityObjectSlim>();
			}

			_tokenMap[token] = job.WithBatch(nextBatch);

			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				int resultsBlockSize = nextBatch.TotalItemsCount;
				int startingIndex = nextBatch.StartingIndex;
				RelativityObjectSlim[] block = await objectManager
					.RetrieveResultsBlockFromExportAsync(workspaceArtifactId, runId, resultsBlockSize, startingIndex)
					.ConfigureAwait(false);
				return block;
			}
		}

		private class Job
		{
			public Job(Guid runId, int workspaceArtifactId, int syncConfigurationArtifactId)
				: this(runId, workspaceArtifactId, syncConfigurationArtifactId, null)
			{
			}

			private Job(Guid runId, int workspaceArtifactId, int syncConfigurationArtifactId, IBatch currentBatch)
			{
				RunId = runId;
				WorkspaceArtifactId = workspaceArtifactId;
				SyncConfigurationArtifactId = syncConfigurationArtifactId;
				CurrentBatch = currentBatch;
			}

			public Guid RunId { get; }
			public int WorkspaceArtifactId { get; }
			public int SyncConfigurationArtifactId { get; }
			public IBatch CurrentBatch { get; }

			public Job WithBatch(IBatch nextBatch)
			{
				return new Job(RunId, WorkspaceArtifactId, SyncConfigurationArtifactId, nextBatch);
			}
		}
	}
}
