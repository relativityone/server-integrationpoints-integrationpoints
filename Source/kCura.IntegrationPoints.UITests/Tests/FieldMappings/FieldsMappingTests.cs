﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;
using kCura.IntegrationPoints.UITests.Configuration.Models;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Testing.Identification;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.UITests.Tests.FieldMappings
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync.FieldMapping]
	[NUnit.Framework.Category(TestCategory.EXPORT_TO_RELATIVITY)]
	[NUnit.Framework.Category(TestCategory.FIELDS_MAPPING)]
	public class FieldsMappingTests : RelativityProviderTestsBase
	{
		private static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
			new Tuple<string, string>("Extracted Text", "Extracted Text"),
			new Tuple<string, string>("Title", "Title")
		};

		protected override Task SuiteSpecificOneTimeSetup()
		{
			return CreateFixedLengthFieldsWithSpecialCharactersAsync(SourceContext.GetWorkspaceId(), SourceFieldManager);
		}

		protected override Task SuiteSpecificSetup()
		{
			return CreateFixedLengthFieldsWithSpecialCharactersAsync(DestinationContext.GetWorkspaceId(), DestinationFieldManager);
		}

		protected async Task CreateFixedLengthFieldsWithSpecialCharactersAsync(int workspaceID, IFieldManager fieldManager)
		{
			char[] specialCharacters = @"!@#$%^&*()-_+= {}|\/;'<>,.?~`".ToCharArray();
			for (int i = 0; i < specialCharacters.Length; i++)
			{
				char special = specialCharacters[i];
				string generatedFieldName = $"aaaaa{special}{i}";
				var fixedLengthTextFieldRequest = new FixedLengthFieldRequest
				{
					ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)global::Relativity.ArtifactType.Document },
					Name = $"{generatedFieldName} FLT",
					Length = 255
				};

				var longTextFieldRequest = new LongTextFieldRequest
				{
					ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)global::Relativity.ArtifactType.Document },
					Name = $"{generatedFieldName} LTF"
				};

				await fieldManager.CreateLongTextFieldAsync(workspaceID, longTextFieldRequest).ConfigureAwait(false);
				await fieldManager.CreateFixedLengthFieldAsync(workspaceID, fixedLengthTextFieldRequest).ConfigureAwait(false);
			}
		}

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
			CollectionAssert.AreEqual(fieldsFromSourceWorkspaceListBox, expectedSourceMappableFields);
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
			CollectionAssert.AreEqual(fieldsFromDestinationWorkspaceListBox, expectedDestinationMappableFields);
		}

		[IdentifiedTest("916e57ba-fb4d-42a4-be2a-4d17df17de60")]
		[RetryOnError]
		public void FieldMapping_ShouldClearMapFromInvalidField_WhenClearButtonIsPressed()
		{
			//Arrange
			const string invalidFieldMappingMessageText = "Mapping of the fields below may fail your job:";
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
			fieldMappingPage.PopupText.Should().Be(invalidFieldMappingMessageText);

			//Act
			IntegrationPointDetailsPage detailsPage = fieldMappingPage.ClearAndProceedOnInvalidMapping();

			PushToRelativityThirdPage clearedMappingPage = PointsAction.EditGoToFieldMappingPage(detailsPage);
			List<string> fieldsFromSelectedSourceWorkspaceListBox =
				clearedMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();
			List<string> fieldsFromSelectedDestinationWorkspaceListBox =
				clearedMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();

			//Assert if fields were removed from mapping
			fieldsFromSelectedDestinationWorkspaceListBox.Should().NotContain(InvalidFieldsMapping.Select(x => x.Item1));
			fieldsFromSelectedSourceWorkspaceListBox.Should().NotContain(InvalidFieldsMapping.Select(x => x.Item2));
		}

		[Test]
		[RetryOnError]
		public async Task FieldMapping_ShouldRemapFields_WhenDestinationWorkspaceWasChanged()
		{
			//Arrange
			const string addtionalFieldName = "Adler sieben";
			const string newDestination = "New destination";
			ObjectTypeIdentifier documentObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document };

			int newWorkspaceID = await Workspace.CreateWorkspaceAsync(newDestination, DestinationContext.WorkspaceName, Log)
				.ConfigureAwait(false);

			await SourceFieldManager.CreateDecimalFieldAsync(SourceContext.WorkspaceId.Value, new DecimalFieldRequest { Name = addtionalFieldName, ObjectType = documentObjectType })
				.ConfigureAwait(false);
			await DestinationFieldManager.CreateDecimalFieldAsync(DestinationContext.WorkspaceId.Value, new DecimalFieldRequest { Name = addtionalFieldName, ObjectType = documentObjectType })
				.ConfigureAwait(false);

			await SourceFieldManager.CreateDecimalFieldAsync(newWorkspaceID, new DecimalFieldRequest { Name = addtionalFieldName, ObjectType = documentObjectType }).ConfigureAwait(false);

			try
			{
				RelativityProviderModel model = CreateRelativityProviderModel();

				PushToRelativityThirdPage fieldMappingPage =
					PointsAction.CreateNewRelativityProviderFieldMappingPage(model);

				fieldMappingPage.MapAllFields();

				List<string> expectedDestinationMappedFields =
					fieldMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();
				List<string> expectedSourceMappedFields =
					fieldMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();

				IntegrationPointDetailsPage detailsPage = fieldMappingPage.SaveIntegrationPoint();

				//Act
				PushToRelativitySecondPage destinationPage = detailsPage.EditIntegrationPoint().GoToNextPagePush();
				destinationPage.DestinationWorkspace = $"{newDestination} - {newWorkspaceID}";

				destinationPage.SelectFolderLocation();
				destinationPage.WaitForPage();
				destinationPage.FolderLocationSelect.ChooseRootElement();

				fieldMappingPage = destinationPage.GoToNextPage();

				//Assert
				fieldMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox()
					.ShouldAllBeEquivalentTo(expectedSourceMappedFields);
				fieldMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox()
					.ShouldAllBeEquivalentTo(expectedDestinationMappedFields);

				fieldMappingPage.PageInfoMessageText.Should()
					.Be("We restored the fields mapping as destination workspace has changed");
			}
			finally
			{
				Workspace.DeleteWorkspace(newWorkspaceID);
			}
		}


		[IdentifiedTest("916e57ba-fb4d-42a4-be2a-4d17df17de59")]
		[RetryOnError]
		[NUnit.Framework.Category(TestCategory.SMOKE)]
		public async Task FieldMapping_ShouldAutoMapAllValidFields_WhenMapFieldsButtonIsPressed()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.FieldMapping = null;
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Identifier and another random field
			List<Tuple<string, string>> fieldsToBeRenamed = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("Control Number", FieldObject.GetRandomName("Control Number")),
				new Tuple<string, string>("File Name", FieldObject.GetRandomName("File Name"))
			};
            
            foreach (Tuple<string, string> field in fieldsToBeRenamed)
			{
				await RenameFieldInSourceWorkspaceAsync(field.Item1, field.Item2).ConfigureAwait(false);
				await RenameFieldInDestinationWorkspaceAsync(field.Item1, field.Item2).ConfigureAwait(false);
			}

            try
            {
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
	            List<FieldDisplayNamePair> expectedFieldPairsArtifactIDList = expectedArtifactIDMatchedFields.Select(fieldPair => new FieldDisplayNamePair(fieldPair)).ToList();

	            List<FieldMapModel> expectedNameMatchedFields =
		            mapAllFieldsUiTestEdition.FieldMap.Where(x =>
			            x.AutoMapMatchType == TestConstants.FieldMapMatchType.Name).ToList();
	            List<FieldDisplayNamePair> expectedFieldPairsNameList = expectedNameMatchedFields.Select(fieldPair => new FieldDisplayNamePair(fieldPair)).ToList();

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
	            foreach (string sourceDisplayName in fieldsFromSelectedSourceWorkspaceListBox)
	            {
		            int index = fieldsFromSelectedSourceWorkspaceListBox.IndexOf(sourceDisplayName);
		            string destinationDisplayName = fieldsFromSelectedDestinationWorkspaceListBox[index];
		            fieldPairsFromSelectedListBox.Add(new FieldDisplayNamePair(sourceDisplayName, destinationDisplayName));
	            }

	            CollectionAssert.AreEqual(fieldsFromSelectedSourceWorkspaceListBox, expectedInOrderSelectedSourceMappableFieldsList);
			
	            Assert.IsTrue(fieldPairsFromSelectedListBox.Exists(x => CompareFieldDisplayNamePair(x, expectedFieldPairIsIdentifier)));

	            foreach (FieldDisplayNamePair fieldDisplayNamePair in expectedFieldPairsArtifactIDList)
	            {
		            Assert.IsTrue(fieldPairsFromSelectedListBox.Exists(x => CompareFieldDisplayNamePair(x, fieldDisplayNamePair)));
	            }

	            foreach (FieldDisplayNamePair fieldDisplayNamePair in expectedFieldPairsNameList)
	            {
		            Assert.IsTrue(fieldPairsFromSelectedListBox.Exists(x => CompareFieldDisplayNamePair(x, fieldDisplayNamePair)));
	            }
            }
            finally
            {
				// Rename fields back to original names
				foreach (Tuple<string, string> field in fieldsToBeRenamed)
				{
					await RenameFieldInDestinationWorkspaceAsync(field.Item2, field.Item1).ConfigureAwait(false);
					await RenameFieldInSourceWorkspaceAsync(field.Item2, field.Item1).ConfigureAwait(false);
				}
			}
		}

		[IdentifiedTest("65917e62-2387-4b1e-afee-721bac33b1c0")]
		[RetryOnError]
		[NUnit.Framework.Category(TestCategory.SMOKE)]
		public async Task FieldMapping_ShouldAutoMapFieldsFromSavedSearch_WhenAutoMapSavedSearchIsPressed()
		{
			//Arrange
			const string savedSearchName = "Saved Search Orzela 7";
			const string controlNumberFieldName = "Control Number";
			const string fileNameFieldName = "File Name";

			List<string> savedSearchMappableFields = new List<string>()
			{
				controlNumberFieldName,
				fileNameFieldName
			};

			await SourceContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
			await DestinationContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);

			List<string> expectedSourceMappedFields = SourceContext
				.WorkspaceMappableFields
				.Where(x => savedSearchMappableFields.Exists(fieldName => fieldName == x.Name))
				.Select(x => x.DisplayName)
				.ToList();

			List<string> expectedDestinationMappedFields = DestinationContext
				.WorkspaceMappableFields
				.Where(x => savedSearchMappableFields.Exists(fieldName => fieldName == x.Name))
				.Select(x => x.DisplayName)
				.ToList();

			await SavedSearch.CreateSavedSearchAsync(SourceContext.GetWorkspaceId(), savedSearchName, new[]
			{
				new FieldRef(fileNameFieldName)
			}).ConfigureAwait(false);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.FieldMapping = null;
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;
			model.SavedSearch = savedSearchName;

			PushToRelativityThirdPage fieldMappingPage = PointsAction.CreateNewRelativityProviderFieldMappingPage(model);

			//Act
			fieldMappingPage.MapFieldsFromSavedSearch();

			//Assert
			List<string> fieldsFromSelectedSourceWorkspaceListBox = fieldMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();
			List<string> fieldsFromSelectedDestinationWorkspaceListBox = fieldMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();

			fieldsFromSelectedSourceWorkspaceListBox.Should().ContainInOrder(expectedSourceMappedFields);
			fieldsFromSelectedDestinationWorkspaceListBox.Should().ContainInOrder(expectedDestinationMappedFields);
		}

		private List<string> CreateFieldMapListBoxFormatFromObjectManagerFetchedList(List<FieldObject> mappableFieldsListFromObjectManager)
		{
			return SyncFieldMapResults
				.SortFieldObjects(mappableFieldsListFromObjectManager)
				.Select(f => f.DisplayName).ToList();
		}

		private bool CompareFieldDisplayNamePair(FieldDisplayNamePair first, FieldDisplayNamePair second)
		{
			return
				first.DestinationDisplayName == second.DestinationDisplayName &&
				first.SourceDisplayName == second.SourceDisplayName;
		}
    }
}