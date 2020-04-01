﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Services.Folder;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.Helpers
{
	public class IntegrationPointBaseHelper
	{
		public static void AssertIntegrationPointModelBase(BaseRdo integrationPointBaseRdo, IntegrationPointModel expectedIntegrationPointModel, IIntegrationPointBaseFieldGuidsConstants guidsConstants)
		{
			var actualSourceConfiguration = JsonConvert.DeserializeObject<RelativityProviderSourceConfiguration>(integrationPointBaseRdo.GetField<string>(new Guid(guidsConstants.SourceConfiguration)));
			var actualDestinationConfiguration = JsonConvert.DeserializeObject<RelativityProviderDestinationConfiguration>(integrationPointBaseRdo.GetField<string>(new Guid(guidsConstants.DestinationConfiguration)));

			var expectedSourceConfiguration = expectedIntegrationPointModel.SourceConfiguration as RelativityProviderSourceConfiguration;
			var expectedDestinationConfiguration = expectedIntegrationPointModel.DestinationConfiguration as RelativityProviderDestinationConfiguration;

			Assert.That(integrationPointBaseRdo.GetField<int?>(new Guid(guidsConstants.SourceProvider)), Is.EqualTo(expectedIntegrationPointModel.SourceProvider));
			Assert.That(integrationPointBaseRdo.GetField<int?>(new Guid(guidsConstants.DestinationProvider)), Is.EqualTo(expectedIntegrationPointModel.DestinationProvider));
			Assert.That(integrationPointBaseRdo.GetField<string>(new Guid(guidsConstants.EmailNotificationRecipients)) ?? string.Empty, Is.EqualTo(expectedIntegrationPointModel.EmailNotificationRecipients ?? string.Empty));
			Assert.That(integrationPointBaseRdo.GetField<bool>(new Guid(guidsConstants.EnableScheduler)), Is.EqualTo(expectedIntegrationPointModel.ScheduleRule.EnableScheduler));
			Assert.That(integrationPointBaseRdo.GetField<bool>(new Guid(guidsConstants.LogErrors)), Is.EqualTo(expectedIntegrationPointModel.LogErrors));
			Assert.That(integrationPointBaseRdo.GetField<string>(new Guid(guidsConstants.Name)), Is.EqualTo(expectedIntegrationPointModel.Name));
			Assert.That(integrationPointBaseRdo.GetField<int?>(new Guid(guidsConstants.Type)), Is.EqualTo(expectedIntegrationPointModel.Type));
			Assert.That(integrationPointBaseRdo.GetField<Choice>(new Guid(guidsConstants.OverwriteFields)).ArtifactID, Is.EqualTo(expectedIntegrationPointModel.OverwriteFieldsChoiceId));


			Assert.That(actualSourceConfiguration.SourceWorkspaceArtifactId, Is.EqualTo(expectedSourceConfiguration.SourceWorkspaceArtifactId));
			Assert.That(actualSourceConfiguration.SavedSearchArtifactId, Is.EqualTo(expectedSourceConfiguration.SavedSearchArtifactId));


			Assert.That(actualDestinationConfiguration.ArtifactTypeID, Is.EqualTo(expectedDestinationConfiguration.ArtifactTypeID));
			Assert.That(actualDestinationConfiguration.CaseArtifactId, Is.EqualTo(expectedDestinationConfiguration.CaseArtifactId));
			Assert.That(actualDestinationConfiguration.DestinationFolderArtifactId, Is.EqualTo(expectedDestinationConfiguration.DestinationFolderArtifactId));
			Assert.That(actualDestinationConfiguration.FieldOverlayBehavior, Is.EqualTo(expectedDestinationConfiguration.FieldOverlayBehavior));
			Assert.That(actualDestinationConfiguration.FolderPathSourceField, Is.EqualTo(expectedDestinationConfiguration.FolderPathSourceField));
			Assert.That(actualDestinationConfiguration.ImportNativeFile, Is.EqualTo(expectedDestinationConfiguration.ImportNativeFile));
			Assert.That(actualDestinationConfiguration.UseFolderPathInformation, Is.EqualTo(expectedDestinationConfiguration.UseFolderPathInformation));

			Assert.That(integrationPointBaseRdo.GetField<string>(new Guid(guidsConstants.FieldMappings)), Is.EqualTo(JsonConvert.SerializeObject(expectedIntegrationPointModel.FieldMappings)));
		}

		public static CreateIntegrationPointRequest CreateCreateIntegrationPointRequest(ITestHelper helper, IRepositoryFactory repositoryFactory, int workspaceArtifactId,
			int savedSearchArtifactId, int typeOfExport, int targetWorkspaceArtifactId, bool importNativeFile, bool logErrors, bool useFolderPathInformation, string emailNotificationRecipients,
			string fieldOverlayBehavior, OverwriteFieldsModel overwriteFieldsModel, List<FieldMap> fieldMappings)
		{
			var folderPathSourceField = 0;
			if (useFolderPathInformation)
			{
				var artifactFieldDtos = repositoryFactory.GetFieldQueryRepository(workspaceArtifactId).RetrieveLongTextFieldsAsync((int) ArtifactType.Document).Result;
				folderPathSourceField = artifactFieldDtos[0].ArtifactId;
			}

			var expectedDestinationConfiguration = new RelativityProviderDestinationConfiguration
			{
				ArtifactTypeID = (int) ArtifactType.Document,
				CaseArtifactId = targetWorkspaceArtifactId,
				ImportNativeFile = importNativeFile,
				UseFolderPathInformation = useFolderPathInformation,
				FolderPathSourceField = folderPathSourceField,
				FieldOverlayBehavior = fieldOverlayBehavior,
				DestinationFolderArtifactId = GetRootFolder(helper, workspaceArtifactId)
			};
			var expectedSourceConfiguration = new RelativityProviderSourceConfiguration
			{
				SourceWorkspaceArtifactId = workspaceArtifactId,
				SavedSearchArtifactId = savedSearchArtifactId,
				TypeOfExport = typeOfExport
			};
			var expectedIntegrationPointModel = new IntegrationPointModel
			{
				ArtifactId = 0,
				EmailNotificationRecipients = emailNotificationRecipients,
				LogErrors = logErrors,
				Name = $"relativity_{Utils.FormattedDateTimeNow}",
				SourceProvider = GetSourceProviderArtifactId(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY, workspaceArtifactId, helper),
				DestinationProvider = GetDestinationProviderArtifactId(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY, workspaceArtifactId, helper),
				DestinationConfiguration = expectedDestinationConfiguration,
				SourceConfiguration = expectedSourceConfiguration,
				FieldMappings = fieldMappings,
				Type = GetTypeArtifactId(helper, workspaceArtifactId, kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportName),
				OverwriteFieldsChoiceId = overwriteFieldsModel.ArtifactId,
				ScheduleRule = new ScheduleModel
				{
					EnableScheduler = false
				}
			};

			return new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = workspaceArtifactId,
				IntegrationPoint = expectedIntegrationPointModel
			};
		}

		private static int GetRootFolder(ITestHelper helper, int workspaceArtifactId)
		{
			using (var folderManager = helper.CreateProxy<IFolderManager>())
			{
				return folderManager.GetWorkspaceRootAsync(workspaceArtifactId).Result.ArtifactID;
			}
		}

		public static int GetTypeArtifactId(ITestHelper helper, int workspaceArtifactId, string typeName)
		{
			using (var typeClient = helper.CreateProxy<IIntegrationPointTypeManager>())
			{
				return typeClient.GetIntegrationPointTypes(workspaceArtifactId).Result.First(x => x.Name == typeName).ArtifactId;
			}
		}

		public static int GetSourceProviderArtifactId(string guid, int workspaceArtifactId, ITestHelper helper)
		{
			using (var providerClient = helper.CreateProxy<IProviderManager>())
			{
				return providerClient.GetSourceProviderArtifactIdAsync(workspaceArtifactId, guid).Result;
			}
		}

		public static int GetDestinationProviderArtifactId(string guid, int workspaceArtifactId, ITestHelper helper)
		{
			using (var providerClient = helper.CreateProxy<IProviderManager>())
			{
				return providerClient.GetDestinationProviderArtifactIdAsync(workspaceArtifactId, guid).Result;
			}
		}
	}
}