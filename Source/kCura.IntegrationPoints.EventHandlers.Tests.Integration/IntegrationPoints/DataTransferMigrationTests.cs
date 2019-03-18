using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Choice = kCura.Relativity.Client.DTOs.Choice;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.IntegrationPoints
{
	[TestFixture]
	public class DataTransferMigrationTests : SourceProviderTemplate
	{
		private IDataTransferLocationMigration _dataTransferLocationMigration;

		private IAPILog _logger;
		private IDestinationProviderRepository _destinationProviderRepository;
		private ISourceProviderRepository _sourceProviderRepository;
		private IDataTransferLocationMigrationHelper _dataTransferLocationMigrationHelper;
		private IRelativityObjectManager _relativityObjectManager;
		private IDataTransferLocationService _dataTransferLocationService;
		private IResourcePoolManager _resourcePoolManager;
		private IEHHelper _ehHelper;
		private ISerializer _serializer;
		private IRepositoryFactory _repositoryFactory;
		private IChoiceQuery _choiceQuery;

		private int _savedIntegrationPointId;

		public DataTransferMigrationTests()
			: base($"DataTransferMigrationTests_{Utils.FormattedDateTimeNow}")
		{ }

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_logger = Substitute.For<IAPILog>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_serializer = new JSONSerializer();
			_dataTransferLocationMigrationHelper = new DataTransferLocationMigrationHelper(_serializer);
			_dataTransferLocationService = Container.Resolve<IDataTransferLocationService>();
			_ehHelper = new EHHelper(Helper, WorkspaceArtifactId);
			_relativityObjectManager = CaseContext.RsapiService.RelativityObjectManager;
			_choiceQuery = Container.Resolve<IChoiceQuery>();
			_resourcePoolManager = Substitute.For<IResourcePoolManager>();

			_resourcePoolManager.GetProcessingSourceLocation(WorkspaceArtifactId).Returns(CreateSampleProcessingSourceLocations());
		}

		public override void TestSetup()
		{
			base.TestSetup();

			_destinationProviderRepository = _repositoryFactory.GetDestinationProviderRepository(WorkspaceArtifactId);
			_sourceProviderRepository = _repositoryFactory.GetSourceProviderRepository(WorkspaceArtifactId);
			_relativityObjectManager = CaseContext.RsapiService.RelativityObjectManager;

			_dataTransferLocationMigration = CreteDataTransferLocationMigration(_logger, _destinationProviderRepository,
				_sourceProviderRepository, _dataTransferLocationMigrationHelper, CaseContext, _relativityObjectManager,
				_dataTransferLocationService, _resourcePoolManager, _ehHelper);
		}

		public override void TestTeardown()
		{
			base.TestTeardown();
			_relativityObjectManager.Delete(_savedIntegrationPointId);
		}

		[Test]
		public void ItShouldMigrateIntegrationPoint()
		{
			Data.IntegrationPoint integrationPoint = CreateIntegrationPoint();
			_savedIntegrationPointId = SaveIntegrationPoint(integrationPoint);

			_dataTransferLocationMigration.Migrate();

			Data.IntegrationPoint integrationPointAfterMigration = _relativityObjectManager.Read<Data.IntegrationPoint>(_savedIntegrationPointId);
			Dictionary<string, object> deserializedSourceConfigurationAfterMigration =
				_serializer.Deserialize<Dictionary<string, object>>(integrationPointAfterMigration.SourceConfiguration);

			string dataTransferLocationRoot = GetDataTransferLocationRoot();
			string expectedPath = Path.Combine(dataTransferLocationRoot, "ExportFolder");

			Assert.That(deserializedSourceConfigurationAfterMigration["Fileshare"], Is.EqualTo(expectedPath));
		}

		[Test]
		public void InvalidSourceConfigurationThrowsException()
		{
			Data.IntegrationPoint integrationPoint = CreateIntegrationPoint();
			integrationPoint.SourceConfiguration = "InvalidSourceConfiguration";
			_savedIntegrationPointId = SaveIntegrationPoint(integrationPoint);

			Assert.That(() => _dataTransferLocationMigration.Migrate(), Throws.Exception.TypeOf<InvalidOperationException>());
			var expectedMessage = $"Failed to migrate Integration Point: {integrationPoint.Name} with ArtifactId: {_savedIntegrationPointId}";

			_logger.Received(1).LogError(Arg.Any<Exception>(), expectedMessage);
		}

		[Test]
		public void MissingRelativitySourceProviderThrowsException()
		{
			Data.IntegrationPoint integrationPoint = CreateIntegrationPoint();
			_savedIntegrationPointId = SaveIntegrationPoint(integrationPoint);

			_sourceProviderRepository = Substitute.For<ISourceProviderRepository>();
			_sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(Constants.IntegrationPoints.SourceProviders.RELATIVITY)
				.Throws(new Exception("SampleException"));

			_dataTransferLocationMigration = CreteDataTransferLocationMigration(_logger, _destinationProviderRepository,
				_sourceProviderRepository, _dataTransferLocationMigrationHelper, CaseContext, _relativityObjectManager,
				_dataTransferLocationService, _resourcePoolManager, _ehHelper);

			Assert.That(() => _dataTransferLocationMigration.Migrate(), Throws.Exception);
			_logger.Received(1).LogError(Arg.Any<Exception>() ,"Failed to retrieve Relativity Source Provider ArtifactId");
		}

		[Test]
		public void MissingLoadFileDestinationProviderThrowsException()
		{
			Data.IntegrationPoint integrationPoint = CreateIntegrationPoint();
			_savedIntegrationPointId = SaveIntegrationPoint(integrationPoint);

			_destinationProviderRepository = Substitute.For<IDestinationProviderRepository>();
			_destinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(
					Constants.IntegrationPoints.DestinationProviders.LOADFILE)
				.Throws(new Exception("SampleException"));

			_dataTransferLocationMigration = CreteDataTransferLocationMigration(_logger, _destinationProviderRepository,
				_sourceProviderRepository, _dataTransferLocationMigrationHelper, CaseContext, _relativityObjectManager,
				_dataTransferLocationService, _resourcePoolManager, _ehHelper);

			Assert.That(() => _dataTransferLocationMigration.Migrate(), Throws.Exception);
			_logger.Received(1).LogError(Arg.Any<Exception>(), "Failed to retrieve LoadFile Destination Provider ArtifactId");
		}

		[Test]
		public void ErrorWithRetrievingIntegrationPointsThrowsException()
		{
			Data.IntegrationPoint integrationPoint = CreateIntegrationPoint();
			_savedIntegrationPointId = SaveIntegrationPoint(integrationPoint);

			_relativityObjectManager = Substitute.For<IRelativityObjectManager>();
			_relativityObjectManager.Query<Data.IntegrationPoint>(Arg.Any<QueryRequest>()).Throws(new Exception("SampleException"));

			_dataTransferLocationMigration = CreteDataTransferLocationMigration(_logger, _destinationProviderRepository,
				_sourceProviderRepository, _dataTransferLocationMigrationHelper, CaseContext, _relativityObjectManager,
				_dataTransferLocationService, _resourcePoolManager, _ehHelper);

			Assert.That(() => _dataTransferLocationMigration.Migrate(), Throws.Exception);
			_logger.Received(1).LogError(Arg.Any<Exception>(), "Failed to retrieve Integration Points data");
		}

		private IDataTransferLocationMigration CreteDataTransferLocationMigration(IAPILog logger,
			IDestinationProviderRepository destinationProviderRepository, ISourceProviderRepository sourceProviderRepository,
			IDataTransferLocationMigrationHelper dataTransferLocationMigrationHelper, ICaseServiceContext serviceContext,
			IRelativityObjectManager integrationPointLibrary,
			IDataTransferLocationService dataTransferLocationService, IResourcePoolManager resourcePoolManager, IEHHelper helper)
		{
			return new DataTransferLocationMigration(logger, destinationProviderRepository,
				sourceProviderRepository, dataTransferLocationMigrationHelper, serviceContext, integrationPointLibrary,
				dataTransferLocationService, resourcePoolManager, helper);
		}

		private Data.IntegrationPoint CreateIntegrationPoint()
		{
			IntegrationPointModel model = CreateIntegrationModel();
			IList<Choice> choices = _choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointFieldGuids.OverwriteFields));
			Data.IntegrationPoint integrationPointRdo = model.ToRdo(choices, new PeriodicScheduleRule());

			return integrationPointRdo;
		}

		private int SaveIntegrationPoint(Data.IntegrationPoint integrationPointRdo)
		{
			return _relativityObjectManager.Create(integrationPointRdo);
		}

		private IntegrationPointModel CreateIntegrationModel()
		{
			return new IntegrationPointModel
			{
				DestinationProvider = GetLoadFileProviderArtifactId(),
				SourceProvider = GetRelativityProviderArtifactId(),
				SourceConfiguration = CreateSourceConfiguration(),
				Name = "DataTransferMigrationIP" + DateTime.Now,
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler() { EnableScheduler = false}
			};
		}

		private string CreateSourceConfiguration()
		{
			IList<string> processingSourceLocations = GetProcessingSourceLocations();

			return $"{{ \"Fileshare\":\"\\\\{processingSourceLocations.First()}\\\\ExportFolder\"}}";
		}

		private int GetLoadFileProviderArtifactId()
		{
			return _destinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(Constants.IntegrationPoints.DestinationProviders.LOADFILE);
		}

		private int GetRelativityProviderArtifactId()
		{
			var relativityProvider =
				SourceProviders.First(
					x =>
						x.Identifier.Equals(Constants.IntegrationPoints.SourceProviders.RELATIVITY, StringComparison.OrdinalIgnoreCase));
			return relativityProvider.ArtifactId;
		}

		private IList<string> GetProcessingSourceLocations()
		{
			IList<ProcessingSourceLocationDTO> processingSourceLocationDtos = _resourcePoolManager.GetProcessingSourceLocation(WorkspaceArtifactId);
			return processingSourceLocationDtos.Select(x => x.Location).ToList();
		}

		private string GetDataTransferLocationRoot()
		{
			return _dataTransferLocationService.GetDefaultRelativeLocationFor(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);
		}

		private IList<ProcessingSourceLocationDTO> CreateSampleProcessingSourceLocations()
		{
			return new List<ProcessingSourceLocationDTO>()
			{
				new ProcessingSourceLocationDTO() {ArtifactId = 123456, Location = @"\\ProcessingSourceLocation"}
			};
		}
	}
}