using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobHistoryErrorService
	{
		private ICaseServiceContext _context;
		private List<JobHistoryError> _jobHistoryErrorList;
		public JobHistoryErrorService(ICaseServiceContext context)
		{
			_context = context;
			_jobHistoryErrorList = new List<JobHistoryError>();
		}

		public Data.JobHistory JobHistory { get; set; }

		//private IBatchReporter _batchReporter;
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
						_context.RsapiService.JobHistoryErrorLibrary.Create(_jobHistoryErrorList);
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
					throw new Exception("Could not commit Job History Errors. These are uncommited errors:" + Environment.NewLine +
															allErrors);
				}
				finally
				{
					_jobHistoryErrorList.Clear();
				}
			}
		}

		private void OnRowError(string documentIdentifier, string errorMessage)
		{
			AddError(ErrorTypeChoices.JobHistoryErrorItem, errorMessage);
		}

		private void OnJobError(Exception ex)
		{
			AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
		}

		public void AddError(Relativity.Client.Choice errorType, Exception ex)
		{
			AddError(errorType, GenerateErrorMessage(ex));
		}

		public void AddError(Relativity.Client.Choice errorType, string errorMessage)
		{
			lock (_jobHistoryErrorList)
			{
				JobHistoryError jobHistoryError = new JobHistoryError();
				//jobHistoryError.ParentArtifactId = this.JobHistory.ArtifactId;
				jobHistoryError.ParentArtifactId = this.JobHistory.ArtifactId;
				jobHistoryError.JobHistory = this.JobHistory.ArtifactId;
				jobHistoryError.Name = Guid.NewGuid().ToString();
				jobHistoryError.ErrorType = errorType;
				jobHistoryError.Error = errorMessage;
				jobHistoryError.TimestampUTC = DateTime.UtcNow;

				_jobHistoryErrorList.Add(jobHistoryError);
			}
		}

		private string GenerateErrorMessage(Exception ex)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(ex.Message);
			sb.AppendLine(ex.StackTrace);
			if (ex.InnerException != null) sb.AppendLine(GenerateErrorMessage(ex.InnerException));
			return sb.ToString();
		}
	}
}
