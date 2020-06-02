using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Agent.Tests.Integration.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using kCura.ScheduleQueue.Core.Core;
using Relativity.Testing.Identification;
using InstanceSetting = Relativity.Services.InstanceSetting.InstanceSetting;
using kCura.IntegrationPoint.Tests.Core.TestCategories;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class SendEmailWorkerTests : SourceProviderTemplate
	{
		private IInstanceSettingManager _instanceSettingsManager;
		private IJobService _jobService;

		private const int _SMTP_PORT = 25;
		private const string _SMTP_IS_SSL_ENABLED = "false";
		private const string _EMAIL_FROM_ADDRESS = "rip.developer@relativity.com";
		private const string _EMAIL_TO_ADDRESS = "relativity.admin@kcura.com";
		private const string _EMAIL_SUBJECT = "Test";
		private const string _EMAIL_BODY = "Integrations";

		private readonly TimeSpan _emailReceivedTimeout = TimeSpan.FromSeconds(120);

		public SendEmailWorkerTests() : base(nameof(SendEmailWorkerTests))
		{
			CreatingAgentEnabled = false;
			CreatingWorkspaceEnabled = true;
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			_jobService = Container.Resolve<IJobService>();
			_instanceSettingsManager = Helper.CreateProxy<IInstanceSettingManager>();
			string localComputerHostname = Dns.GetHostName();

			await SetNotificationInstanceSettings(localComputerHostname).ConfigureAwait(false);
		}

		/// <summary>
		/// This test inserts <see cref="TaskType.SendEmailWorker"/> job to schedule queue
		/// and verifies that agent sends email using configured SMTP server.
		/// This tests checks backwards compatibility after changing the type
		/// of serialized jobDetails from EmailJobParameters to TaskParameters
		/// with EmailJobParameters inside it. JIRA: REL-354651
		/// </summary>
		/// <returns></returns>
		[IdentifiedTest("034276d0-b7a3-4d79-903c-271a0c19f3a0")]
		[ConnectivityToTestRunnerRequiredTest]
		public async Task ShouldSendEmailToSmtpServer()
		{
			// arrange
			await EnsureAgentIsEnabledAsync().ConfigureAwait(false);
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

		[IdentifiedTest("97170FB5-91DC-4317-8FCA-443CFAFFC014")]
		[ConnectivityToTestRunnerRequiredTest]
		public async Task ShouldSendEmailToSmtpServerWithJobTypeAsSendEmailManager()
		{
			// arrange
			await EnsureAgentIsEnabledAsync().ConfigureAwait(false);
			int integrationPointArtifactID = CreateDummyIntegrationPoint();

			FakeSmtpMessage receivedMessage;
			using (FakeSmtpServer fakeSmtpServer = FakeSmtpServer.Start(_SMTP_PORT))
			{
				// act
				AddSendingEmailJobToQueue(integrationPointArtifactID, TaskType.SendEmailManager);

				// assert
				receivedMessage = await fakeSmtpServer.GetFirstMessage(_emailReceivedTimeout).ConfigureAwait(false);

			}
			AssertReceivedMessage(receivedMessage);
		}

		[IdentifiedTest("21C7BC9A-5291-4B69-9F7C-A3E3A1ED609A")]
		[ConnectivityToTestRunnerRequiredTest]
		public async Task ShouldSendEmailWithBatchInstanceIDToSmtpServer()
		{
			// arrange
			await EnsureAgentIsEnabledAsync().ConfigureAwait(false);
			int integrationPointArtifactID = CreateDummyIntegrationPoint();

			FakeSmtpMessage receivedMessage;
			using (FakeSmtpServer fakeSmtpServer = FakeSmtpServer.Start(_SMTP_PORT))
			{
				// act
				AddSendingEmailWithBatchInstanceIDJobToQueue(integrationPointArtifactID);

				// assert
				receivedMessage = await fakeSmtpServer.GetFirstMessage(_emailReceivedTimeout).ConfigureAwait(false);

			}
			AssertReceivedMessage(receivedMessage);

		}

		private void AddSendingEmailWithBatchInstanceIDJobToQueue(int integrationPointArtifactID)
		{
			EmailJobParameters message = new EmailJobParameters
			{
				Subject = _EMAIL_SUBJECT,
				MessageBody = _EMAIL_BODY,
				Emails = new[] { _EMAIL_TO_ADDRESS }
			};

			TaskParameters taskParameters = new TaskParameters
			{
				BatchInstance = Guid.NewGuid(),
				BatchParameters = message
			};

			_jobService.CreateJob(
				workspaceID: WorkspaceArtifactId,
				relatedObjectArtifactID: integrationPointArtifactID,
				taskType: TaskType.SendEmailWorker.ToString(),
				nextRunTime: DateTime.UtcNow,
				jobDetails: Newtonsoft.Json.JsonConvert.SerializeObject(taskParameters),
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

		private int CreateDummyIntegrationPoint([CallerMemberName] string testName = "")
		{
			ImportSettings destinationConfiguration = new ImportSettings
			{
				CaseArtifactId = WorkspaceArtifactId
			};
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint
			{
				Name = $"{nameof(SendEmailWorkerTests)}-{testName}",
				SourceProvider = SourceProviders.First().ArtifactId,
				DestinationProvider = RelativityDestinationProviderArtifactId,
				DestinationConfiguration = Newtonsoft.Json.JsonConvert.SerializeObject(destinationConfiguration),
				OverwriteFields = OverwriteFieldsChoices.IntegrationPointAppendOnly
			};

			int integrationPointArtifactID = ObjectManager.Create(integrationPoint);
			return integrationPointArtifactID;
		}

		private static async Task EnsureAgentIsEnabledAsync()
		{
			await IntegrationPoint.Tests.Core.Agent.CreateIntegrationPointAgentIfNotExistsAsync().ConfigureAwait(false);
			await IntegrationPoint.Tests.Core.Agent.EnableAllIntegrationPointsAgentsAsync().ConfigureAwait(false);
		}

		private void AddSendingEmailJobToQueue(int integrationPointArtifactID, TaskType taskType)
		{
			EmailJobParameters message = new EmailJobParameters
			{
				Subject = _EMAIL_SUBJECT,
				MessageBody = _EMAIL_BODY,
				Emails = new[] { _EMAIL_TO_ADDRESS }
			};

			_jobService.CreateJob(
				workspaceID: WorkspaceArtifactId,
				relatedObjectArtifactID: integrationPointArtifactID,
				taskType: taskType.ToString(),
				nextRunTime: DateTime.UtcNow,
				jobDetails: Newtonsoft.Json.JsonConvert.SerializeObject(message),
				SubmittedBy: 9,
				rootJobID: null,
				parentJobID: null
			);
		}

		private async Task SetNotificationInstanceSettings(string localComputerHostname)
		{
			await UpdateNotificationInstanceSettings("SMTPPort", _SMTP_PORT.ToString()).ConfigureAwait(false);
			await UpdateNotificationInstanceSettings("SMTPServer", localComputerHostname).ConfigureAwait(false);
			await UpdateNotificationInstanceSettings("SMTPSSLisRequired", _SMTP_IS_SSL_ENABLED).ConfigureAwait(false);
			await UpdateNotificationInstanceSettings("EmailFrom", _EMAIL_FROM_ADDRESS).ConfigureAwait(false);
		}

		private async Task UpdateNotificationInstanceSettings(string settingName, string value)
		{
			InstanceSetting initialPassword = await GetNotificationInstanceSettings(settingName).ConfigureAwait(false);

			initialPassword.Value = value;
			await _instanceSettingsManager.UpdateSingleAsync(initialPassword).ConfigureAwait(false);
		}

		private async Task<InstanceSetting> GetNotificationInstanceSettings(string settingName)
		{
			Query query = new Query
			{
				Condition = $"'Section' == 'kCura.Notification' AND 'Name' == '{settingName}'"
			};
			InstanceSettingQueryResultSet result = await _instanceSettingsManager.QueryAsync(query).ConfigureAwait(false);
			return result.Results.Single().Artifact;
		}
	}
}
