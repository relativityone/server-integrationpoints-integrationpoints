using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using NUnit.Framework;
using Relativity.Services.Folder;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	using System;
	using System.Collections.Generic;

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

			var helper = new TestHelper();
			_folderManager = helper.CreateAdminProxy<IFolderManager>();
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
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name);
			model.Source = "Saved Search";
			model.RelativityInstance = "This Instance";
			model.DestinationWorkspace = $"{_destinationContext.WorkspaceName} - {_destinationContext.WorkspaceId}";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No;

			model.FieldMapping = new List<Tuple<string, string>>()
			{
				new Tuple<string, string>("Control Number", "Control Number"),
				new Tuple<string, string>("Extracted Text", "Extracted Text"),
				new Tuple<string, string>("Title", "Title"),
				new Tuple<string, string>("Date Created", "Date Created")
			};

			return model;
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_02()
		{
			//Arrange
			BaseUiValidator jobStatusValidator = new BaseUiValidator();
			DocumentPathValidator pathValidator = DocumentPathValidator.CreateForField(_destinationContext.GetWorkspaceId(), _folderManager);
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), pathValidator);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			jobStatusValidator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_03()
		{
			//Arrange
			BaseUiValidator jobStatusValidator = new BaseUiValidator();
			DocumentPathValidator pathValidator = DocumentPathValidator.CreateForFolderTree(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), _folderManager);
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), pathValidator);
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			jobStatusValidator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_04()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_05()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_06()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_07()
		{
			// Arrange
			BaseUiValidator jobStatusValidator = new BaseUiValidator();
			DocumentPathValidator pathValidator = DocumentPathValidator.CreateForField(_destinationContext.GetWorkspaceId(), _folderManager);
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), pathValidator);

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
			jobStatusValidator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]); 
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_08()
		{
			// Arrange
			BaseUiValidator jobStatusValidator = new BaseUiValidator();
			DocumentPathValidator pathValidator = DocumentPathValidator.CreateForFolderTree(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), _folderManager);
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), pathValidator);

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
			jobStatusValidator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_09()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test]
		public void RelativityProvider_TC_RTR_MDO_10()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_11()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_12()
		{
			// Arrange

			BaseUiValidator jobStatusValidator = new BaseUiValidator();
			DocumentPathValidator pathValidator = DocumentPathValidator.CreateForField(Context.GetWorkspaceId(), _folderManager);
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), pathValidator);

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
			jobStatusValidator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test]
		public void RelativityProvider_TC_RTR_MDO_13()
		{
			// Arrange
			BaseUiValidator jobStatusValidator = new BaseUiValidator();
			DocumentPathValidator pathValidator = DocumentPathValidator.CreateForFolderTree(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), _folderManager);
			DocumentsValidator documentsValidator = new PushDocumentsValidator(Context.GetWorkspaceId(), _destinationContext.GetWorkspaceId(), pathValidator);

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
			jobStatusValidator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			documentsValidator.Validate();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

	}

}
