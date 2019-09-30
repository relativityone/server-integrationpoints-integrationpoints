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
using Relativity.Testing.Identification;
using InstanceSetting = Relativity.Services.InstanceSetting.InstanceSetting;

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
		/// </summary>
		/// <returns></returns>
		[IdentifiedTest("034276d0-b7a3-4d79-903c-271a0c19f3a0")]
		[ConnectivityToTestRunnerRequiredTest]
		public async Task ShouldSendEmailToSmtpServer()
		{
			// arrange
			await EnsureAgentIsEnabledAsync().ConfigureAwait(false);
			int integrationPointArtifactId = CreateDummyIntegrationPoint();

			using (FakeSmtpServer fakeSmtpServer = FakeSmtpServer.Start(_SMTP_PORT))
			{
				// act
				AddSendingEmailJobToQueue(integrationPointArtifactId);

				// assert
				FakeSmtpMessage receivedMessage = await fakeSmtpServer.GetFirstMessage(_emailReceivedTimeout)
					.ConfigureAwait(false);

				receivedMessage.Should().NotBeNull();
				receivedMessage.FromAddress.Should().Be(_EMAIL_FROM_ADDRESS);
				receivedMessage.ToAddresses.Should().ContainSingle()
					.Which.Should().Be(_EMAIL_TO_ADDRESS);
				receivedMessage.Subject.Should().Be(_EMAIL_SUBJECT);
				receivedMessage.Data.Should().Contain(_EMAIL_BODY);
			}
		}

		private int CreateDummyIntegrationPoint([CallerMemberName] string testName = "")
		{
			var destinationConfiguration = new ImportSettings
			{
				CaseArtifactId = WorkspaceArtifactId
			};
			var integrationPoint = new Data.IntegrationPoint
			{
				Name = $"{nameof(SendEmailWorkerTests)}-{testName}",
				SourceProvider = SourceProviders.First().ArtifactId,
				DestinationProvider = RelativityDestinationProviderArtifactId,
				DestinationConfiguration = Newtonsoft.Json.JsonConvert.SerializeObject(destinationConfiguration),
				OverwriteFields = OverwriteFieldsChoices.IntegrationPointAppendOnly
			};

			int integrationPointArtifactId = ObjectManager.Create(integrationPoint);
			return integrationPointArtifactId;
		}

		private static async Task EnsureAgentIsEnabledAsync()
		{
			await IntegrationPoint.Tests.Core.Agent.CreateIntegrationPointAgentIfNotExistsAsync().ConfigureAwait(false);
			await IntegrationPoint.Tests.Core.Agent.EnableAllIntegrationPointsAgentsAsync().ConfigureAwait(false);
		}

		private void AddSendingEmailJobToQueue(int integrationPointArtifactId)
		{
			var message = new EmailMessage
			{
				Subject = _EMAIL_SUBJECT,
				MessageBody = _EMAIL_BODY,
				Emails = new[] { _EMAIL_TO_ADDRESS }
			};

			_jobService.CreateJob(
				workspaceID: WorkspaceArtifactId,
				relatedObjectArtifactID: integrationPointArtifactId,
				taskType: TaskType.SendEmailWorker.ToString(),
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
			var query = new Query
			{
				Condition = $"'Section' == 'kCura.Notification' AND 'Name' == '{settingName}'"
			};
			InstanceSettingQueryResultSet result = await _instanceSettingsManager.QueryAsync(query).ConfigureAwait(false);
			return result.Results.Single().Artifact;
		}
	}
}
