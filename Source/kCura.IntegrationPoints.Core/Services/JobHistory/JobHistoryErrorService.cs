using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Injection;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobHistoryErrorService : IJobHistoryErrorService
	{
		public const int ERROR_BATCH_SIZE = 500;
		private readonly ICaseServiceContext _context;
		private readonly List<JobHistoryError> _jobHistoryErrorList;
		private readonly IAPILog _logger;
		private bool _errorOccurredDuringJob;

		public JobHistoryErrorService(ICaseServiceContext context, IHelper helper)
		{
			_context = context;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<IJobHistoryErrorService>();
			_jobHistoryErrorList = new List<JobHistoryError>();
			_errorOccurredDuringJob = false;
			JobLevelErrorOccurred = false;
		}

		internal int PendingErrorCount => _jobHistoryErrorList.Count;

		public bool JobLevelErrorOccurred { get; private set; }

		public Data.JobHistory JobHistory { get; set; }
		public Data.IntegrationPoint IntegrationPoint { get; set; }
		public IJobStopManager JobStopManager { get; set; }

		public void SubscribeToBatchReporterEvents(object batchReporter)
		{
			if (batchReporter is IBatchReporter)
			{
				((IBatchReporter) batchReporter).OnJobError += OnJobError;
				((IBatchReporter) batchReporter).OnDocumentError += OnRowError;
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
						InjectionManager.Instance.Evaluate("9B9265FB-F63D-44D3-90A2-87C1570F746D");
						_errorOccurredDuringJob = true;

						if (IntegrationPoint != null)
						{
							IntegrationPoint.HasErrors = true;
						}

						_context.RsapiService.JobHistoryErrorLibrary.Create(_jobHistoryErrorList);
					}

					if (IntegrationPoint!=null && !_errorOccurredDuringJob || (JobStopManager?.IsStopRequested() == true) )
					{
						IntegrationPoint.HasErrors = false;
					}
				}
				catch (Exception ex)
				{
					//if failed to commit, throw all buffered errors as part of an exception
					List<string> errorList = _jobHistoryErrorList.Select(x =>
						x.ErrorType.Name.Equals(ErrorTypeChoices.JobHistoryErrorJob.Name)
							? $"{x.TimestampUTC} Type: {x.ErrorType.Name}    Error: {x.Error}"
							: $"{x.TimestampUTC} Type: {x.ErrorType.Name}    Identifier: {x.SourceUniqueID}    Error: {x.Error}").ToList();

					string allErrors = string.Join(Environment.NewLine, errorList.ToArray());
					allErrors += string.Format("{0}{0}Reason for exception: {1}", Environment.NewLine, ex.FlattenErrorMessages());

					LogCommittingErrorsFailed(ex, allErrors);

					throw new Exception("Could not commit Job History Errors. These are uncommitted errors:" + Environment.NewLine + allErrors);
				}
				finally
				{
					_jobHistoryErrorList.Clear();
					UpdateIntegrationPoint();
				}
			}
		}

		public void AddError(Choice errorType, Exception ex)
		{
			string message = ex.FlattenErrorMessages();

			if (ex is IntegrationPointValidationException)
			{
			    var ipException = ex as IntegrationPointValidationException;
			    message = string.Join(Environment.NewLine, ipException.ValidationResult.MessageTexts);
			}

		    AddError(errorType, string.Empty, ex.Message, message);
		}

		public void AddError(Choice errorType, string documentIdentifier, string errorMessage, string stackTrace)
		{
			lock (_jobHistoryErrorList)
			{
				if ((JobHistory != null) && (JobHistory.ArtifactId > 0))
				{
					DateTime now = DateTime.UtcNow;

					var jobHistoryError = new JobHistoryError
					{
						ParentArtifactId = JobHistory.ArtifactId,
						JobHistory = JobHistory.ArtifactId,
						Name = Guid.NewGuid().ToString(),
						ErrorType = errorType,
						ErrorStatus = ErrorStatusChoices.JobHistoryErrorNew,
						SourceUniqueID = documentIdentifier,
						Error = errorMessage,
						StackTrace = stackTrace,
						TimestampUTC = now
					};

					_jobHistoryErrorList.Add(jobHistoryError);

					if (errorType == ErrorTypeChoices.JobHistoryErrorJob)
					{
						JobLevelErrorOccurred = true;
						CommitErrors();
					} 
					else if(_jobHistoryErrorList.Count == ERROR_BATCH_SIZE)
					{
						CommitErrors();
					}
				}
				else
				{
					LogMissingJobHistoryError();
					//we can't create JobHistoryError without JobHistory,
					//in such case log error into Error Tab by throwing Exception.
					throw new Exception($"Type:{errorType.Name} Id:{documentIdentifier}  Error:{errorMessage}");
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

		private void UpdateIntegrationPoint()
		{
			try
			{
				if (IntegrationPoint != null)
				{
					InjectionManager.Instance.Evaluate("6a620133-011a-4fb8-8b37-758b53a46872");
					_context.RsapiService.RelativityObjectManager.Update(IntegrationPoint);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to update Integration Point's Has Error field.");
				//Ignore error, if we can't update the Integration Point's Has Errors Field, just continue on.
				//The field may be out of state with the true job status, or subsequent Update calls may succeed.
			}
		}
		
		#region Logging

		private void LogCommittingErrorsFailed(Exception ex, string allErrors)
		{
			_logger.LogError(ex, "Could not commit Job History Errors. These are uncommitted errors: {Errors}.", allErrors);
		}

		private void LogMissingJobHistoryError()
		{
			_logger.LogError("Failed to create Job History Error: Job History doesn't exists.");
		}
		
		#endregion
	}
}