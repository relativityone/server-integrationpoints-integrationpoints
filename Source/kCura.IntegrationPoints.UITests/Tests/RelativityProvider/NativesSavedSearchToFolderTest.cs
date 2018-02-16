using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
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
	public class NativesSavedSearchToFolderTest : UiTest
	{
		private TestContext _destinationContext = null;
		private IntegrationPointsAction _integrationPointsAction;
		private IFolderManager _folderManager;
		private INativesService _nativesService;

		protected override void ContextSetUp()
		{
			Context.ExecuteRelativityFolderPathScript();
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();

			_folderManager = Context.Helper.CreateAdminProxy<IFolderManager>();
			_nativesService = new NativesService(Context.Helper);
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

		private RelativityProviderModel CreateRelativityProviderModelWithNatives()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name);
			model.Source = "Saved Search";
			model.RelativityInstance = "This Instance";
			model.DestinationWorkspace = $"{_destinationContext.WorkspaceName} - {_destinationContext.WorkspaceId}";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles;
			return model;
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			//TODO add root validation
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_03()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_04()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			//TODO add root validation
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_05()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_06()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_07()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_08()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_09()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			//TODO add root validation
		}


		[Test]
		public void RelativityProvider_TC_RTR_NF_10()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_11()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		[Test]
		public void RelativityProvider_TC_RTR_NF_12()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}


		[Test]
		public void RelativityProvider_TC_RTR_NF_13()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true; 

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		private void ValidateDocumentsForField()
		{
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForField(Context.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesAndInRepositoryValidator(_nativesService, Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), true, true));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForFolderTree()
		{
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForFolderTree(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), _folderManager))
				.ValidateWith(new DocumentNativesAndInRepositoryValidator(_nativesService, Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), true, true));

			documentsValidator.Validate();
		}
	}
}
