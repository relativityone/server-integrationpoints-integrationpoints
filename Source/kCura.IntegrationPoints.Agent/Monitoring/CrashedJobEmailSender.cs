using System;
using System.Linq;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IntegrationPointRdoService;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.Notifications;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using JobHistoryErrorService = kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError.JobHistoryErrorService;

namespace kCura.IntegrationPoints.Agent.Monitoring
{
    public class CrashedJobEmailSender
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IAPILog _log;

        public CrashedJobEmailSender(IServicesMgr servicesMgr, IRelativityObjectManager objectManager, IAPILog log)
        {
            _servicesMgr = servicesMgr;
            _objectManager = objectManager;
            _log = log;
        }

        public void SendEmailNotifications(Job job, IntegrationPoint integrationPoint)
        {
            if (string.IsNullOrWhiteSpace(integrationPoint.EmailNotificationRecipients))
            {
                _log.LogInformation("Notification email for crashed job will not be send because recipients list is empty");
                return;
            }

            try
            {
                QueryRequest request = new QueryRequest()
                {
                    Condition = $"'{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{job.RelatedObjectArtifactID}] AND '{JobHistoryFields.JobStatus}' == CHOICE {JobStatusChoices.JobHistoryErrorJobFailedGuid}",
                    Sorts = new[]
                    {
                        new Sort()
                        {
                            Direction = SortEnum.Descending,
                            FieldIdentifier = new FieldRef()
                            {
                                Guid = JobHistoryFieldGuids.EndTimeUTCGuid
                            }
                        }
                    }
                };

                JobHistory jobHistory = _objectManager.Query<JobHistory>(request, 0, 1).Items.FirstOrDefault();

                if (jobHistory != null)
                {
                    INotificationService notificationService = GetNotificationService();

                    Guid jobHistoryGuid = new Guid(jobHistory.BatchInstance);
                    notificationService.PrepareAndSendEmailNotificationAsync(new ImportJobContext(job.WorkspaceID, job.JobId, jobHistoryGuid, jobHistory.ArtifactId), integrationPoint.EmailNotificationRecipients).GetAwaiter().GetResult();
                }
                else
                {
                    _log.LogError("Cannot send notification email for crashed job because Job History has not been found");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to send notification email for crashed job");
            }
        }

        private INotificationService GetNotificationService()
        {
            IKeplerServiceFactory serviceFactory = new ServiceFactory(_servicesMgr, new DynamicProxyFactory(() => new StopwatchWrapper(), _log), _log);
            IIntegrationPointRdoService integrationPointRdoService = new IntegrationPointRdoService(serviceFactory, _log);
            IDateTime dateTime = new DateTimeWrapper();
            IJobHistoryService jobHistoryService = new JobHistoryService(serviceFactory, dateTime, integrationPointRdoService, _log);
            JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(serviceFactory, new DefaultGuidService(), dateTime, new RetryHandler(_log), _log);
            return new NotificationService(jobHistoryService, serviceFactory, jobHistoryErrorService, _log);
        }
    }
}