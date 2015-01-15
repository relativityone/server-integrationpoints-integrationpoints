﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.ScheduleQueue.Core.BatchProcess;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncManager : BatchManagerBase<string>, IDisposable
	{
		private ICaseServiceContext _caseServiceContext;
		private IDataProviderFactory _providerFactory;
		private IJobManager _jobManager;
		private IJobService _jobService;
		private IntegrationPointService _helper;
		private IScheduleRuleFactory _scheduleRuleFactory;

		public SyncManager(ICaseServiceContext caseServiceContext,
			IDataProviderFactory providerFactory,
			IJobManager jobManager,
			IJobService jobService,
			IntegrationPointService helper,
			IScheduleRuleFactory scheduleRuleFactory)
		{
			_caseServiceContext = caseServiceContext;
			_providerFactory = providerFactory;
			_jobManager = jobManager;
			_jobService = jobService;
			_helper = helper;
			_scheduleRuleFactory = scheduleRuleFactory;
			base.RaiseJobPreExecute += new JobPreExecuteEvent(JobPreExecute);
			base.RaiseJobPostExecute += new JobPostExecuteEvent(JobPostExecute);
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
			Guid identifier = Guid.Parse(_caseServiceContext.RsapiService.SourceProviderLibrary.Read(job.RelatedObjectArtifactID).Identifier);
			IDataSourceProvider provider = _providerFactory.GetDataProvider(identifier);
			FieldEntry idField = _helper.GetIdentifierFieldEntry(integrationPointID);
			string options = _helper.GetSourceOptions(integrationPointID);
			IDataReader idReader = provider.GetBatchableIds(idField, options);

			return new ReaderEnumerable(idReader);
		}

		public override void CreateBatchJob(Job job, List<string> batchIDs)
		{
			_jobManager.CreateJob(batchIDs, TaskType.SyncWorker, job.WorkspaceID, job.RelatedObjectArtifactID);
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

		private void JobPreExecute(Job job)
		{
			//do nothing
			return;
		}

		private void JobPostExecute(Job job, TaskResult taskResult)
		{
			try
			{
				int integrationPointID = job.RelatedObjectArtifactID;
				IntegrationPoint rdoIntegrationPoint =
					_caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);
				if (rdoIntegrationPoint != null)
				{
					rdoIntegrationPoint.LastRuntime = DateTime.UtcNow;
					rdoIntegrationPoint.NextScheduledRuntime = _jobService.GetJobNextUtcRunDateTime(job, _scheduleRuleFactory,
						taskResult);
					_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(rdoIntegrationPoint);
				}
			}
			catch { }
		}

		public void Dispose()
		{
			Debug.WriteLine("");
		}
	}
}
