using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
    [IdentifiedTestFixture("C4243B32-D40D-4922-B45A-9A0276393CAE")]
    [TestLevel.L1]
    public class SendEmailWorkerTests : TestsBase
    {
        private const int _SMTP_PORT = 25;
        private const string _SMTP_IS_SSL_ENABLED = "false";
        private const string _EMAIL_FROM_ADDRESS = "rip.developer@relativity.com";
        private const string _EMAIL_TO_ADDRESS = "relativity.admin@kcura.com";
        private const string _EMAIL_SUBJECT = "Test";
        private const string _EMAIL_BODY = "Integrations";

        private SendEmailWorker _sut;

        public override void SetUp()
        {
            base.SetUp();

            _sut = Container.Resolve<SendEmailWorker>(); ;
        }

        [IdentifiedTestCase("68499997-0835-41A2-AE72-7871C0E5F1FC")]
        public async Task ShouldSendEmailToSmtpServer()
        {
            // arrange
            int integrationPointArtifactID = CreateDummyIntegrationPoint();

            using (FakeSmtpServer fakeSmtpServer = FakeSmtpServer.Start(_SMTP_PORT))
            {
                // act
                AddSendingEmailJobToQueue(integrationPointArtifactID, TaskType.SendEmailWorker);

                // assert
                FakeSmtpMessage receivedMessage = await fakeSmtpServer.GetFirstMessage(_emailReceivedTimeout)
                    .ConfigureAwait(false);

                AssertReceivedMessage(receivedMessage);
            }
        }

    }
}
