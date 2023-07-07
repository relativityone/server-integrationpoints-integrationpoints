using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.EmailNotifications;
using Relativity.Services.EmailNotificationsManager;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications
{
    internal class NotificationService : INotificationService
    {
        private readonly IKeplerServiceFactory _keplerServiceFactory;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IAPILog _logger;

        public NotificationService(
            IJobHistoryService jobHistoryService,
            IKeplerServiceFactory keplerServiceFactory,
            IJobHistoryErrorService jobHistoryErrorService,
            IAPILog logger)
        {
            _jobHistoryService = jobHistoryService;
            _keplerServiceFactory = keplerServiceFactory;
            _jobHistoryErrorService = jobHistoryErrorService;
            _logger = logger;
        }

        public async Task PrepareAndSendEmailNotificationAsync(ImportJobContext jobContext, IntegrationPointDto integrationPoint)
        {
            try
            {
                List<string> emailRecipients = GetRecipientsList(integrationPoint);

                if (emailRecipients.Any())
                {
                    _logger.LogInformation("Sending notification to {emailCount} recipient(s)", emailRecipients.Count);

                    EmailNotificationRequest emailRequest = await GetEmailNotificationRequestAsync(jobContext, emailRecipients).ConfigureAwait(false);
                    using (IEmailNotificationsManager emailManager = await _keplerServiceFactory.CreateProxyAsync<IEmailNotificationsManager>().ConfigureAwait(false))
                    {
                        await emailManager.SendEmailNotificationAsync(emailRequest).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                   ex,
                   "Failed to send a notification email for job with ID {JobHistoryArtifactID} in workspace ID {WorkspaceArtifactID}.",
                   jobContext.JobHistoryId,
                   jobContext.WorkspaceId);
            }
        }

        private async Task<EmailNotificationRequest> GetEmailNotificationRequestAsync(ImportJobContext jobContext, List<string> emailRecipients)
        {
            Data.JobHistory jobHistory = await _jobHistoryService.ReadJobHistoryByGuidAsync(jobContext.WorkspaceId, jobContext.JobHistoryGuid).ConfigureAwait(false);

            if (jobHistory == null)
            {
                throw new Exception($"Notification request preparation failed. Could not get job history with id: {jobContext.JobHistoryId}");
            }

            string subject = GetSubject(jobHistory);
            string body = await GetEmailBody(jobContext.WorkspaceId, jobHistory).ConfigureAwait(false);

            EmailNotificationRequest emailRequest = new EmailNotificationRequest
            {
                Subject = subject,
                Recipients = emailRecipients,
                Body = body,
                IsBodyHtml = true
            };

            return emailRequest;
        }

        private async Task<string> GetEmailBody(int sourceWorkspaceArtifactId, Data.JobHistory jobHistory)
        {
            StringBuilder emailBodyBuilder = new StringBuilder();

            emailBodyBuilder.AppendLine(NotificationConstants._MESSAGE_CONTENT.ToH3HeaderHtml());
            emailBodyBuilder.AppendLine(NotificationConstants._BODY_NAME.ToLineWithBoldedSectionHtml(jobHistory.Name));
            emailBodyBuilder.AppendLine(NotificationConstants._BODY_DESTINATION.ToLineWithBoldedSectionHtml(jobHistory.DestinationWorkspace));
            emailBodyBuilder.AppendLine(NotificationConstants._BODY_STATUS.ToLineWithBoldedSectionHtml(jobHistory.JobStatus.Name));
            emailBodyBuilder.AppendLine(NotificationConstants._BODY_STATISTICS.ToJobStatisticsFormattedHtml(jobHistory));

            if (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryErrorJobFailed))
            {
                Data.JobHistoryError error = await _jobHistoryErrorService.GetLastJobLevelError(sourceWorkspaceArtifactId, jobHistory.ArtifactId).ConfigureAwait(false);
                string errorMsg = error?.Error ?? NotificationConstants._ERROR_DEFAULT_MSG;
                emailBodyBuilder.AppendLine(NotificationConstants._BODY_ERROR.ToLineWithBoldedSectionHtml(errorMsg));
            }

            return emailBodyBuilder.ToString();
        }

        private string GetSubject(Data.JobHistory jobHistory)
        {
            return string.Format(CultureInfo.InvariantCulture, NotificationConstants._SUBJECT_CONTENT, jobHistory.Name, jobHistory.JobStatus.Name);
        }

        private List<string> GetRecipientsList(IntegrationPointDto integrationPoint)
        {
            List<string> recipients = new List<string>();
            if (!string.IsNullOrWhiteSpace(integrationPoint.EmailNotificationRecipients))
            {
                recipients = integrationPoint.EmailNotificationRecipients
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                    .ToList();
            }
            return recipients;
        }
    }
}
