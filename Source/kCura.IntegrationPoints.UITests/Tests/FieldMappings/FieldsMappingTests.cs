using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;
using kCura.IntegrationPoints.UITests.Configuration.Models;
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

			//Identifier and another random field
			List<string> fieldsToBeChanged = new List<string>{ "Control Number" , "File Name"};
            
            foreach (var fieldName in fieldsToBeChanged)
            {
				await SetRandomNameToFLTFieldDestinationWorkspaceAsync(fieldName).ConfigureAwait(false);
                await SetRandomNameToFLTFieldSourceWorkspaceAsync(fieldName).ConfigureAwait(false);
			}
			
			await SourceContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
            await DestinationContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
			SyncFieldMapResults mapAllFieldsUiTestEdition  = new SyncFieldMapResults(SourceContext.WorkspaceAutoMapAllEnabledFields, DestinationContext.WorkspaceAutoMapAllEnabledFields);
            
			List<string> expectedInOrderSelectedSourceMappableFieldsList =
                mapAllFieldsUiTestEdition.FieldMapSorted.Select(x =>x.SourceFieldObject.DisplayName).ToList();

			FieldMapModel expectedIdentifierMatchedField =
                mapAllFieldsUiTestEdition.FieldMap.Single(x =>
                    x.AutoMapMatchType == TestConstants.FieldMapMatchType.IsIdentifier);
            var expectedFieldPairIsIdentifier = new FieldDisplayNamePair(expectedIdentifierMatchedField);

			List<FieldMapModel> expectedArtifactIDMatchedFields =
                mapAllFieldsUiTestEdition.FieldMap.Where(x =>
                    x.AutoMapMatchType == TestConstants.FieldMapMatchType.ArtifactID).ToList();
            var expectedFieldPairsArtifactIDList = expectedArtifactIDMatchedFields.Select(fieldPair => new FieldDisplayNamePair(fieldPair)).ToList();

			List<FieldMapModel> expectedNameMatchedFields =
                mapAllFieldsUiTestEdition.FieldMap.Where(x =>
                    x.AutoMapMatchType == TestConstants.FieldMapMatchType.Name).ToList();
            var expectedFieldPairsNameList = expectedNameMatchedFields.Select(fieldPair => new FieldDisplayNamePair(fieldPair)).ToList();

			PushToRelativityThirdPage fieldMappingPage =
                PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			//Act
			fieldMappingPage = fieldMappingPage.MapAllFields();

			//Assert
			List<string> fieldsFromSelectedSourceWorkspaceListBox =
                fieldMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();
            List<string> fieldsFromSelectedDestinationWorkspaceListBox =
                fieldMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();

            var fieldPairsFromSelectedListBox = new List<FieldDisplayNamePair>();
            foreach (var sourceDisplayName in fieldsFromSelectedSourceWorkspaceListBox)
            {
                int index = fieldsFromSelectedSourceWorkspaceListBox.IndexOf(sourceDisplayName);
                string destinationDisplayName = fieldsFromSelectedDestinationWorkspaceListBox[index];
                fieldPairsFromSelectedListBox.Add(new FieldDisplayNamePair(sourceDisplayName, destinationDisplayName));
            }

            fieldsFromSelectedSourceWorkspaceListBox.Should()
                .ContainInOrder(expectedInOrderSelectedSourceMappableFieldsList);

			fieldPairsFromSelectedListBox.Should().ContainEquivalentOf(expectedFieldPairIsIdentifier);
            foreach (var fieldDisplayNamePair in expectedFieldPairsArtifactIDList)
            {
				fieldPairsFromSelectedListBox.Should().ContainEquivalentOf(fieldDisplayNamePair);
			}

			foreach (var fieldDisplayNamePair in expectedFieldPairsNameList)
            {
                fieldPairsFromSelectedListBox.Should().ContainEquivalentOf(fieldDisplayNamePair);
            }
		}

		private List<string> CreateFieldMapListBoxFormatFromObjectManagerFetchedList(
			List<FieldObject> mappableFieldsListFromObjectManager)
		{
			return SyncFieldMapResults.SortFieldObjects(mappableFieldsListFromObjectManager)
				.Select(f => f.DisplayName).ToList();
		}
    }
}