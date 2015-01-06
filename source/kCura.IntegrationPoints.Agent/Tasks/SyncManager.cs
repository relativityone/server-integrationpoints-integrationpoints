using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.ScheduleQueue.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.ScheduleQueue.Core.BatchProcess;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncManager : BatchManagerBase<string>
	{
		private readonly IDataProviderFactory _providerFactory;
		private readonly IJobManager _jobManager;
		private readonly IntegrationPointService _helper;

		public SyncManager(IDataProviderFactory providerFactory, IJobManager jobManager, IntegrationPointService helper)
		{
			_providerFactory = providerFactory;
			_jobManager = jobManager;
			_helper = helper;
		}

		public override int BatchSize
		{
			get { return Config.AgentConfig.BatchSize; }
		}

		public override IEnumerable<string> GetUnbatchedIDs(Job job)
		{
			if (job.RelatedObjectArtifactID < 1)
			{
				throw new ArgumentNullException("Job must have a Related Object ArtifactID");
			}
			var integrationPointID = job.RelatedObjectArtifactID;
			IDataSourceProvider provider = _providerFactory.GetDataProvider(Guid.NewGuid());
			FieldEntry idField = _helper.GetIdentifierFieldEntry(integrationPointID);
			string options = _helper.GetSourceOptions(integrationPointID);
			IDataReader idReader = provider.GetBatchableIds(idField, options);

			return new ReaderEnumerable(idReader);
		}

		public override void CreateBatchJob(Job job, List<string> batchIDs)
		{
			_jobManager.CreateJob(batchIDs, TaskType.SyncWorker, job.RelatedObjectArtifactID);
		}

		private class ReaderEnumerable : IEnumerable<string>
		{
			private IDataReader _reader;
			public ReaderEnumerable(IDataReader reader)
			{
				_reader = reader;
			}
			public IEnumerator<string> GetEnumerator()
			{
				while (_reader.Read())
				{
					yield return _reader.GetString(0);
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

	}
}
