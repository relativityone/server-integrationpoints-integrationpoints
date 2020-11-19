using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Productions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using IntegrationPointModel = kCura.IntegrationPoints.Core.Models.IntegrationPointModel;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Parallelizable(ParallelScope.None)]
	public class IntegrationPointServiceForPushProductionTests : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryService _jobHistoryService;

		private const int _ADMIN_USER_ID = 9;

		public IntegrationPointServiceForPushProductionTests() : base("IntegrationPointService Source", "IntegrationPointService Destination")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			Container.Resolve<IJobService>();

			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
		}

		[TearDown]
		public void TearDown()
		{
			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
		}

		[IdentifiedTest("df96a034-dc7d-4b20-84f1-1a66baeab551")]
		[SmokeTest]
		[Parallelizable(ParallelScope.None)]
		public Task CreateAndRunIntegrationPoint_PushProductions_GoldFlow()
		{
			//Arrange

			ProductionHelper productionHelper = new ProductionHelper(WorkspaceArtifactId);
			int productionId = productionHelper.CreateProductionSetAndImportData($"Production {DateTime.Now:yy-MM-dd HH-mm-ss}", DocumentTestDataBuilder.TestDataType.SmallWithFoldersStructure);

			ImportSettings destinationSettings = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			destinationSettings.ImageImport = true;
			destinationSettings.ImagePrecedence = Array.Empty<ProductionDTO>();

			SourceConfiguration sourceConfiguration = new SourceConfiguration()
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
				TargetWorkspaceArtifactId = TargetWorkspaceArtifactID,
				TypeOfExport = SourceConfiguration.ExportType.ProductionSet,
				SourceProductionId = productionId
			};

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = Serializer.Serialize(destinationSettings),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = Serializer.Serialize(sourceConfiguration),
				LogErrors = true,
				Name = $"CreateAndRunIntegrationPoint_PushProduction_GoldFlow {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);
			IntegrationPointModel integrationPointPostJob = _integrationPointService.ReadIntegrationPointModel(integrationPoint.ArtifactID);
			IJobHistoryRepository jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactID);
			int jobHistoryArtifactId = jobHistoryRepository.GetLastJobHistoryArtifactId(integrationPointPostJob.ArtifactID);
			Console.WriteLine($"Job History Artifact ID: {jobHistoryArtifactId}");
			Data.JobHistory jobHistory = _jobHistoryService.GetJobHistory(new List<int>() { jobHistoryArtifactId })[0];

			//Assert
			Assert.AreEqual(false, integrationPointPostJob.HasErrors);
			Assert.IsNotNull(integrationPointPostJob.LastRun);
			Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, jobHistory.JobStatus.Name);
			Assert.AreEqual(JobTypeChoices.JobHistoryRun.Name, jobHistory.JobType.Name);
			return AssertAllDocumentsHaveImagesAsync(TargetWorkspaceArtifactID, 3);
		}

		private async Task AssertAllDocumentsHaveImagesAsync(int workspaceID, int expectedNumberOfDocuments)
		{
			using (var objectManager = Helper.CreateProxy<IObjectManager>())
			{
				const string relativityImageCountFieldName = "Relativity Image Count";
				QueryResult queryResult = await objectManager.QueryAsync(workspaceID, new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.Document
					},
					Fields = new[]
					{
						new FieldRef()
						{
							Name = relativityImageCountFieldName
						}
					}
				}, start: 0, length: int.MaxValue).ConfigureAwait(false);

				queryResult.Objects.Count.Should().Be(expectedNumberOfDocuments);

				foreach (RelativityObject document in queryResult.Objects)
				{
					document.FieldValuePairExists(relativityImageCountFieldName).Should().BeTrue();
					FieldValuePair relativityImageCountField =
						document.FieldValues.Single(x => x.Field.Name == relativityImageCountFieldName);
					relativityImageCountField
						.Value.Should().BeAssignableTo<int>()
						.Which.Should().BeGreaterThan(0);
				}
			}
		}
	}
}