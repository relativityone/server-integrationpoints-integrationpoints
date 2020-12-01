using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;
using kCura.IntegrationPoints.UITests.Configuration.Models;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using NUnit.Framework;
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
	[Category(TestCategory.RIP_SYNC)]
	[Category(TestCategory.FIELDS_MAPPING)]
	public class FieldsMappingTests : RelativityProviderTestsBase
	{
		private static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
			new Tuple<string, string>("Extracted Text", "Extracted Text"),
			new Tuple<string, string>("Title", "Title")
		};

		public FieldsMappingTests() : base(shouldImportDocuments: false)
		{ }

		protected override async Task SuiteSpecificOneTimeSetup()
		{
			await CreateFixedLengthFieldsWithSpecialCharactersAsync(SourceContext).ConfigureAwait(false);
			await CreateFixedLengthFieldsWithSpecialCharactersAsync(DestinationContext).ConfigureAwait(false);
			
		}

		protected override Task SuiteSpecificTearDown() => Task.CompletedTask;

		[IdentifiedTest("916e57ba-fb4d-42a4-be2a-4d17df17de57")]
		[RetryOnError]
		public async Task FieldMapping_ShouldDisplayMappableFieldsInCorrectOrderInSourceWorkspaceFieldList()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			PushToRelativityThirdPage fieldMappingPage =
				PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			List<string> fieldsFromSourceWorkspaceListBox = fieldMappingPage.GetFieldsFromSourceWorkspaceListBox();

			await SourceContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
			List<string> expectedSourceMappableFields =
				CreateFieldMapListBoxFormatFromObjectManagerFetchedList(SourceContext.WorkspaceMappableFields);

			//Assert
			CollectionAssert.AreEqual(fieldsFromSourceWorkspaceListBox, expectedSourceMappableFields);
		}

		[IdentifiedTest("916e57ba-fb4d-42a4-be2a-4d17df17de58")]
		[RetryOnError]
		public async Task FieldMapping_ShouldDisplayMappableFieldsInCorrectOrderInDestinationWorkspaceFieldList()
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
			
			await DestinationContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
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
			List<Tuple<string, string>> fieldsMapping = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("Control Number", "Control Number"),
			};
			List<Tuple<string, string>> invalidFieldsMapping = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("Alert", "Classification Index")
			};
			fieldsMapping.AddRange(invalidFieldsMapping);

			RelativityProviderModel model = CreateRelativityProviderModel();

			PushToRelativityThirdPage fieldMappingPage =
				PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			PointsAction.MapWorkspaceFields(fieldMappingPage, fieldsMapping);
			fieldMappingPage = fieldMappingPage.ClickSaveButtonExpectPopup();
			
			//Assert text on popup
			fieldMappingPage.MappedFieldsWarning.Text.Should().Be(invalidFieldMappingMessageText);

			//Act

			IntegrationPointDetailsPage detailsPage = fieldMappingPage.ClearAndProceedOnInvalidMapping();

			PushToRelativityThirdPage clearedMappingPage = PointsAction.EditGoToFieldMappingPage(detailsPage);
			List<string> fieldsFromSelectedSourceWorkspaceListBox =
				clearedMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();
			List<string> fieldsFromSelectedDestinationWorkspaceListBox =
				clearedMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();

			//Assert if fields were removed from mapping
			fieldsFromSelectedDestinationWorkspaceListBox.Should().NotContain(invalidFieldsMapping.Select(x => x.Item1));
			fieldsFromSelectedSourceWorkspaceListBox.Should().NotContain(invalidFieldsMapping.Select(x => x.Item2));
		}

		[IdentifiedTest("6dec5ade-4c72-4360-a364-e190125b01e6")]
		[RetryOnError]
		public void FieldMapping_ShouldDisplayInvalidFieldsMappingReasons_WhenClickedSavedButton()
		{
			//Arrange
			List<Tuple<string, string>> fieldsMapping = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("Control Number", "Control Number"),
			};
			string sourceField1Name = "Alert";
			string destinationField1Name = "Classification Index"; 
			
			string sourceField2Name = "aaaaa-10 FLT";
			string destinationField2Name = "MD5 Hash";
			
			string sourceField3Name = "Imaging Set";
			string destinationField3Name = "Imaging Set";

			string incompatibleTypesMessage = "Types are not compatible";
			string unicodeMessage = "Unicode is different";
			string unsupportedMessage = "Selected fields types might fail the job";
			
			List<Tuple<string, string>> invalidFieldsMapping = new List<Tuple<string, string>>
			{
				new Tuple<string, string>(sourceField1Name, destinationField1Name),
				new Tuple<string, string>(sourceField2Name, destinationField2Name),
				new Tuple<string, string>(sourceField3Name, destinationField3Name)
			};
			fieldsMapping.AddRange(invalidFieldsMapping);

			RelativityProviderModel model = CreateRelativityProviderModel();

			PushToRelativityThirdPage fieldMappingPage =
				PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			PointsAction.MapWorkspaceFields(fieldMappingPage, fieldsMapping);
			
			//Act
			fieldMappingPage = fieldMappingPage.ClickSaveButtonExpectPopup();

			//Assert
			fieldMappingPage.InvalidMap0WebElement.Text.Should().Contain(sourceField1Name);
			fieldMappingPage.InvalidMap0WebElement.Text.Should().Contain(destinationField1Name);
			fieldMappingPage.InvalidReasons00WebElement.Text.Should().Contain(incompatibleTypesMessage);

			fieldMappingPage.InvalidMap1WebElement.Text.Should().Contain(sourceField2Name);
			fieldMappingPage.InvalidMap1WebElement.Text.Should().Contain(destinationField2Name);
			fieldMappingPage.InvalidReasons10WebElement.Text.Should().Contain(incompatibleTypesMessage);
			fieldMappingPage.InvalidReasons11WebElement.Text.Should().Contain(unicodeMessage);

			fieldMappingPage.InvalidMap2WebElement.Text.Should().Contain(sourceField3Name);
			fieldMappingPage.InvalidMap2WebElement.Text.Should().Contain(destinationField3Name);
			fieldMappingPage.InvalidReasons20WebElement.Text.Should().Contain(unsupportedMessage);

		}

		[IdentifiedTest("4a6bbec9-24f5-421d-8233-ffbf9824c371")]
		[RetryOnError]
		public void FieldMapping_ShouldClearMapFromInvalidFieldExceptObjectIdentifier_WhenClearButtonIsPressed()
		{
			//Arrange
			const string invalidFieldMappingMessageText = "Mapping of the fields below may fail your job:";

			List<Tuple<string, string>> fieldsMapping = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("Control Number", "Control Number"),
				};
			List<Tuple<string, string>> invalidFieldsMapping = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("Alert", "Classification Index")
				};
			fieldsMapping.AddRange(invalidFieldsMapping);

			RelativityProviderModel model = CreateRelativityProviderModel();

			PushToRelativityThirdPage fieldMappingPage = PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
			PointsAction.MapWorkspaceFields(fieldMappingPage, fieldsMapping);
			fieldMappingPage = fieldMappingPage.ClickSaveButtonExpectPopup();

			//Assert text on popup

			fieldMappingPage.ObjectIdentifierWarningOrNull.Should().BeNull();
			fieldMappingPage.MappedFieldsWarning.Text.Should().Be(invalidFieldMappingMessageText);

			//Act
			IntegrationPointDetailsPage detailsPage = fieldMappingPage.ClearAndProceedOnInvalidMapping();

			PushToRelativityThirdPage clearedMappingPage = PointsAction.EditGoToFieldMappingPage(detailsPage);
			List<string> fieldsFromSelectedSourceWorkspaceListBox = clearedMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();
			List<string> fieldsFromSelectedDestinationWorkspaceListBox = clearedMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();

			//Assert if fields were removed from mapping
			fieldsFromSelectedDestinationWorkspaceListBox.Should().OnlyContain(x => x == "Control Number [Object Identifier]");
			fieldsFromSelectedSourceWorkspaceListBox.Should().OnlyContain(x => x == "Control Number [Object Identifier]");

			fieldsFromSelectedDestinationWorkspaceListBox.Should().NotContain(invalidFieldsMapping.Select(x => x.Item1));
			fieldsFromSelectedSourceWorkspaceListBox.Should().NotContain(invalidFieldsMapping.Select(x => x.Item2));
		}

		[IdentifiedTest("8a191344-a730-11ea-bb37-0242ac130002")]
		[RetryOnError]
		public async Task FieldMapping_ShouldClearMapFromInvalidFieldExceptObjectIdentifier_WhenObjectIdentifierInDestinationTooShort_AndClearButtonIsPressed()
		{
			try
			{
				//Arrange
				await ResizeControlNumberInDestinationAsync(50).ConfigureAwait(false);

				string objectIdentifierWarningText = "The Source Maximum Length of the Object Identifier is greater than the one in Destination.\r\n" +
				                                     "If you want to adjust it click Cancel, if not click Proceed to continue with current mapping.";
				const string invalidFieldMappingMessageText = "Mapping of the fields below may fail your job:";

				List<Tuple<string, string>> fieldsMapping = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("Control Number", "Control Number"),
				};
				List<Tuple<string, string>> invalidFieldsMapping = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("Alert", "Classification Index")
				};
				fieldsMapping.AddRange(invalidFieldsMapping);

				RelativityProviderModel model = CreateRelativityProviderModel();

				PushToRelativityThirdPage fieldMappingPage = PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
				PointsAction.MapWorkspaceFields(fieldMappingPage, fieldsMapping);
				fieldMappingPage = fieldMappingPage.ClickSaveButtonExpectPopup();

				//Assert text on popup
				fieldMappingPage.ObjectIdentifierWarning.Text.Should().Be(objectIdentifierWarningText);
				fieldMappingPage.MappedFieldsWarning.Text.Should().Be(invalidFieldMappingMessageText);

				//Act
				IntegrationPointDetailsPage detailsPage = fieldMappingPage.ClearAndProceedOnInvalidMapping();

				PushToRelativityThirdPage clearedMappingPage = PointsAction.EditGoToFieldMappingPage(detailsPage);
				List<string> fieldsFromSelectedSourceWorkspaceListBox = clearedMappingPage.GetFieldsFromSelectedSourceWorkspaceListBox();
				List<string> fieldsFromSelectedDestinationWorkspaceListBox = clearedMappingPage.GetFieldsFromSelectedDestinationWorkspaceListBox();

				//Assert if fields were removed from mapping
				fieldsFromSelectedDestinationWorkspaceListBox.Should().OnlyContain(x => x == "Control Number [Object Identifier]");
				fieldsFromSelectedSourceWorkspaceListBox.Should().OnlyContain(x => x == "Control Number [Object Identifier]");

				fieldsFromSelectedDestinationWorkspaceListBox.Should().NotContain(invalidFieldsMapping.Select(x => x.Item1));
				fieldsFromSelectedSourceWorkspaceListBox.Should().NotContain(invalidFieldsMapping.Select(x => x.Item2));

			}
			finally
			{
				await ResizeControlNumberInDestinationAsync(255).ConfigureAwait(false);
			}
		}

		[IdentifiedTest("8ad36b14-67df-4b0c-8e09-5a13bdf835c0")]
		[RetryOnError]
		public async Task FieldMapping_ShouldDisplayWarning_WhenObjectIdentifierInDestinationTooShort()
		{
			try
			{
				//Arrange
				await ResizeControlNumberInDestinationAsync(50).ConfigureAwait(false);

				string objectIdentifierWarningText = "The Source Maximum Length of the Object Identifier is greater than the one in Destination.\r\n" +
				                                     "If you want to adjust it click Cancel, if not click Proceed to continue with current mapping.";

				List<Tuple<string, string>> fieldsMapping = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("Control Number", "Control Number"),
				};

				RelativityProviderModel model = CreateRelativityProviderModel();

				PushToRelativityThirdPage fieldMappingPage = PointsAction.CreateNewRelativityProviderFieldMappingPage(model);
				PointsAction.MapWorkspaceFields(fieldMappingPage, fieldsMapping);
				fieldMappingPage = fieldMappingPage.ClickSaveButtonExpectPopup();

				//Assert
				fieldMappingPage.ObjectIdentifierWarning.Text.Should().Be(objectIdentifierWarningText);
				fieldMappingPage.MappedFieldsWarningOrNull.Should().BeNull();
				fieldMappingPage.ClearAndProceedBtnOrNull.Should().BeNull();
			}
			finally
			{
				await ResizeControlNumberInDestinationAsync(255).ConfigureAwait(false);
			}
		}

		[IdentifiedTest("ea443f17-ee72-40f5-9950-7934374ff5c0")]
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
		[Category(TestCategory.SMOKE)]
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
				SyncFieldMapResults mapAllFieldsUiTestEdition = new SyncFieldMapResults(SourceContext.WorkspaceAutoMapAllEnabledFields, DestinationContext.WorkspaceAutoMapAllEnabledFields);

				List<string> expectedInOrderSelectedSourceMappableFieldsList =
					mapAllFieldsUiTestEdition.FieldMapSorted.Select(x => x.SourceFieldObject.DisplayName).ToList();

				FieldMapModel expectedIdentifierMatchedField =
					mapAllFieldsUiTestEdition.FieldMap.Single(x =>
						x.AutoMapMatchType == TestConstants.FieldMapMatchType.IsIdentifier);
				var expectedFieldPairIsIdentifier = new FieldDisplayNamePair(expectedIdentifierMatchedField);

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

				//Fields with the same name should be mapped
				foreach (FieldDisplayNamePair fieldDisplayNamePair in expectedFieldPairsNameList)
				{
					Assert.IsTrue(fieldPairsFromSelectedListBox.Exists(x => CompareFieldDisplayNamePair(x, fieldDisplayNamePair)));
				}

				//Fields with different name but the same id should not be mapped
				foreach (FieldDisplayNamePair fieldDisplayNamePair in fieldsToBeRenamed.Select(x => new FieldDisplayNamePair(x.Item1, x.Item2)))
				{
					Assert.IsFalse(fieldPairsFromSelectedListBox.Exists(x => CompareFieldDisplayNamePair(x, fieldDisplayNamePair)));
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
		[Category(TestCategory.SMOKE)]
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

		protected async Task CreateFixedLengthFieldsWithSpecialCharactersAsync(Configuration.TestContext workspaceContext)
		{
			int workspaceID = workspaceContext.GetWorkspaceId();
			IFieldManager fieldManager = workspaceContext.Helper.CreateProxy<IFieldManager>();

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

		private Task ResizeControlNumberInDestinationAsync(int newLength)
		{
			const int controlNumberArtifactID = 1003667;
			return DestinationFieldManager
				.UpdateFixedLengthFieldAsync(DestinationContext.GetWorkspaceId(), controlNumberArtifactID, new FixedLengthFieldRequest()
				{
					ObjectType = new ObjectTypeIdentifier()
					{
						ArtifactTypeID = (int)ArtifactType.Document
					},
					Name = "Control Number",
					Length = newLength
				});
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