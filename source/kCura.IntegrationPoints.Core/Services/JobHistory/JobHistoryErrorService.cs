using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Extensions;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobHistoryErrorService : IJobHistoryErrorService
	{
		private readonly ICaseServiceContext _context;
		private readonly List<JobHistoryError> _jobHistoryErrorList;
		private bool _errorOccurredDuringJob;
		public bool JobLevelErrorOccurred { get; private set; }
		public const int ERROR_BATCH_SIZE = 500;

		public JobHistoryErrorService(ICaseServiceContext context)
		{
			_context = context;
			_jobHistoryErrorList = new List<JobHistoryError>();
			_errorOccurredDuringJob = false;
			JobLevelErrorOccurred = false;
		}

		public Data.JobHistory JobHistory { get; set; }
		public IntegrationPoint IntegrationPoint { get; set; }
		public IJobStopManager JobStopManager { get; set; }

		public void SubscribeToBatchReporterEvents(object batchReporter)
		{
			if (batchReporter is IBatchReporter)
			{
				((IBatchReporter)batchReporter).OnJobError += new JobError(OnJobError);
				((IBatchReporter)batchReporter).OnDocumentError += new RowError(OnRowError);
			}
		}

		public void CommitErrors()
		{
			lock (_jobHistoryErrorList)
			{
				try
				{
					if (_jobHistoryErrorList.Any())
					{
						kCura.Method.Injection.InjectionManager.Instance.Evaluate("9B9265FB-F63D-44D3-90A2-87C1570F746D");
						_errorOccurredDuringJob = true;
						IntegrationPoint.HasErrors = true;

						_context.RsapiService.JobHistoryErrorLibrary.Create(_jobHistoryErrorList);
					}

					if (!_errorOccurredDuringJob || JobStopManager?.IsStopRequested() == true)
					{
						IntegrationPoint.HasErrors = false;
					}
				}
				catch (Exception ex)
				{
					//if failed to commit, throw all buffered errors as part of an exception
					string allErrors = string.Empty;
					List<string> errorList = _jobHistoryErrorList.Select(x =>
						((x.ErrorType.Name.Equals(ErrorTypeChoices.JobHistoryErrorJob.Name))
							? string.Format("{0} Type: {1}    Error: {2}", x.TimestampUTC, x.ErrorType.Name, x.Error)
							: string.Format("{0} Type: {1}    Identifier: {2}    Error: {3}", x.TimestampUTC, x.ErrorType.Name,
								x.SourceUniqueID, x.Error))).ToList();
					allErrors = String.Join(Environment.NewLine, errorList.ToArray());
					allErrors += string.Format("{0}{0}Reason for exception: {1}", Environment.NewLine, ex.FlattenErrorMessages());
					throw new Exception("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine + allErrors);
				}
				finally
				{
					_jobHistoryErrorList.Clear();
					UpdateIntegrationPoint();
				}
			}
		}

		private void OnRowError(string documentIdentifier, string errorMessage)
		{
			if (IntegrationPoint.LogErrors.GetValueOrDefault(false))
			{
				if (JobStopManager?.IsStopRequested() == true)
				{
					return;
				}
				AddError(ErrorTypeChoices.JobHistoryErrorItem, documentIdentifier, errorMessage, errorMessage);
			}
		}

		private void OnJobError(Exception ex)
		{
			AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
		}

		public void AddError(Relativity.Client.Choice errorType, Exception ex)
		{
			AddError(errorType, string.Empty, ex.Message, ex.FlattenErrorMessages());
		}

		public void AddError(Relativity.Client.Choice errorType, string documentIdentifier, string errorMessage, string stackTrace)
		{
			lock (_jobHistoryErrorList)
			{
				if (this.JobHistory != null && this.JobHistory.ArtifactId > 0)
				{
					DateTime now = DateTime.UtcNow;

					JobHistoryError jobHistoryError = new JobHistoryError();
					jobHistoryError.ParentArtifactId = this.JobHistory.ArtifactId;
					jobHistoryError.JobHistory = this.JobHistory.ArtifactId;
					jobHistoryError.Name = Guid.NewGuid().ToString();
					jobHistoryError.ErrorType = errorType;
					jobHistoryError.ErrorStatus = ErrorStatusChoices.JobHistoryErrorNew;
					jobHistoryError.SourceUniqueID = documentIdentifier;
					jobHistoryError.Error = errorMessage;
					jobHistoryError.StackTrace = stackTrace;
					jobHistoryError.TimestampUTC = now;

					_jobHistoryErrorList.Add(jobHistoryError);

					if (errorType == ErrorTypeChoices.JobHistoryErrorJob)
					{
						JobLevelErrorOccurred = true;
					}

					if (_jobHistoryErrorList.Count == ERROR_BATCH_SIZE)
					{
						CommitErrors();
					}
				}
				else
				{
					//we can't create JobHistoryError without JobHistory,
					//in such case log error into Error Tab by throwing Exception.
					throw new System.Exception(string.Format("Type:{0}  Id:{1}  Error:{2}", errorType.Name, documentIdentifier, errorMessage));
				}
			}
		}

		private void UpdateIntegrationPoint()
		{
			try
			{
				if (IntegrationPoint != null)
				{
					kCura.Method.Injection.InjectionManager.Instance.Evaluate("6a620133-011a-4fb8-8b37-758b53a46872");
					_context.RsapiService.IntegrationPointLibrary.Update(IntegrationPoint);
				}
			}
			catch
			{
				//Ignore error, if we can't update the Integration Point's Has Errors Field, just continue on.
				//The field may be out of state with the true job status, or subsequent Update calls may succeed.
			}
		}

		internal int PendingErrorCount
		{
			get { return _jobHistoryErrorList.Count; }
		}
	}
}