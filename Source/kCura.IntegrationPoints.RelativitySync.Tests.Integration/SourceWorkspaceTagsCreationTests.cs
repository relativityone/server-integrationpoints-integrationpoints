using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using kCura.IntegrationPoints.RelativitySync.Tests.Integration.Stubs;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class SourceWorkspaceTagsCreationTests : RelativityProviderTemplate
	{
		private SourceWorkspaceTagsCreation _instance;

		private JobHistory _jobHistory;
		private SourceWorkspaceTagsCreationConfigurationStub _configuration;

		public SourceWorkspaceTagsCreationTests() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();

			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For<IHelper>().Instance(Helper));
			container.Register(Component.For<IAPILog>().Instance(new Mock<IAPILog>().Object));

			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay, "name", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);

			_jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointModel.ArtifactID, Guid.NewGuid(), JobTypeChoices.JobHistoryRun);

			_instance = new SourceWorkspaceTagsCreation(container);

			_configuration = new SourceWorkspaceTagsCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = TargetWorkspaceArtifactId,
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactId,
				JobArtifactId = _jobHistory.ArtifactId
			};
		}

		[Test]
		public async Task ItShouldCreateWorkspaceTag()
		{
			// ACT
			await _instance.ExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.IsTrue(_configuration.IsDestinationWorkspaceTagArtifactIdSet);

			await AssertTag(_configuration.DestinationWorkspaceTagArtifactId).ConfigureAwait(false);
		}

		private async Task AssertTag(int tagArtifactId)
		{
			using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				ReadRequest request = new ReadRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = tagArtifactId
					},
					Fields = new[]
					{
						new FieldRef
						{
							Name = "Job History"
						}
					}
				};

				ReadResult result = await objectManager.ReadAsync(SourceWorkspaceArtifactId, request).ConfigureAwait(false);
				Assert.IsNotNull(result);

				IList<RelativityObjectValue> jobHistory = (IList<RelativityObjectValue>) result.Object["Job History"].Value;
				Assert.AreEqual(jobHistory[0].ArtifactID, _jobHistory.ArtifactId);
			}
		}

		[Test]
		public async Task ItShouldUpdateExistingTag()
		{
			var configuration = new SourceWorkspaceTagsCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = TargetWorkspaceArtifactId,
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactId,
				JobArtifactId = _jobHistory.ArtifactId
			};
			await _instance.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ACT
			await _instance.ExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(configuration.DestinationWorkspaceTagArtifactId, _configuration.DestinationWorkspaceTagArtifactId);
		}
	}
}