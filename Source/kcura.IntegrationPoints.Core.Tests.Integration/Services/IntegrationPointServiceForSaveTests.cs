using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class IntegrationPointServiceForSaveTests : RelativityProviderTemplate
	{
		private const string _SOURCECONFIG = "Source Config";
		private const string _NAME = "Name";
		private const string _FIELDMAP = "Map";
		private const string _INTEGRATION_POINT_PROVIDER_VALIDATION_EXCEPTION_MESSAGE = "Integration Points provider validation failed, please review result property for the details.";
		private DestinationProvider _destinationProvider;
		private IJobService _jobService;

		public IntegrationPointServiceForSaveTests() : base("IntegrationPointService Source", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			var request = new QueryRequest
			{
				Fields = new DestinationProvider().ToFieldList(),
			};
			_destinationProvider = ObjectManager.Query<DestinationProvider>(request).First();
			_jobService = Container.Resolve<IJobService>();
		}

		#region UpdateProperties

		[IdentifiedTest("8a8a2bf4-7fbf-4da8-a7d2-4e9eaa2cb55c")]
		public void SaveIntegration_UpdateNothing()
		{
			//Arrange
			const string name = "SaveIntegration_UpdateNothing";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			//Act
			IntegrationPointModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			//Assert
			ValidateModel(originalModel, newModel, new string[0]);
		}

		[IdentifiedTest("be866088-3e84-4b04-9ac5-d7a68f1b021c")]
		public void SaveIntegration_UpdateName_OnRanIp_ErrorCase()
		{
			//Arrange
			const string name = "SaveIntegration_UpdateName_OnRanIp_ErrorCase";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);
			defaultModel.Name = "SaveIntegration_UpdateName_OnRanIp_ErrorCase-NewName";

			//Act & Assert
			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel));
		}

		[IdentifiedTest("a7e8ce95-14a3-4754-89fd-d629f1ef1f9c")]
		public void SaveIntegration_UpdateMap_OnRanIp()
		{
			//Arrange
			const string name = "SaveIntegration_UpdateMap_OnRanIp";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);
			defaultModel.Map = CreateSampleFieldsMapWithLongTextField();

			//Act
			IntegrationPointModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			//Assert
			ValidateModel(originalModel, newModel, new string[] { _FIELDMAP });
		}

		[IdentifiedTest("042b5825-71a0-47cc-b7dc-1fd203fd9d35")]
		public void SaveIntegration_UpdateConfig_OnNewRip()
		{
			//Arrange
			const string name = "SaveIntegration_UpdateConfig_OnNewRip";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			int newSavedSearch = SavedSearch.CreateSavedSearch(SourceWorkspaceArtifactID, name);
			defaultModel.SourceConfiguration = CreateSourceConfig(newSavedSearch, SourceWorkspaceArtifactID, SourceConfiguration.ExportType.SavedSearch);

			//Act & Assert
			Assert.DoesNotThrow(() => CreateOrUpdateIntegrationPoint(defaultModel));
		}

		[IdentifiedTest("cb18aa30-f3b7-4585-b080-bda80c22f5dd")]
		public void SaveIntegration_UpdateName_OnNewRip()
		{
			//Arrange
			const string name = "SaveIntegration_UpdateName_OnNewRip";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);
			defaultModel.Name = name + " 2";

			//Act & Assert
			Assert.Throws<Exception>(() => CreateOrUpdateIntegrationPoint(defaultModel), "Unable to save Integration Point: Name cannot be changed once the Integration Point has been run");
		}

		[IdentifiedTest("eebfc6d4-1e9e-4edc-b097-ea3e17df2c8b")]
		public void SaveIntegration_UpdateMap_OnNewRip()
		{
			//Arrange
			const string name = "SaveIntegration_UpdateMap_OnNewRip";
			IntegrationPointModel originalModel = CreateIntegrationPointThatIsAlreadyRunModel(name);
			IntegrationPointModel defaultModel = CreateOrUpdateIntegrationPoint(originalModel);

			defaultModel.Map = CreateSampleFieldsMapWithLongTextField();

			//Act
			IntegrationPointModel newModel = CreateOrUpdateIntegrationPoint(defaultModel);

			//Assert
			ValidateModel(originalModel, newModel, new[] { _FIELDMAP });
		}

		#endregion UpdateProperties

		[IdentifiedTestCase("f5a2acfe-df93-4ff8-93c9-39da403ba068", "")]
		[IdentifiedTestCase("2834dde8-69f5-46e3-8abf-9cd5ac2b207e", null)]
		[IdentifiedTestCase("3b26bad4-22d9-4221-9088-37d8bcc3040e", "02/31/3000")]
		[IdentifiedTestCase("105257b1-ec45-4621-8d6a-724847f749e5", "01-31-3000")]
		[IdentifiedTestCase("caafb3a0-0019-4c83-bf5c-b3ee93d45b7b", "abcdefg")]
		[IdentifiedTestCase("9ce111e3-887b-4571-8836-eda4a4017a81", "12345")]
		[IdentifiedTestCase("3fef2ba4-b866-4a00-bb9e-f7fae35a7140", "-01/31/3000")]
		public void CreateScheduledIntegrationPoint_WithInvalidStartDate_ExpectError(string startDate)
		{
			//Arrange
			DateTime utcNow = DateTime.UtcNow;

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"CreateScheduledIntegrationPoint_WithInvalidStartDate_ExpectError {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = startDate,
					EndDate = utcNow.AddDays(1).ToString("MM/dd/yyyy"),
					ScheduledTime = utcNow.ToString("HH") + ":" + utcNow.AddMinutes(1).ToString("mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					TimeZoneId = TimeZoneInfo.Utc.Id
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Act & Assert
			Assert.Throws<IntegrationPointValidationException>(() => CreateOrUpdateIntegrationPoint(integrationModel),
				_INTEGRATION_POINT_PROVIDER_VALIDATION_EXCEPTION_MESSAGE);
		}

		[IdentifiedTestCase("41567d7a-0d3c-4695-8138-4d01a0bd01a8", "")]
		[IdentifiedTestCase("c2942f7c-6817-4ea0-b862-97ebca44ba19", null)]
		[IdentifiedTestCase("c32007b1-1ac0-4a84-a0c7-2f81b4be5937", "15/31/3000")]
		[IdentifiedTestCase("3b92dc98-ecd0-413a-92de-7ba34b650f88", "31-01-3000")]
		[IdentifiedTestCase("6ac17062-fc4d-4fa4-9e23-1483e550bc1a", "abcdefg")]
		[IdentifiedTestCase("15828d94-39f6-4f8a-97bf-8e94befdf8b8", "12345")]
		[IdentifiedTestCase("0cc2bdac-b0d6-4968-af02-da1b614789a1", "-01/31/3000")]
		public void CreateScheduledIntegrationPoint_WithInvalidEndDate_ExpectError(string endDate)
		{
			//Arrange
			DateTime utcNow = DateTime.UtcNow;

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"CreateScheduledIntegrationPoint_WithInvalidEndDate_ExpectError {DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = utcNow.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					EndDate = endDate,
					ScheduledTime = utcNow.ToString("HH") + ":" + utcNow.AddMinutes(1).ToString("mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			//Act
			IntegrationPointModel integrationPointModel = CreateOrUpdateIntegrationPoint(integrationModel);
			//We need to clean ScheduleAgentQueue table because this Integration Point is scheduled and RIP Application and workspace 
			//will be deleted after test is finished
			CleanScheduleAgentQueueFromAllRipJobs(integrationPointModel.ArtifactID);

			//Assert 
			Assert.IsNull(integrationPointModel.Scheduler.EndDate);
		}

		#region "Helpers"

		private void CleanScheduleAgentQueueFromAllRipJobs(int integrationPointArtifactId)
		{
			IList<Job> jobs = _jobService.GetJobs(integrationPointArtifactId);
			foreach (Job job in jobs)
			{
				_jobService.DeleteJob(job.JobId);
			}
		}

		private void ValidateModel(IntegrationPointModel expectedModel, IntegrationPointModel actual, string[] updatedProperties)
		{
			Action<object, object> assertion = DetermineAssertion(updatedProperties, _SOURCECONFIG);
			assertion(expectedModel.SourceConfiguration, actual.SourceConfiguration);

			assertion = DetermineAssertion(updatedProperties, _NAME);
			assertion(expectedModel.Name, actual.Name);

			assertion = DetermineAssertion(updatedProperties, _FIELDMAP);
			assertion(expectedModel.Map, actual.Map);

			Assert.AreEqual(expectedModel.HasErrors, actual.HasErrors);
			Assert.AreEqual(expectedModel.DestinationProvider, actual.DestinationProvider);
			Assert.NotNull(expectedModel.LastRun);
			Assert.NotNull(actual.LastRun);
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

		private string CreateSourceConfig(int savedSearchId, int targetWorkspaceId, SourceConfiguration.ExportType typeOfExport)
		{
			var sourceConfiguration = new SourceConfiguration()
			{
				SavedSearchArtifactId = savedSearchId,
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
				TargetWorkspaceArtifactId = targetWorkspaceId,
				TypeOfExport = typeOfExport
			};
			return Container.Resolve<ISerializer>().Serialize(sourceConfiguration);
		}

		private IntegrationPointModel CreateIntegrationPointThatHasNotRun(string name)
		{
			return new IntegrationPointModel()
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.OverlayOnly),
				DestinationProvider = _destinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateSourceConfig(SavedSearchArtifactID, TargetWorkspaceArtifactID, SourceConfiguration.ExportType.SavedSearch),
				LogErrors = true,
				Name = $"${name} - {DateTime.Today:yy-MM-dd HH-mm-ss}",
				Map = CreateDefaultFieldMap(),
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};
		}

		private IntegrationPointModel CreateIntegrationPointThatIsAlreadyRunModel(string name)
		{
			IntegrationPointModel model = CreateIntegrationPointThatHasNotRun(name);
			model.LastRun = DateTime.UtcNow;
			return model;
		}

		private FieldMap[] GetSampleFields()
		{
			var repositoryFactory = Container.Resolve<IRepositoryFactory>();
			IFieldQueryRepository sourceFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(SourceWorkspaceArtifactID);
			IFieldQueryRepository destinationFieldQueryRepository = repositoryFactory.GetFieldQueryRepository(TargetWorkspaceArtifactID);

			ArtifactDTO identifierSourceField = sourceFieldQueryRepository.RetrieveIdentifierField((int)ArtifactType.Document);
			ArtifactDTO identifierDestinationField = destinationFieldQueryRepository.RetrieveIdentifierField((int)ArtifactType.Document);

			ArtifactFieldDTO[] sourceFields = sourceFieldQueryRepository.RetrieveLongTextFieldsAsync((int)ArtifactType.Document).Result;
			ArtifactFieldDTO[] destinationFields = destinationFieldQueryRepository.RetrieveLongTextFieldsAsync((int)ArtifactType.Document).Result;

			var map = new[]
			{
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = identifierSourceField.ArtifactId.ToString(),
						DisplayName = identifierSourceField.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true
					},
					FieldMapType = FieldMapTypeEnum.Identifier,
					DestinationField = new FieldEntry()
					{
						FieldIdentifier = identifierDestinationField.ArtifactId.ToString(),
						DisplayName = identifierDestinationField.Fields.First(field => field.Name == "Name").Value as string + " [Object Identifier]",
						IsIdentifier = true
					},
				},
				new FieldMap()
				{
					SourceField = new FieldEntry()
					{
						FieldIdentifier = sourceFields.First().ArtifactId.ToString(),
						DisplayName = sourceFields.First().Name,
						IsIdentifier = false
					},
					FieldMapType = FieldMapTypeEnum.None,
					DestinationField = new FieldEntry()
					{
						FieldIdentifier = destinationFields.First().ArtifactId.ToString(),
						DisplayName = destinationFields.First().Name,
						IsIdentifier = false
					}
				}
			};
			return map;
		}

		private string CreateSampleFieldsMapWithLongTextField()
		{
			FieldMap[] map = GetSampleFields();
			return Container.Resolve<ISerializer>().Serialize(map);
		}
		#endregion
	}
}