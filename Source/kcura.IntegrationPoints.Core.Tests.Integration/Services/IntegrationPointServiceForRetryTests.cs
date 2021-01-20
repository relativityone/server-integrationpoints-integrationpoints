using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Parallelizable(ParallelScope.None)]
	public class IntegrationPointServiceForRetryTests : RelativityProviderTemplate
	{
		private const int _ADMIN_USER_ID = 9;
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryService _jobHistoryService;

		public IntegrationPointServiceForRetryTests() : base("IntegrationPointService Retry Source", "IntegrationPointService Retry Destination")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();

			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
		}

		[SetUp]
		public void Setup()
		{
			Import.ImportNewDocuments(SourceWorkspaceArtifactID, Import.GetImportTable("IPTestDocument", 3));
		}

		[TearDown]
		public void TearDown()
		{
			DocumentService.DeleteAllDocuments(SourceWorkspaceArtifactID);
			DocumentService.DeleteAllDocuments(TargetWorkspaceArtifactID);
		}
	
		[IdentifiedTest("8a1efb36-117e-4c96-814d-537209d04314")]
		[Parallelizable(ParallelScope.None)]
		public async Task RetryIntegrationPoint_GoldFlow()
		{
			//Arrange
			const string errorFieldName = "Field with errors";
			int sourceErrorFieldArtifactId = 0, destinationErrorFieldArtifactId = 0;
			using (IFieldManager fm = Helper.CreateProxy<IFieldManager>())
			{
				sourceErrorFieldArtifactId = await fm.CreateFixedLengthFieldAsync(SourceWorkspaceArtifactID,
					CreateFixedLengthFieldRequest(errorFieldName, 5)).ConfigureAwait(false);

				destinationErrorFieldArtifactId = await fm.CreateFixedLengthFieldAsync(TargetWorkspaceArtifactID,
					CreateFixedLengthFieldRequest(errorFieldName, 4)).ConfigureAwait(false);
			}

			FieldMap[] importErrorFieldMap = GetErrorFieldMap(errorFieldName, errorFieldName, sourceErrorFieldArtifactId.ToString()).ToArray();
			FieldMap[] pushErrorFieldMap = GetErrorFieldMap(errorFieldName, sourceErrorFieldArtifactId.ToString(), destinationErrorFieldArtifactId.ToString()).ToArray();

			// Import documents
			const string documentPrefix = "RetryDocs";
			const int numberOfDocumentsToRetry = 10;
			DataTable sourceImportTable = Import.GetImportTable(documentPrefix, numberOfDocumentsToRetry);
			sourceImportTable.Columns.Add(errorFieldName, typeof(string));
			foreach (DataRow dataRow in sourceImportTable.Rows)
			{
				dataRow[errorFieldName] = "12345";
			}

			Import.ImportNewDocuments(SourceWorkspaceArtifactID, sourceImportTable, importErrorFieldMap);
			Import.ImportNewDocuments(TargetWorkspaceArtifactID, Import.GetImportTable(documentPrefix, 2));

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOverlay),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"RetryIntegrationPoint_GoldFlow {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Append/Overlay",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = Serializer.Serialize(Serializer.Deserialize<FieldMap[]>(Serializer.Serialize(GetDefaultFieldMap())).Concat(pushErrorFieldMap)),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act

			//Create Errors
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);

			IntegrationPointModel integrationPointWithErrors =
				_integrationPointService.ReadIntegrationPointModel(integrationPoint.ArtifactID);

			// Fix errors
			sourceImportTable = Import.GetImportTable(documentPrefix, numberOfDocumentsToRetry);
			sourceImportTable.Columns.Add(errorFieldName);
			foreach (DataRow dataRow in sourceImportTable.Rows)
			{
				dataRow[errorFieldName] = "123";
			}

			Import.ImportNewDocuments(SourceWorkspaceArtifactID, sourceImportTable, importErrorFieldMap);

			//Retry Errors
			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, integrationPoint.ArtifactID);
			IntegrationPointModel integrationPointPostRetry = _integrationPointService.ReadIntegrationPointModel(integrationPoint.ArtifactID);

			IJobHistoryRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactID);
			IList<int> jobHistoryArtifactIds = new List<int> { jobHistoryErrorRepository.GetLastJobHistoryArtifactId(integrationPointPostRetry.ArtifactID) };
			Data.JobHistory jobHistory = _jobHistoryService.GetJobHistory(jobHistoryArtifactIds)[0];

			//Assert
			Assert.AreEqual(true, integrationPointWithErrors.HasErrors, "The first integration point run should have errors");
			Assert.AreEqual(false, integrationPointPostRetry.HasErrors, "The integration point post retry should not have errors");
			Assert.AreEqual(numberOfDocumentsToRetry, jobHistory.ItemsTransferred);
			Assert.AreEqual(0, jobHistory.ItemsWithErrors);
			Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, jobHistory.JobStatus.Name);
			Assert.AreEqual(JobTypeChoices.JobHistoryRetryErrors.Name, jobHistory.JobType.Name);
		}

		private IEnumerable<FieldMap> GetErrorFieldMap(string fieldWithErrors, string sourceErrorFieldIdentifier, string destinationErrorFieldIdentifier)
		{
			return new FieldMap[]
			{
				new FieldMap
				{
					FieldMapType = FieldMapTypeEnum.None,
					SourceField = new global::Relativity.IntegrationPoints.Contracts.Models.FieldEntry
					{
						DisplayName = fieldWithErrors,
						FieldIdentifier = sourceErrorFieldIdentifier,
					},
					DestinationField = new FieldEntry
					{
						DisplayName = fieldWithErrors,
						FieldIdentifier = destinationErrorFieldIdentifier,
					},
				},
			};
		}

		private FixedLengthFieldRequest CreateFixedLengthFieldRequest(string fieldName, int length)
		{
			return new FixedLengthFieldRequest()
			{
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
				Name = fieldName,
				Length = length
			};
		}
	}
}
