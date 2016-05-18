using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Services;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using Newtonsoft.Json;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration
{
	using Core.Models;
	using Core.Services;
	using Core.Services.JobHistory;
	using Core.Services.ServiceContext;
	using Data;
	using Data.Factories;
	using Data.Repositories;
	using global::Relativity.API;
	using global::Relativity.Services.ObjectQuery;
	using NSubstitute;

	public class ViewErrors : WorkspaceDependentTemplate
	{
		private IRepositoryFactory _repositoryFactory;
		private IJobHistoryRepository _jobHistoryRepository;
		private IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private IIntegrationPointService _integrationPointService;
		private IJobHistoryService _jobHistoryService;
		private ICaseServiceContext _caseServiceContext;



		public ViewErrors() : base("ViewErrorsSource", "ViewErrorsSourceDestination")
		{
		}

		[Test]
		[Explicit]
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
				Destination = CreateDefaultDestinationConfig(),
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
			IntegrationPoint integrationPointDto = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointCreated.ArtifactID);
			_jobHistoryService.CreateRdo(integrationPointDto, Guid.NewGuid(), DateTime.Now);
			//Act

			_integrationPointService.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPointCreated.ArtifactID, 9);
			GetErrorsFromView(integrationPointCreated.ArtifactID);
			

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
			Helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser).Returns(Helper.CreateUserObjectQueryManager());
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

		private void GetErrorsFromView(int integrationPointArtifactId)
		{
			Selenium.LogIntoRelativity(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword);
			Selenium.GoToWorkspace(1062408);
			Selenium.GoToTab("Integration Points");
			Selenium.GoToIntegrationPoint(integrationPointArtifactId);
		}
	}
}
