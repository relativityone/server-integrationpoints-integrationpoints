using System;
using System.Collections.Generic;
using System.Linq;
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

        [IdentifiedTest("916e57ba-fb4d-42a4-be2a-4d17df17de60")]
        [RetryOnError]
		public void FieldMapping_ShouldClearMapFromInvalidField_WhenClearButtonIsPressed()
		{
			//Arrange
            const string _INVALID_FIELD_MAPPING_MESSAGE_TEXT = "Your job may be unsuccessfully finished by those Source and Destination fields:";
			List<Tuple<string, string>> FieldsMapping = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Control Number", "Control Number"),
            };
            List<Tuple<string, string>> InvalidFieldsMapping = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Alert", "Classification Index")
            };
            FieldsMapping.AddRange(InvalidFieldsMapping);

			RelativityProviderModel model = CreateRelativityProviderModel();

            PushToRelativityThirdPage fieldMappingPage =
				PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			PointsAction.MapWorkspaceFields(fieldMappingPage, FieldsMapping);
			fieldMappingPage = fieldMappingPage.ClickSaveButtonExpectPopup();

			
			//Assert text on popup
            fieldMappingPage.GetTextFromPopupBox().Should().Be(_INVALID_FIELD_MAPPING_MESSAGE_TEXT);
            
            //Act
			IntegrationPointDetailsPage detailsPage = fieldMappingPage.ClearAndProceedOnInvalidMapping();
			
            PushToRelativityThirdPage clearedMappingPage = PointsAction.EditGoToFieldMappingPage(detailsPage);
			List<string> fieldsFromSelectedSourceWorkspaceListBox =
                clearedMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();
			List<string> fieldsFromSelectedDestinationWorkspaceListBox =
                clearedMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();
            
            //Assert if fields were removed from mapping
			fieldsFromSelectedDestinationWorkspaceListBox.Should().NotContain(InvalidFieldsMapping.Select(x =>x.Item1));
            fieldsFromSelectedSourceWorkspaceListBox.Should().NotContain(InvalidFieldsMapping.Select(x => x.Item2));
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