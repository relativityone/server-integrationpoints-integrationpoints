﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.FieldMappings
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.FIELDS_MAPPING)]
	public class FieldsMappingTests : RelativityProviderTestsBase
	{
		private static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
			new Tuple<string, string>("Extracted Text", "Extracted Text"),
			new Tuple<string, string>("Title", "Title")
		};

		private RelativityProviderModel CreateRelativityProviderModel(string name)
		{
			RelativityProviderModel model = new RelativityProviderModel(name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No,
				FieldMapping = DefaultFieldsMapping
			};

			return model;
		}

		private RelativityProviderModel CreateRelativityProviderModel()
		{
			return CreateRelativityProviderModel(TestContext.CurrentContext.Test.Name);
		}

		[IdentifiedTest("916e57ba-fb4d-42a4-be2a-4d17df17de57")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		public void FieldMapping_ShouldDisplayMappableFieldsInCorrectOrderInSourceWorkspaceFieldList()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			PushToRelativityThirdPage fieldMappingPage =
				PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			List<string> fieldsFromSourceWorkspaceListBox = fieldMappingPage.GetFieldsFromSourceWorkspaceListBox();
			List<string> expectedSourceMappableFields =
				CreateFieldMapListBoxFormatFromObjectManagerFetchedList(SourceContext.WorkspaceMappableFields);

			//Assert
			fieldsFromSourceWorkspaceListBox.Should().ContainInOrder(expectedSourceMappableFields);
		}

		[IdentifiedTest("916e57ba-fb4d-42a4-be2a-4d17df17de58")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		public void FieldMapping_ShouldDisplayMappableFieldsInCorrectOrderInDestinationWorkspaceFieldList()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			PushToRelativityThirdPage fieldMappingPage =
				PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			List<string> fieldsFromDestinationWorkspaceListBox =
				fieldMappingPage.GetFieldsFromDestinationWorkspaceListBox();
			List<string> expectedDestinationMappableFields =
				CreateFieldMapListBoxFormatFromObjectManagerFetchedList(DestinationContext.WorkspaceMappableFields);

			//Assert
			fieldsFromDestinationWorkspaceListBox.Should().ContainInOrder(expectedDestinationMappableFields);
		}
		
		[IdentifiedTest("65917e62-2387-4b1e-afee-721bac33b1c0")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		public async Task FieldMapping_ShouldAutoMapFieldsFromSavedSearch()
		{
			//Arrange
			var createFieldRequest = new FixedLengthFieldRequest()
			{
				ObjectType = new ObjectTypeIdentifier()
				{
					ArtifactTypeID = (int) ArtifactType.Document
				},
				Name = "Orzel 7",
				Length = 255,
				
			};

			int sourceFieldID = await SourceFieldManager
				.CreateFixedLengthFieldAsync(SourceContext.GetWorkspaceId(), createFieldRequest)
				.ConfigureAwait(false);
			int destinationFieldID = await DestinationFieldManager
				.CreateFixedLengthFieldAsync(DestinationContext.GetWorkspaceId(), createFieldRequest)
				.ConfigureAwait(false);

			int savedSearchID = await SavedSearch.CreateSavedSearchAsync(SourceContext.GetWorkspaceId(),
				"Orzel 7 search", new[]
				{
					new FieldRef
					{
						ArtifactID = sourceFieldID
					}
				}).ConfigureAwait(false);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			PushToRelativityThirdPage fieldMappingPage =
				PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			List<string> fieldsFromDestinationWorkspaceListBox =
				fieldMappingPage.GetFieldsFromDestinationWorkspaceListBox();
			List<string> expectedDestinationMappableFields =
				CreateFieldMapListBoxFormatFromObjectManagerFetchedList(DestinationContext.WorkspaceMappableFields);
		}

		private List<string> CreateFieldMapListBoxFormatFromObjectManagerFetchedList(
			List<FieldObject> mappableFieldsListFromObjectManager)
		{
			return mappableFieldsListFromObjectManager.OrderBy(f => f.Name)
				.ThenBy(f => f.Type)
				.Select(field =>
					field.IsIdentifier ? $"{field.Name} [Object Identifier]" : $"{field.Name} [{field.Type}]").ToList();
		}
	}
}