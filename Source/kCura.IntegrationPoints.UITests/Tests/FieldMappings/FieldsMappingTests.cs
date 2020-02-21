﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.FieldMappings
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
    [Category(TestCategory.EXPORT_TO_RELATIVITY)]
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

		[IdentifiedTest("916e57ba-fb4d-42a4-be2a-4d17df17de59")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		public async Task FieldMapping_ShouldAutoMapAllValidFields_WhenMapFieldsButtonIsPressed()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.FieldMapping = null;
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//mapping Identifiers with different names
            List<string> fieldToChangeName = new List<string>{"Control Number", "FileName"};
            foreach (var fieldName in fieldToChangeName)
            {
				await SetRandomNameToFLTFieldDestinationWorkspaceAsync(fieldName).ConfigureAwait(false);
                await SetRandomNameToFLTFieldSourceWorkspaceAsync(fieldName).ConfigureAwait(false);
			}

            await SourceContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
            await DestinationContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);

			PushToRelativityThirdPage fieldMappingPage =
                PointsAction.CreateNewRelativityProviderFieldMappingPage(model);

            List<FieldObject> mapAllFieldsUiTestEdition = GetMapAllFieldsUiTestEdition(
                SourceContext.WorkspaceAutoMapAllEnabledFields, DestinationContext.WorkspaceAutoMapAllEnabledFields);
            List<string> expectedSelectedCommonMappableFields =
                CreateFieldMapListBoxFormatFromObjectManagerFetchedList(mapAllFieldsUiTestEdition);

            //Act
			fieldMappingPage = fieldMappingPage.MapAllFields();

            //Assert
			List<string> fieldsFromSelectedDestinationWorkspaceListBox =
				fieldMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();
			List<string> fieldsFromSelectedSourceWorkspaceListBox =
				fieldMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();

			fieldsFromSelectedSourceWorkspaceListBox.Should().ContainInOrder(expectedSelectedCommonMappableFields);
			fieldsFromSelectedDestinationWorkspaceListBox.Should().ContainInOrder(expectedSelectedCommonMappableFields);
		}

		private List<string> CreateFieldMapListBoxFormatFromObjectManagerFetchedList(
			List<FieldObject> mappableFieldsListFromObjectManager)
		{
			return mappableFieldsListFromObjectManager
                .OrderByDescending(f => f.IsIdentifier)
				.ThenBy(f => f.Name)
				.ThenBy(f => f.Type)
				.Select(field =>
					field.IsIdentifier ? $"{field.Name} [Object Identifier]" : $"{field.Name} [{field.DisplayType}]").ToList();
		}

        private List<FieldObject> GetMapAllFieldsUiTestEdition(List<FieldObject> sourceWorkspaceFields,
            List<FieldObject> destinationWorkspaceFields)
        {   
            var mapAllFieldList = new List<FieldObject>();
			foreach (var swf in sourceWorkspaceFields)
            {
                foreach (var dwf in destinationWorkspaceFields)
                {
					if (swf.ArtifactID == dwf.ArtifactID)
                    {
						mapAllFieldList.Add(swf);
                    }
                    else if (swf.Name == dwf.Name)
                    {
                        mapAllFieldList.Add(swf);
                    }
				}
            }

            return mapAllFieldList;
        }
	}
}