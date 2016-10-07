using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration
{
	[Category(kCura.IntegrationPoint.Tests.Core.Constants.INTEGRATION_CATEGORY)]
	[TestFixture]
	[Ignore("Tests need refactor")]
	public class ViewErrors : RelativityProviderTemplate
	{
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryRepository _jobHistoryRepository;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IIntegrationPointService _integrationPointService;
		private IJobHistoryService _jobHistoryService;
		private ICaseServiceContext _caseServiceContext;
		private IObjectTypeRepository _objectTypeRepository;

		private IWebDriver _driver;

		public ViewErrors() : base("ViewErrorsSource", "ViewErrorsSourceDestination")
		{
		}

		[Test]
		[Ignore("Test needs refactor")]
		public void ExpectDisabledViewErrorsLinkOnIntegrationPointCreation()
		{
			//Arrange
			//UserModel user = new UserModel
			//{
			//	EmailAddress = SharedVariables.RelativityUserName,
			//	Password = SharedVariables.RelativityPassword
			//};

			//string response = kCura.IntegrationPoint.Tests.Core.IntegrationPoint.CreateIntegrationPoint(Guid.NewGuid().ToString(),
			//	SourceWorkspaceArtifactId,
			//	TargetWorkspaceArtifactId,
			//	SavedSearchArtifactId,
			//	FieldOverlayBehavior.UseFieldSettings,
			//	ImportOverwriteMode.AppendOverlay,
			//	false,
			//	false,
			//	user);
			_driver = new ChromeDriver();

			Import.ImportNewDocuments(SourceWorkspaceArtifactId, GetImportTable());
			ResolveServices();
			//DestinationConfiguration destinationConfiguration = new DestinationConfiguration
			//{
			//	ArtifactTypeId = 10,
			//	CaseArtifactId = SourceWorkspaceArtifactId,
			//	Provider = "Relativity",
			//	ImportOverwriteMode = "Append",
			//	ImportNativeFile = false,
			//	UseFolderPathInformation = false,
			//	FieldOverlayBehavior = "Use Field Settings"
			//};

			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = false,
				Name = "View Errors" + DateTime.Now,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};

			//IntegrationPointModel integrationPoint = JsonConvert.DeserializeObject<IntegrationPointModel>(response);
			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Data.IntegrationPoint integrationPointDto = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointCreated.ArtifactID);
			_jobHistoryService.GetOrCreateScheduledRunHistoryRdo(integrationPointDto, Guid.NewGuid(), DateTime.Now);
			//Act

			//_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
			GetErrorsFromView(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID);

			//Assert
			//Assert.IsTrue(ranIntegrationPoint);
		}

		private string CreateSourceConfig(int savedSearchId)
		{
			return $"{{\"SavedSearchArtifactId\":{savedSearchId},\"SourceWorkspaceArtifactId\":\"{SourceWorkspaceArtifactId}\",\"TargetWorkspaceArtifactId\":{TargetWorkspaceArtifactId}}}";
		}

		private void ResolveServices()
		{
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(SourceWorkspaceArtifactId);
			_jobHistoryErrorRepository = _repositoryFactory.GetJobHistoryErrorRepository(SourceWorkspaceArtifactId);
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();
			_objectTypeRepository = Container.Resolve<IObjectTypeRepository>();
		}

		private DataTable GetImportTable()
		{
			DataTable table = new DataTable();
			table.Columns.Add("Control Number", typeof(string));
			//table.Columns.Add("NATIVE_FILE_PATH_001", typeof(string));
			//table.Columns.Add("Parent Document ID", typeof(string));
			table.Rows.Add("Doc1");//, "C:\\important3.txt");//, "");
			table.Rows.Add("Doc2");//, "C:\\important4.txt");//, "");
								   // table.Rows.Add("Doc2", "C:\\addressomg.txt", "Doc2");
			return table;
		}

		private void GetErrorsFromView(int workspaceArtifactId, int integrationPointArtifactId)
		{
			_driver.LogIntoRelativity(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			_driver.GoToWorkspace(workspaceArtifactId);

			int artifactTypeId = _objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));

			_driver.GoToObjectInstance(workspaceArtifactId, integrationPointArtifactId, artifactTypeId);
		}
	}
}