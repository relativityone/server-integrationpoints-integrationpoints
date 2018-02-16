using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using Relativity.Services.Folder;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class MetadataSavedSearchToFolderTest: UiTest
	{
		private TestContext _destinationContext;
		private IntegrationPointsAction _integrationPointsAction;
		private IFolderManager _folderManager;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();

			_folderManager = Context.Helper.CreateAdminProxy<IFolderManager>();
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}


		[SetUp]
		public void SetUp()
		{
			_destinationContext = new TestContext()
			.CreateWorkspace(); 
		}

		[TearDown]
		public void TearDown()
		{ 
			_destinationContext?.TearDown();
		}

		private RelativityProviderModel CreateRelativityProviderModel()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name)
			{
				Source = "Saved Search",
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{_destinationContext.WorkspaceName} - {_destinationContext.WorkspaceId}",
				CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No,
				FieldMapping = DefaultFieldsMapping
			};

			return model;
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_01()
		{
			//Arrange
			DocumentsValidator documentsValidator =
				new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
					.ValidateWith(DocumentPathValidator.CreateForRoot(_destinationContext.GetWorkspaceId(), _folderManager))
					.ValidateWith(new DocumentNativesValidator(false));

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();

			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_02()
		{
			//Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForField(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_03()
		{
			//Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForFolderTree(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_04()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForRoot(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_05()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForRoot(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_06()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForRoot(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_07()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForField(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]); 
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_08()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForFolderTree(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();
			
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_09()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForRoot(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test]
		public void RelativityProvider_TC_RTR_MDO_10()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForRoot(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_11()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForRoot(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_12()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForField(_destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));


			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test]
		public void RelativityProvider_TC_RTR_MDO_13()
		{
			// Arrange
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForFolderTree(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesValidator(false));

			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true; 

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}
	}

}
