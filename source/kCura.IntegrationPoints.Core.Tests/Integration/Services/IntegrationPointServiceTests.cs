using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Category("Integration Tests")]
	public class IntegrationPointServiceTests : WorkspaceDependentTemplate
	{
		private const string _SOURCECONFIG = "Source Config";
		private const string _NAME = "Name";
		private const string _FIELDMAP = "Map";
		private DestinationProvider _destinationProvider;
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryService _jobHistoryService;
		private const int _ADMIN_USER_ID = 9;

		public IntegrationPointServiceTests()
			: base("IntegrationPointService Source", null)
		{
		}

		[TestFixtureSetUp]
		public override void SetUp()
		{
			base.SetUp();
			_destinationProvider = CaseContext.RsapiService.DestinationProviderLibrary.ReadAll().First();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
		}

		#region UpdateProperties

		[Test]
		public void SaveIntegration_UpdateNothing()
		{
			const string name = "Resaved Rip";
			IntegrationModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);
			IntegrationModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			ValidateModel(originalModel, newModel, new string[0]);
		}

		[Test]
		public void SaveIntegration_UpdateName_OnRanIp_ErrorCase()
		{
			const string name = "Update Name - OnRanIp";
			IntegrationModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Name = "newName";

			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel));
		}

		[Test]
		public void SaveIntegration_UpdateMap_OnRanIp()
		{
			const string name = "Update Map - OnRanIp";
			IntegrationModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Map = "Blahh";

			IntegrationModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);
			ValidateModel(originalModel, newModel, new string[] { _FIELDMAP });

			Audit audit = this.GetLastForIntegrationPoint(defaultModel.Name);
			Assert.AreEqual(SharedVariables.UserFullName, audit.UserFullName, "The user should be correct.");
			Assert.AreEqual("Update", audit.AuditAction, "The audit action should be correct.");
		}

		[Test]
		public void SaveIntegration_UpdateConfig_OnNewRip()
		{
			const string name = "Update Source Config - SavedSearch - OnNewRip";
			IntegrationModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			int newSavedSearch = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactId, name);
			defaultModel.SourceConfiguration = CreateSourceConfig(newSavedSearch, SourceWorkspaceArtifactId);

			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel), "Unable to save Integration Point: Source Configuration cannot be changed once the Integration Point has been run");
		}

		[Test]
		public void SaveIntegration_UpdateName_OnNewRip()
		{
			const string name = "Update Name - OnNewRip";
			IntegrationModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Name = name + " 2";

			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel), "Unable to save Integration Point: Name cannot be changed once the Integration Point has been run");
		}

		[Test]
		public void SaveIntegration_UpdateMap_OnNewRip()
		{
			const string name = "Update Map - OnNewRip";
			IntegrationModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Map = "New Map string";

			IntegrationModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			ValidateModel(originalModel, newModel, new[] { _FIELDMAP });

			Audit audit = this.GetLastForIntegrationPoint(defaultModel.Name);
			Assert.AreEqual(SharedVariables.UserFullName, audit.UserFullName, "The user should be correct.");
			Assert.AreEqual("Update", audit.AuditAction, "The audit action should be correct.");
		}

		#endregion

		[Test]
		public void CreateAndRunIntegrationPoint()
		{
			//Arrange
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable("RunNow",3));

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = GetDestinationConfigWithOverlayOnly(),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);
			IntegrationModel integrationPointPostJob = _integrationPointService.ReadIntegrationPoint(integrationPoint.ArtifactID);
			IJobHistoryRepository jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactId);
			IList<int> jobHistoryArtifactIds = new List<int> { jobHistoryErrorRepository.GetLastJobHistoryArtifactId(integrationPointPostJob.ArtifactID) };
			JobHistory jobHistory = _jobHistoryService.GetJobHistory(jobHistoryArtifactIds)[0];

			//Assert
			Assert.AreEqual(false, integrationPointPostJob.HasErrors);
			Assert.IsNotNull(integrationPointPostJob.LastRun);
			Assert.AreEqual(3, jobHistory.ItemsImported);

		}

		[Test]
		public void RetryIntegrationPointErrors()
		{
			//Arrange
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable("Retry", 3));

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act

			//Create Errors by using Append Only
			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPoint.ArtifactID);

			//Update Integration Point's SelectedOverWrite to "Overlay Only"
			IntegrationModel integrationPointPostRun = _integrationPointService.ReadIntegrationPoint(integrationPoint.ArtifactID);
			integrationPointPostRun.SelectedOverwrite = "Overlay Only";
			integrationPointPostRun.Destination = GetDestinationConfigWithOverlayOnly();
			CreateOrUpdateIntegrationPoint(integrationPointPostRun);

			//Retry Errors
			_integrationPointService.RetryIntegrationPoint(SourceWorkspaceArtifactId, integrationPointPostRun.ArtifactID, _ADMIN_USER_ID);
			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointPostRun.ArtifactID);
			IntegrationModel integrationPointPostRetry = _integrationPointService.ReadIntegrationPoint(integrationPointPostRun.ArtifactID);

			//Assert
			Assert.AreEqual(true, integrationPointPostRun.HasErrors);
			Assert.AreEqual(false, integrationPointPostRetry.HasErrors);
		}

		[Test]
		public void CreateAndRunScheduledIntegrationPoint()
		{
			//Arrange
			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable("Scheduled", 3));

			DateTime utcNow = DateTime.UtcNow;

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = GetDestinationConfigWithOverlayOnly(),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPointServiceTest" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = utcNow.ToString("MM/dd/yyyy"),
					EndDate = utcNow.AddDays(1).ToString("MM/dd/yyyy"),
					ScheduledTime = utcNow.ToString("HH") + ":" + utcNow.AddMinutes(1).ToString("mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
				},
				Map = CreateDefaultFieldMap()
			};

			IntegrationModel integrationPointPreJobExecution = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act

			//Create Errors by using Append Only
			Status.WaitForScheduledJobToComplete(Container, SourceWorkspaceArtifactId, integrationPointPreJobExecution.ArtifactID, TaskType.ExportService.ToString());
			IntegrationModel integrationPointPostRun = _integrationPointService.ReadIntegrationPoint(integrationPointPreJobExecution.ArtifactID);

			//Assert
			Assert.AreEqual(null, integrationPointPreJobExecution.LastRun);
			Assert.AreEqual(false, integrationPointPreJobExecution.HasErrors);
			Assert.AreEqual(false, integrationPointPostRun.HasErrors);
			Assert.IsNotNull(integrationPointPostRun.LastRun);
			Assert.IsNotNull(integrationPointPostRun.NextRun);
		}

		private void ValidateModel(IntegrationModel expectedModel, IntegrationModel actual, string[] updatedProperties)
		{
			Action<object, object> assertion = DetermineAssertion(updatedProperties, _SOURCECONFIG);
			assertion(expectedModel.SourceConfiguration, actual.SourceConfiguration);

			assertion = DetermineAssertion(updatedProperties, _NAME);
			assertion(expectedModel.Name, actual.Name);

			assertion = DetermineAssertion(updatedProperties, _FIELDMAP);
			assertion(expectedModel.Map, actual.Map);

			Assert.AreEqual(expectedModel.HasErrors, actual.HasErrors);
			Assert.AreEqual(expectedModel.DestinationProvider, actual.DestinationProvider);
			Assert.AreEqual(expectedModel.LastRun.Value.Date, actual.LastRun.Value.Date);
			Assert.AreEqual(expectedModel.LastRun.Value.Hour, actual.LastRun.Value.Hour);
			Assert.AreEqual(expectedModel.LastRun.Value.Minute, actual.LastRun.Value.Minute);
		}

		private Action<object, object> DetermineAssertion(string[] updatedProperties, string property)
		{
			Action<object, object> assertion;
			if (updatedProperties.Contains(property))
			{
				assertion = Assert.AreNotEqual;
			}
			else
			{
				assertion = Assert.AreEqual;
			}
			return assertion;
		}

		private string CreateSourceConfig(int savedSearchId, int targetWorkspaceId)
		{
			return $"{{\"SavedSearchArtifactId\":{savedSearchId},\"SourceWorkspaceArtifactId\":\"{SourceWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{targetWorkspaceId}}}";
		}

		private IntegrationModel CreateIntegrationPointThatHasNotRun(string name)
		{
			return new IntegrationModel()
			{
				Destination = $"{{\"artifactTypeID\":10,\"CaseArtifactId\":{TargetWorkspaceArtifactId},\"Provider\":\"relativity\",\"DoNotUseFieldsMapCache\":true,\"ImportOverwriteMode\":\"AppendOnly\",\"importNativeFile\":\"false\",\"UseFolderPathInformation\":\"false\",\"ExtractedTextFieldContainsFilePath\":\"false\",\"ExtractedTextFileEncoding\":\"utf - 16\",\"CustodianManagerFieldContainsLink\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\"}}",
				DestinationProvider = _destinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateSourceConfig(SavedSearchArtifactId, TargetWorkspaceArtifactId),
				LogErrors = true,
				Name = $"${name} - {DateTime.Today}",
				Map = "[]",
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
			};
		}

		private IntegrationModel CreateIntegrationPointThatIsAlreadyRunModel(string name)
		{
			IntegrationModel model = CreateIntegrationPointThatHasNotRun(name);
			model.LastRun = DateTime.Now;
			return model;
		}

		private DataTable GetImportTable(string documentPrefix ,int numberOfDocuments)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));

			for (int index = 1; index <= numberOfDocuments; index++)
			{
				string controlNumber = $"{documentPrefix}{index}";
				table.Rows.Add(controlNumber);
			}
			return table;
		}

		private string GetDestinationConfigWithOverlayOnly()
		{
			ImportSettings destinationConfig = new ImportSettings
			{
				ArtifactTypeId = 10,
				CaseArtifactId = SourceWorkspaceArtifactId,
				Provider = "Relativity",
				ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly,
				ImportNativeFile = false,
				ExtractedTextFieldContainsFilePath = false,
				FieldOverlayBehavior = "Use Field Settings"
			};
			return Container.Resolve<ISerializer>().Serialize(destinationConfig);
		}
	}
}