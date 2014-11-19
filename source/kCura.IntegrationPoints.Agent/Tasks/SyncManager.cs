using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.ScheduleQueueAgent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncManager : ITask
	{
		private readonly IDataProviderFactory _providerFactory;
		private readonly IJobManager _jobManager;
		private readonly IntegrationPointHelper _helper;
		public SyncManager(IDataProviderFactory providerFactory, IJobManager jobManager, IntegrationPointHelper helper)
		{
			_providerFactory = providerFactory;
			_jobManager = jobManager;
			_helper = helper;
		}

		public void Execute(Job job)
		{
			if (!job.RelatedObjectArtifactID.HasValue)
			{
				throw new ArgumentNullException("Job must have a Related Object ArtifactID");
			}
			var ipID = job.RelatedObjectArtifactID.Value;
			IDataReader idReader = GetProviderDataReader(ipID);
			var batchSize = Config.AgentConfig.BatchSize;
			CreateJobs(idReader, batchSize);
		}

		private IDataReader GetProviderDataReader(int ipID)
		{
			IDataSourceProvider provider = _providerFactory.GetDataProvider();
			FieldEntry idField = _helper.GetIdentifierFieldEntry(ipID);
			string options = _helper.GetSourceOptions(ipID);
			IDataReader idReader = provider.GetBatchableData(idField, options);
			return idReader;
		}

		public virtual void CreateJobs(IDataReader reader, int batchSize)
		{
			var list = new List<string>();
			var idx = 0;

			while (reader.Read())
			{
				list.Add(reader.GetString(0));
				idx++;
				if (idx == batchSize)
				{
					_jobManager.CreateJob(list, TaskType.SyncWorker);
					list = new List<string>();
					idx = 0;
				}
			}

			if (list.Any())
			{
				_jobManager.CreateJob(list, TaskType.SyncWorker);
			}

		}

	}
}
