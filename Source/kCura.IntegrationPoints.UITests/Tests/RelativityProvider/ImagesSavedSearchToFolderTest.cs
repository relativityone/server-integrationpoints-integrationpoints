using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class ImagesSavedSearchToFolderTest : UiTest
	{
		private TestContext _destinationContext = null;
		private IntegrationPointsAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
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

		private RelativityProviderModel CreateRelativityProviderModelWithImages()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name);
			model.Source = "Saved Search";
			model.RelativityInstance = "This Instance";
			model.DestinationWorkspace = $"{_destinationContext.WorkspaceName} - {_destinationContext.WorkspaceId}";
			model.CopyImages = true;
			return model;
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_IMG_1()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			//Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		public void RelativityProvider_TC_RTR_IMG_2()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		public void RelativityProvider_TC_RTR_IMG_3()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_IMG_4()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_IMG_5()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_IMG_6()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		//ToDo: We need to have production prepared before running those tests to fill in production precedence
		//[Test, Order(10)]
		//public void RelativityProvider_TC_RTR_IMG_7()
		//{
		//	// Arrange
		//	_destinationContext.ImportDocuments();

		//	RelativityProviderModel model = CreateRelativityProviderModelWithImages();
		//	model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
		//	model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.ProducedImages;
		//	model.CopyFilesToRepository = false;

		//	// Act
		//	IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
		//	PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

		//	// Assert
		//	Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]); 
		//}

		//[Test, Order(10)]
		//public void RelativityProvider_TC_RTR_IMG_8()
		//{
		//	// Arrange
		//	_destinationContext.ImportDocuments();

		//	RelativityProviderModel model = CreateRelativityProviderModelWithImages();
		//	model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
		//	model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.ProducedImages;
		//	model.CopyFilesToRepository = true;

		//	// Act
		//	IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
		//	PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

		//	// Assert
		//	Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		//}

		//[Test, Order(10)]
		//public void RelativityProvider_TC_RTR_IMG_9()
		//{
		//	// Arrange
		//	_destinationContext.ImportDocuments();

		//	RelativityProviderModel model = CreateRelativityProviderModelWithImages();
		//	model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
		//	model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.ProducedImages;
		//	model.CopyFilesToRepository = false;

		//	// Act
		//	IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
		//	PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

		//	// Assert
		//	Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		//}


		//[Test, Order(10)]
		//public void RelativityProvider_TC_RTR_IMG_10()
		//{
		//	// Arrange
		//	_destinationContext.ImportDocuments();

		//	RelativityProviderModel model = CreateRelativityProviderModelWithImages();
		//	model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
		//	model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.ProducedImages;
		//	model.CopyFilesToRepository = true;

		//	// Act
		//	IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
		//	PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

		//	// Assert
		//	Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		//}

		//[Test, Order(10)]
		//public void RelativityProvider_TC_RTR_IMG_11()
		//{
		//	// Arrange
		//	_destinationContext.ImportDocuments();

		//	RelativityProviderModel model = CreateRelativityProviderModelWithImages();
		//	model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
		//	model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.ProducedImages;
		//	model.CopyFilesToRepository = false;
		//	// Act
		//	IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
		//	PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

		//	// Assert
		//	Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		//}

		//[Test, Order(10)]
		//public void RelativityProvider_TC_RTR_IMG_12()
		//{
		//	// Arrange
		//	_destinationContext.ImportDocuments();

		//	RelativityProviderModel model = CreateRelativityProviderModelWithImages();
		//	model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
		//	model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.ProducedImages;
		//	model.CopyFilesToRepository = true;

		//	// Act
		//	IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
		//	PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

		//	// Assert
		//	Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		//}


		//[Test, Order(10)]
		//public void RelativityProvider_TC_RTR_IMG_13()
		//{
		//	// Arrange
		//	_destinationContext.ImportDocuments();

		//	RelativityProviderModel model = CreateRelativityProviderModelWithImages();
		//	model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
		//	model.ImagePrecedence = RelativityProviderModel.ImagePrecedenceEnum.ProducedImages;
		//	model.CopyFilesToRepository = true;

		//	// Act
		//	IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
		//	PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

		//	// Assert
		//	Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		//}

	}

}
