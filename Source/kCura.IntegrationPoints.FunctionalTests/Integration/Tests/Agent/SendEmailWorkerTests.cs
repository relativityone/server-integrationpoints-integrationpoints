using System;
using System.Net;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
    [IdentifiedTestFixture("C4243B32-D40D-4922-B45A-9A0276393CAE")]
    [TestExecutionCategory.CI, TestLevel.L1]
    public class SendEmailWorkerTests : TestsBase
    {
        private const string _EMAIL_FROM_ADDRESS = "rip.developer@relativity.com";
        private const string _EMAIL_TO_ADDRESS = "relativity.admin@kcura.com";
        private const string _EMAIL_SUBJECT = "Test";
        private const string _EMAIL_BODY = "Integrations";
        private const int _SMTP_PORT = 25;
        private const bool _SMTP_USE_SSL = false;
        private const string _SMTP_PASSWORD_SETTING = "A7Pass";
        private const string _SMTP_USER_NAME_SETTING = "A7";
        
        [IdentifiedTest("3E578A6E-D86A-4711-93C9-DB6A1C562C65")]
        public void SyncWorkerShouldAddSendEmailTaskTypeToScheduleQueue()
        {
            // Arrange
            MyFirstProviderUtil myFirstProviderUtil = new MyFirstProviderUtil(Container, FakeRelativityInstance,
                SourceWorkspace, Serializer);

            const int numberOfRecords = 1000;
            string xmlPath = myFirstProviderUtil.PrepareRecords(numberOfRecords);
            JobTest job = myFirstProviderUtil.PrepareJob(xmlPath, out JobHistoryTest jobHistory, RegisterJobContext, _EMAIL_TO_ADDRESS);
            SyncWorker sut = myFirstProviderUtil.PrepareSut((importJob) => { importJob.Complete(); });

            jobHistory.TotalItems = 2000;

            // Act
            sut.Execute(job.AsJob());

            // Assert
            jobHistory.ItemsTransferred.Should().Be(numberOfRecords);
            FakeRelativityInstance.JobsInQueue.Count.ShouldBeEquivalentTo(2);
            FakeRelativityInstance.JobsInQueue[1].TaskType.ShouldBeEquivalentTo(TaskType.SendEmailWorker.ToString());
        }

        [IdentifiedTest("DD858EA0-4588-4BE2-A3FF-F8C29E1F557B")]
        public async Task SendEmailWorkerShouldSendEmailToServer()
        {
            // Arrange
            FakeSmtpMessage receivedMessage;
            JobTest job = PrepareSendEmailWorkerJob();
            SendEmailWorker sut = PrepareSendEmailWorkerSut();

            using (FakeSmtpServer fakeSmtpServer = FakeSmtpServer.Start(_SMTP_PORT))
            {
                // act
                sut.Execute(job.AsJob());

                receivedMessage = await fakeSmtpServer.GetFirstMessage(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }

            // Assert
            AssertReceivedMessage(receivedMessage);
        }

        [IdentifiedTest("30C43B6D-396E-458A-AB28-630E735FB02C")]
        public async Task SendEmailWorkerWithBatchInstanceIdShouldSendEmailToServer()
        {
            // Arrange
            FakeSmtpMessage receivedMessage;
            JobTest job = PrepareSendEmailWorkerJob(true);
            SendEmailWorker sut = PrepareSendEmailWorkerSut();

            using (FakeSmtpServer fakeSmtpServer = FakeSmtpServer.Start(_SMTP_PORT))
            {
                // act
                sut.Execute(job.AsJob());

                receivedMessage = await fakeSmtpServer.GetFirstMessage(TimeSpan.FromSeconds(2))
                    .ConfigureAwait(false);
            }

            // Assert
            AssertReceivedMessage(receivedMessage);
        }

        private JobTest PrepareSendEmailWorkerJob(bool withBatchInstance = false)
        {
            SourceProviderTest provider =
                SourceWorkspace.Helpers.SourceProviderHelper.CreateMyFirstProvider();

            IntegrationPointTest integrationPoint =
                SourceWorkspace.Helpers.IntegrationPointHelper.CreateImportIntegrationPoint(provider,
                    identifierFieldName: "Name", sourceProviderConfiguration: null);

            JobTest job =
                FakeRelativityInstance.Helpers.JobHelper.ScheduleIntegrationPointRun(SourceWorkspace, integrationPoint);

            string jobDetails;

            EmailJobParameters message = new EmailJobParameters
            {
                Subject = _EMAIL_SUBJECT,
                MessageBody = _EMAIL_BODY,
                Emails = new[] { _EMAIL_TO_ADDRESS }
            };

            if (withBatchInstance)
            {
                TaskParameters taskParameters = new TaskParameters
                {
                    BatchInstance = Guid.NewGuid(),
                    BatchParameters = message
                };

                jobDetails = Newtonsoft.Json.JsonConvert.SerializeObject(taskParameters);

            }
            else
            {
                jobDetails = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            }
            

            job.JobDetails = jobDetails;
            
            return job;
        }

        private SendEmailWorker PrepareSendEmailWorkerSut()
        {
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
