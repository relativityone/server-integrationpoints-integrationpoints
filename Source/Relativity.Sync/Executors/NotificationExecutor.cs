using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.EmailNotifications;
using Relativity.Services.EmailNotificationsManager;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class NotificationExecutor : IExecutor<INotificationConfiguration>
	{
		private const string _SUBJECT_COMPLETE = "Relativity Job successfully completed for '{0}'";
		private const string _SUBJECT_COMPLETE_ERRORS = "Relativity Job completed with errors for '{0}'";
		private const string _SUBJECT_FAILED = "Relativity Job failed for '{0}'";
		private const string _SUBJECT_STOPPED = "Relativity Job stopped for '{0}'";

		private const string _BODY_NAME = "Name: {0}";
		private const string _BODY_SOURCE = "Source Workspace: {0}";
		private const string _BODY_DESTINATION = "Destination Workspace: {0}";
		private const string _BODY_ERROR = "Error: {0}";

		private const string _MESSAGE_COMPLETE = "A job for the following Relativity Integration Point has successfully completed.";
		private const string _MESSAGE_COMPLETE_ERRORS = "A job for the following Relativity Integration Point has successfully completed with errors.";
		private const string _MESSAGE_FAILED = "A job for the following Relativity Integration Point has failed to complete.";
		private const string _MESSAGE_STOPPED = "A job for the following Relativity Integration Point has been stopped.";

		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly IProgressRepository _progressRepository;
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISyncLog _logger;

		public NotificationExecutor(ISourceServiceFactoryForAdmin serviceFactory, IProgressRepository progressRepository,
			IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository, IJobHistoryErrorRepository jobHistoryErrorRepository, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_progressRepository = progressRepository;
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(INotificationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Sending notifications.");

			try
			{
				EmailNotificationRequest emailRequest = await GetEmailNotificationRequest(configuration, token).ConfigureAwait(false);
				using (var emailManager = await _serviceFactory.CreateProxyAsync<IEmailNotificationsManager>().ConfigureAwait(false))
				{
					await emailManager.SendEmailNotificationAsync(emailRequest).ConfigureAwait(false);
				}
			}
			catch (Exception emailRequestException)
			{
				_logger.LogWarning(emailRequestException,
					"Failed to send a notification email for job with ID {JobHistoryArtifactID} in workspace ID {WorkspaceArtifactID}.",
					configuration.JobHistoryArtifactId, configuration.SourceWorkspaceArtifactId);
			}
			return ExecutionResult.Success();
		}

		private async Task<EmailNotificationRequest> GetEmailNotificationRequest(INotificationConfiguration configuration, CancellationToken token)
		{
			var emailRequest = new EmailNotificationRequest
			{
				Recipients = configuration.EmailRecipients,
				IsBodyHtml = false
			};

			IReadOnlyCollection<IProgress> progresses = await _progressRepository.QueryAllAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);

			SyncJobStatus jobStatus = SyncJobStatus.Failed;
			if (progresses != null && progresses.Any())
			{
				jobStatus = progresses.OrderByDescending(x => x.Status).First().Status;
			}
			await FillEmailRequest(emailRequest, configuration, jobStatus, token).ConfigureAwait(false);
			return emailRequest;
		}

		private async Task FillEmailRequest(EmailNotificationRequest emailRequest, INotificationConfiguration configuration, SyncJobStatus jobStatus, CancellationToken token)
		{
			async Task FillSubjectAndBody(string subject, string message)
			{
				emailRequest.Subject = string.Format(CultureInfo.InvariantCulture, subject, configuration.JobName);
				emailRequest.Body = await GenerateMessage(configuration, message, token).ConfigureAwait(false);
			}

			async Task FillSubjectAndBodyForError(string subject)
			{
				emailRequest.Subject = string.Format(CultureInfo.InvariantCulture, subject, configuration.JobName);
				emailRequest.Body = await GenerateErrorMessage(configuration, token).ConfigureAwait(false);
			}

			switch (jobStatus)
			{
				case SyncJobStatus.Cancelled:
					await FillSubjectAndBody(_SUBJECT_STOPPED, _MESSAGE_STOPPED).ConfigureAwait(false);
					break;
				case SyncJobStatus.CompletedWithErrors:
					await FillSubjectAndBody(_SUBJECT_COMPLETE_ERRORS, _MESSAGE_COMPLETE_ERRORS).ConfigureAwait(false);
					break;
				case SyncJobStatus.Completed:
					await FillSubjectAndBody(_SUBJECT_COMPLETE, _MESSAGE_COMPLETE).ConfigureAwait(false);
					break;
				default:
					await FillSubjectAndBodyForError(_SUBJECT_FAILED).ConfigureAwait(false);
					break;
			}
		}

		private async Task<string> GenerateErrorMessage(INotificationConfiguration configuration, CancellationToken token)
		{
			string body = await GenerateMessage(configuration, _MESSAGE_FAILED, token).ConfigureAwait(false);
			string errorBody = await GenerateErrorBody(configuration).ConfigureAwait(false);

			string fullErrorBody = string.Join($"{Environment.NewLine}{Environment.NewLine}", body, errorBody);
			return fullErrorBody;
		}

		private async Task<string> GenerateMessage(INotificationConfiguration configuration, string headerMessage, CancellationToken token)
		{
			string body = await GenerateJobInfoBody(configuration, token).ConfigureAwait(false);
			string fullBody = string.Join($"{Environment.NewLine}{Environment.NewLine}", headerMessage, body);
			return fullBody;
		}

		private async Task<string> GenerateJobInfoBody(INotificationConfiguration configuration, CancellationToken token)
		{
			string nameBody = string.Format(CultureInfo.InvariantCulture, _BODY_NAME, configuration.JobName);
			string sourceBody = string.Format(CultureInfo.InvariantCulture, _BODY_SOURCE, configuration.SourceWorkspaceTag);

			string destinationInfo = await GetDestinationWorkspaceInformation(configuration, token).ConfigureAwait(false);
			string destinationBody = string.Format(CultureInfo.InvariantCulture, _BODY_DESTINATION, destinationInfo);

			string body = string.Join($"{Environment.NewLine}{Environment.NewLine}", nameBody, sourceBody, destinationBody);
			return body;
		}

		private async Task<string> GetDestinationWorkspaceInformation(INotificationConfiguration configuration, CancellationToken token)
		{
			string destinationInfo = string.Empty;
			try
			{
				DestinationWorkspaceTag destinationWorkspaceTag = await _destinationWorkspaceTagRepository
					.ReadAsync(configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);

				destinationInfo = $"{destinationWorkspaceTag.DestinationInstanceName} - {destinationWorkspaceTag.DestinationWorkspaceName} - {destinationWorkspaceTag.DestinationWorkspaceArtifactId}";
			}
			catch (Exception readTagException)
			{
				_logger.LogWarning(readTagException,
					"Failed to retrieve the destination workspace tag information for source workspace {SourceWorkspace} and destination workspace {DestinationWorkspace}.",
					configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId);
			}
			return destinationInfo;
		}

		private async Task<string> GenerateErrorBody(INotificationConfiguration configuration)
		{
			string errorMessage = await GetJobHistoryErrorInformation(configuration).ConfigureAwait(false);
			string errorBody = string.Format(CultureInfo.InvariantCulture, _BODY_ERROR, errorMessage);
			return errorBody;
		}

		private async Task<string> GetJobHistoryErrorInformation(INotificationConfiguration configuration)
		{
			string errorMessage = string.Empty;
			try
			{
				IJobHistoryError jobHistoryError = await _jobHistoryErrorRepository.GetLastJobErrorAsync(configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId).ConfigureAwait(false);
				errorMessage = jobHistoryError?.ErrorMessage ?? string.Empty;
			}
			catch (Exception readErrorException)
			{
				_logger.LogWarning(readErrorException,
					"Failed to retrieve the job history error information for source workspace {SourceWorkspace} and destination workspace {DestinationWorkspace}.",
					configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId);
			}
			return errorMessage;
		}
	}
}