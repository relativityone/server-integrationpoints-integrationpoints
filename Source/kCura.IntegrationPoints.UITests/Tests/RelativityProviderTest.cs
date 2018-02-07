using System;
using System.Threading;
using kCura.Injection.Behavior;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.UITests.Common;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class RelativityProviderMetadataOnlyTest : UiTest
	{
		private TestContext _context = null;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
		}


		[SetUp]
		public void SetUp()
		{
			_context = new TestContext()
				.CreateWorkspace();
		}

		[TearDown]
		public void TearDown()
		{
			_context?.TearDown();
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_1()
		{

			// Arrange
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_1";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Append Only";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "No";

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_2()
		{

			// Arrange

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_2";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Append Only";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Field";
			third.SelectReadFromField = "Document Folder Path";

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_3()
		{

			// Arrange

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_3";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Append Only";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Folder Tree";


			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_4()
		{

			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_1";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Overlay Only";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "No";

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);


		}
		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_5()
		{
			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_2";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Overlay Only";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Field";
			third.SelectReadFromField = "Document Folder Path";
			third.SelectMoveExitstinDocuments("No");

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_6()
		{

			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_3";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Overlay Only";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Folder Tree";
			third.SelectMoveExitstinDocuments("No");

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_7()
		{
			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_2";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Overlay Only";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Field";
			third.SelectReadFromField = "Document Folder Path";
			third.SelectMoveExitstinDocuments("Yes");

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_8()
		{
			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_3";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Overlay Only";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Folder Tree";
			third.SelectMoveExitstinDocuments("Yes");

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_9()
		{
			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_1";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Append/Overlay";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "No";

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);


		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_10()
		{
			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_2";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Append/Overlay";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Field";
			third.SelectReadFromField = "Document Folder Path";
			third.SelectMoveExitstinDocuments("No");

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_11()
		{

			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_3";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Append/Overlay";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Folder Tree";
			third.SelectMoveExitstinDocuments("No");

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_12()
		{

			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_2";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Append/Overlay";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Field";
			third.SelectReadFromField = "Document Folder Path";
			third.SelectMoveExitstinDocuments("Yes");

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_13()
		{
			// Arrange

			_context.ImportDocuments();

			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP TC_RTR_MDO_3";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = $"{_context.WorkspaceName} - {_context.WorkspaceId}";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Append/Overlay";
			third.SelectCopyNativeFiles("No");
			third.SelectFolderPathInfo = "Read From Folder Tree";
			third.SelectMoveExitstinDocuments("Yes");

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}
	}

}
