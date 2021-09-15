using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Helpers;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.InstanceSetting;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
    [IdentifiedTestFixture("C4243B32-D40D-4922-B45A-9A0276393CAE")]
    [TestLevel.L1]
    public class SendEmailWorkerTests : TestsBase
    {
        private const string _EMAIL_FROM_ADDRESS = "rip.developer@relativity.com";
        private const string _EMAIL_TO_ADDRESS = "relativity.admin@kcura.com";
        private const string _EMAIL_SUBJECT = "Test";
        private const string _EMAIL_BODY = "Integrations";
        private const int _SMTP_PORT = 25;

        private const string _SMTP_IS_SSL_ENABLED = "false";
        private const bool _SMTP_USE_SSL = false;
        private const string _SMTP_PASSWORD_SETTING = "A7Pass";
        private const string _SMTP_USER_NAME_SETTING = "A7";

        private string _SMTP_SERVER_NAME = $"127.0.0.1:{_SMTP_PORT}";

        private IJobService _jobService;
        // private IInstanceSettingManager _instanceSettingsManager;
        // private SendEmailWorker _sut;


        public override void SetUp()
        {
            base.SetUp();

            //_instanceSettingsManager = Helper.CreateProxy<IInstanceSettingManager>();
            //string localComputerHostname = Dns.GetHostName();

            //SetNotificationInstanceSettings(localComputerHostname).ConfigureAwait(false);
            _jobService = Container.Resolve<IJobService>();
            //_sut = Container.Resolve<SendEmailWorker>(); ;
        }

        [IdentifiedTest("3E578A6E-D86A-4711-93C9-DB6A1C562C65")]
        public void SyncWorker_ShouldAddSendEmailWorkerToScheduleQueue()
        {
            // Arrange
            const int numberOfRecords = 1000;
            string xmlPath = PrepareRecords(numberOfRecords);
            JobTest job = PrepareSyncWorkerJob(xmlPath, out JobHistoryTest jobHistory);
            SyncWorker sut = PrepareSyncWorkerSut((importJob) => { importJob.Complete(); });

            jobHistory.TotalItems = 2000;

            // Act
            sut.Execute(job.AsJob());

            // Assert
            jobHistory.ItemsTransferred.Should().Be(numberOfRecords);
            FakeRelativityInstance.JobsInQueue.Count.ShouldBeEquivalentTo(2);
            FakeRelativityInstance.JobsInQueue[1].TaskType.ShouldBeEquivalentTo(TaskType.SendEmailWorker.ToString());
        }

        [IdentifiedTest("DD858EA0-4588-4BE2-A3FF-F8C29E1F557B")]
        public async Task SyncWorker_SendEmailWorkerShouldSendEmailToServer()
        {
            // Arrange
            FakeSmtpMessage receivedMessage;
            const int numberOfRecords = 1000;
            string xmlPath = PrepareRecords(numberOfRecords);
            JobTest job = PrepareSendEmailWorkerJob(xmlPath, out JobHistoryTest jobHistory);
            SendEmailWorker sut = PrepareSendEmailWorkerSut((importJob) => { importJob.Complete(); });

            jobHistory.TotalItems = 2000;

            using (FakeSmtpServer fakeSmtpServer = FakeSmtpServer.Start(_SMTP_PORT))
            {
                // act
                sut.Execute(job.AsJob());
                // AddSendingEmailJobToQueue(100027, TaskType.SendEmailWorker);

                receivedMessage = await fakeSmtpServer.GetFirstMessage(TimeSpan.FromSeconds(5))
                    .ConfigureAwait(false);
            }

            // Assert
            AssertReceivedMessage(receivedMessage);
        }

        private string PrepareRecords(int numberOfRecords)
        {
            string xml = new MyFirstProviderXmlGenerator().GenerateRecords(numberOfRecords);
            string tmpPath = Path.GetTempFileName();
            File.WriteAllText(tmpPath, xml);
            return tmpPath;
        }

        //private async Task SetNotificationInstanceSettings(string localComputerHostname)
        //{
        //    await UpdateNotificationInstanceSettings("SMTPPort", _SMTP_PORT.ToString()).ConfigureAwait(false);
        //    await UpdateNotificationInstanceSettings("SMTPServer", localComputerHostname).ConfigureAwait(false);
        //    await UpdateNotificationInstanceSettings("SMTPSSLisRequired", _SMTP_IS_SSL_ENABLED).ConfigureAwait(false);
        //    await UpdateNotificationInstanceSettings("EmailFrom", _EMAIL_FROM_ADDRESS).ConfigureAwait(false);
        //}

        //private async Task UpdateNotificationInstanceSettings(string settingName, string value)
        //{
        //    Relativity.Services.InstanceSetting.InstanceSetting initialPassword = await GetNotificationInstanceSettings(settingName).ConfigureAwait(false);

        //    initialPassword.Value = value;
        //    await _instanceSettingsManager.UpdateSingleAsync(initialPassword).ConfigureAwait(false);
        //}

        //private async Task<Relativity.Services.InstanceSetting.InstanceSetting> GetNotificationInstanceSettings(string settingName)
        //{
        //    Relativity.Services.Query query = new Relativity.Services.Query
        //    {
        //        Condition = $"'Section' == 'kCura.Notification' AND 'Name' == '{settingName}'"
        //    };
        //    InstanceSettingQueryResultSet result = await _instanceSettingsManager.QueryAsync(query).ConfigureAwait(false);
        //    return result.Results.Single().Artifact;
        //}
        
        private SyncWorker PrepareSyncWorkerSut(Action<FakeJobImport> importAction)
        {
            Container.Register(Component.For<IDataSourceProvider>()
                .ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>()
                .Named(MyFirstProvider.Provider.GlobalConstants.FIRST_PROVIDER_GUID));

            Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport(importAction))
                .LifestyleSingleton());

            FakeInstanceSettingsBundle fakeInstanceSettingsBundle = new FakeInstanceSettingsBundle();

            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPServer", _SMTP_SERVER_NAME);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPPassword", _SMTP_PASSWORD_SETTING);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPSSLisRequired", _SMTP_IS_SSL_ENABLED);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPPort", _SMTP_PORT);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPUserName", _SMTP_USER_NAME_SETTING);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("EmailFrom", _EMAIL_FROM_ADDRESS);

            Container.Register(Component.For<IInstanceSettingsBundle>().Instance(fakeInstanceSettingsBundle).LifestyleTransient().IsDefault());

            //FakeAgent fakeAgent = Container.Resolve<FakeAgent>();
            //fakeAgent.ShouldRunOnce = false;

            SyncWorker sut = Container.Resolve<SyncWorker>();
            //SendEmailWorker sut = Container.Resolve<SendEmailWorker>();
            return sut;
        }

        private JobTest PrepareSyncWorkerJob(string xmlPath, out JobHistoryTest jobHistory)
        {
            AgentTest agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

            SourceProviderTest provider =
                SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
                    identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

            integrationPoint.SourceProvider = provider.ArtifactId;
            integrationPoint.SourceConfiguration = xmlPath;
            integrationPoint.EmailNotificationRecipients = _EMAIL_TO_ADDRESS;

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
            jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
            string[] recordsIds = XDocument.Load(xmlPath).XPathSelectElements("//Name").Select(x => x.Value).ToArray();

            taskParameters.BatchParameters = recordsIds;
            
            job.JobDetails = Serializer.Serialize(taskParameters);
            job.LockedByAgentID = agent.ArtifactId;
            job.RootJobId = JobId.Next;

            InsertBatchToJobTrackerTable(job, jobHistory);

            RegisterJobContext(job);

            return job;
        }

        private JobTest PrepareSendEmailWorkerJob(string xmlPath, out JobHistoryTest jobHistory)
        {
            AgentTest agent = FakeRelativityInstance.Helpers.AgentHelper.CreateIntegrationPointAgent();

            SourceProviderTest provider =
                SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
                    identifierFieldName: "Name", sourceProviderConfiguration: xmlPath);

            integrationPoint.SourceProvider = provider.ArtifactId;
            integrationPoint.SourceConfiguration = xmlPath;
            integrationPoint.EmailNotificationRecipients = _EMAIL_TO_ADDRESS;

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);
            jobHistory = SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

            TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
            string[] recordsIds = XDocument.Load(xmlPath).XPathSelectElements("//Name").Select(x => x.Value).ToArray();

            taskParameters.BatchParameters = recordsIds;

            EmailJobParameters message = new EmailJobParameters
            {
                Subject = _EMAIL_SUBJECT,
                MessageBody = _EMAIL_BODY,
                Emails = new[] { _EMAIL_TO_ADDRESS }
            };

            var jobDetails = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            job.JobDetails = jobDetails;
            job.LockedByAgentID = agent.ArtifactId;
            job.RootJobId = JobId.Next;

            InsertBatchToJobTrackerTable(job, jobHistory);

            RegisterJobContext(job);

            return job;
        }

        private SendEmailWorker PrepareSendEmailWorkerSut(Action<FakeJobImport> importAction)
        {
            Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport(importAction))
                .LifestyleSingleton());

            FakeInstanceSettingsBundle fakeInstanceSettingsBundle = (FakeInstanceSettingsBundle)Container.Resolve<IInstanceSettingsBundle>();

            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPServer", Dns.GetHostName());
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPPassword", _SMTP_PASSWORD_SETTING);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPSSLisRequired", _SMTP_USE_SSL);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPPort", (uint?)_SMTP_PORT);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("SMTPUserName", _SMTP_USER_NAME_SETTING);
            fakeInstanceSettingsBundle.SmtpConfigurationSettings.Add("EmailFrom", _EMAIL_FROM_ADDRESS);

            Container.Register(Component.For<IInstanceSettingsBundle>().Instance(fakeInstanceSettingsBundle).LifestyleTransient().IsDefault());
            
            SendEmailWorker sut = Container.Resolve<SendEmailWorker>();
            return sut;
        }

        private void InsertBatchToJobTrackerTable(JobTest job, JobHistoryTest jobHistory)
        {
            string tableName = string.Format("RIP_JobTracker_{0}_{1}_{2}", job.WorkspaceID, job.RootJobId, jobHistory.BatchInstance);

            if (!FakeRelativityInstance.JobTrackerResourceTables.ContainsKey(tableName))
            {
                FakeRelativityInstance.JobTrackerResourceTables[tableName] = new List<JobTrackerTest>();
            }

            FakeRelativityInstance.JobTrackerResourceTables[tableName].Add(new JobTrackerTest { JobId = job.JobId });
        }

        private void AddSendingEmailJobToQueue(int integrationPointArtifactId, TaskType taskType)
        {
            EmailJobParameters message = new EmailJobParameters
            {
                Subject = _EMAIL_SUBJECT,
                MessageBody = _EMAIL_BODY,
                Emails = new[] { _EMAIL_TO_ADDRESS }
            };

            var jobDetails = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            _jobService.CreateJob(
                workspaceID: SourceWorkspace.ArtifactId,
                relatedObjectArtifactID: integrationPointArtifactId,
                taskType: taskType.ToString(),
                nextRunTime: DateTime.UtcNow,
                jobDetails: jobDetails,
                SubmittedBy: 9,
                rootJobID: null,
                parentJobID: null
            );
        }

        private static void AssertReceivedMessage(FakeSmtpMessage receivedMessage)
        {
            receivedMessage.Should().NotBeNull("message should be send to SMTP server");
            receivedMessage.FromAddress.Should().Be(_EMAIL_FROM_ADDRESS);
            receivedMessage.ToAddresses.Should().ContainSingle()
                .Which.Should().Be(_EMAIL_TO_ADDRESS);
            receivedMessage.Subject.Should().Be(_EMAIL_SUBJECT);
            receivedMessage.Data.Should().Contain(_EMAIL_BODY);
        }
    }
}
