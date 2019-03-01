using System;
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
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class DestinationWorkspaceTagsCreationTests : RelativityProviderTemplate
	{
		private DestinationWorkspaceTagsCreation _instance;
		private DestinationWorkspaceTagsCreationConfigurationStub _configuration;
		private JobHistory _jobHistory;
		private int _sourceWorkspaceArtifactTypeId;
		private int _sourceJobArtifactTypeId;

		public DestinationWorkspaceTagsCreationTests() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();

			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For<IHelper>().Instance(Helper));

			CreateJobHistory();

			CreateObjectTypes(container).ConfigureAwait(false).GetAwaiter().GetResult();

			_instance = new DestinationWorkspaceTagsCreation(container);

			_configuration = new DestinationWorkspaceTagsCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = TargetWorkspaceArtifactId,
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactId,
				JobArtifactId = _jobHistory.ArtifactId,
				SourceWorkspaceArtifactTypeId = _sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = _sourceJobArtifactTypeId
			};
		}

		private void CreateJobHistory()
		{
			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay, "name", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);

			_jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointModel.ArtifactID, Guid.NewGuid(), JobTypeChoices.JobHistoryRun);
		}

		private async Task CreateObjectTypes(IWindsorContainer container)
		{
			DestinationWorkspaceObjectTypesCreation objectTypesCreation = new DestinationWorkspaceObjectTypesCreation(container);
			DestinationWorkspaceObjectTypesCreationConfigurationStub configuration = new DestinationWorkspaceObjectTypesCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = TargetWorkspaceArtifactId
			};
			await objectTypesCreation.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			_sourceWorkspaceArtifactTypeId = configuration.SourceWorkspaceArtifactTypeId;
			_sourceJobArtifactTypeId = configuration.SourceJobArtifactTypeId;
		}

		[Test]
		public async Task ItShouldCreateWorkspaceAndJobTags()
		{
			// ACT
			await _instance.ExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.IsTrue(_configuration.IsSourceJobTagSet);
			Assert.IsTrue(_configuration.IsSourceWorkspaceTagSet);

			await AssertTags(_configuration.SourceWorkspaceTagArtifactId.Value, _configuration.SourceJobTagArtifactId.Value).ConfigureAwait(false);
		}

		private async Task AssertTags(int sourceWorkspaceTag, int sourceJobTag)
		{
			using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				ReadRequest sourceWorkspaceTagRequest = new ReadRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = sourceWorkspaceTag
					}
				};
				ReadResult sourceWorkspaceTagResult = await objectManager.ReadAsync(TargetWorkspaceArtifactId, sourceWorkspaceTagRequest).ConfigureAwait(false);
				Assert.IsNotNull(sourceWorkspaceTagResult);

				ReadRequest sourceJobTagRequest = new ReadRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = sourceJobTag
					}
				};
				ReadResult sourceJobTagResult = await objectManager.ReadAsync(TargetWorkspaceArtifactId, sourceJobTagRequest).ConfigureAwait(false);
				Assert.IsNotNull(sourceJobTagResult);
			}
		}

		[Test]
		public async Task ItShouldUpdateWorkspaceTagAndCreateJobTagIfTagsExist()
		{
			var configuration = new DestinationWorkspaceTagsCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = TargetWorkspaceArtifactId,
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactId,
				JobArtifactId = _jobHistory.ArtifactId,
				SourceWorkspaceArtifactTypeId = _sourceWorkspaceArtifactTypeId,
				SourceJobArtifactTypeId = _sourceJobArtifactTypeId
			};
			await _instance.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ACT
			await _instance.ExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(_configuration.SourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
			Assert.AreNotEqual(_configuration.SourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
		}

		[Test]
		public async Task ItShouldNotExecuteIfObjectTypesAreSet()
		{
			Mock<IDestinationWorkspaceTagsCreationConfiguration> configuration = new Mock<IDestinationWorkspaceTagsCreationConfiguration>();
			configuration.Setup(x => x.IsSourceJobTagSet).Returns(true);
			configuration.Setup(x => x.IsSourceWorkspaceTagSet).Returns(true);

			// ACT
			bool result = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.IsFalse(result);
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public async Task ItShouldExecuteIfObjectTypesAreMissing(bool isSourceJobTagSet, bool isSourceWorkspaceTagSet)
		{
			Mock<IDestinationWorkspaceTagsCreationConfiguration> configuration = new Mock<IDestinationWorkspaceTagsCreationConfiguration>();
			configuration.Setup(x => x.IsSourceJobTagSet).Returns(isSourceJobTagSet);
			configuration.Setup(x => x.IsSourceWorkspaceTagSet).Returns(isSourceWorkspaceTagSet);

			// ACT

			bool result = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.IsTrue(result);
		}
	}
}