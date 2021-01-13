using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using kCura.Utility;
using NUnit.Framework;
using OpenQA.Selenium;
using Relativity.Services.Workspace;
using Relativity.Testing.Identification;
using DateTime = System.DateTime;

namespace kCura.IntegrationPoints.UITests.Tests.Profile
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Profiles]
	[Category(TestCategory.RIP_SYNC)]
	[Category(TestCategory.PROFILE)]
	internal class CreateAndApplyProfilesUiValuesTest : RelativityProviderTestsBase
	{
		private IntegrationPointProfileAction _profileAction;

		private readonly string _timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.ffff");
		private readonly string _sourceProductionName = $"SrcProd_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
		private readonly string _destinationProductionName = $"DestProd_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";

		public CreateAndApplyProfilesUiValuesTest() : base(false)
		{
		}

		private RelativityProviderModel CreateRelativityProviderSavedSearchModel(
			string name = null,
			RelativityProviderModel.CopyNativeFilesEnum copyNativeFiles =
				RelativityProviderModel.CopyNativeFilesEnum.No)
		{
			RelativityProviderModel model = new RelativityProviderModel(name ?? TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyNativeFiles = copyNativeFiles,
			};

			return model;
		}

		private RelativityProviderModel CreateRelativityProviderSavedSearchImagesModel()
		{
			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				SavedSearch = "All documents",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyImages = true,
				CreateSavedSearch = false,
				SourceProductionName = _sourceProductionName,
				DestinationProductionName = _destinationProductionName
			};
			return model;
		}

		protected override Task SuiteSpecificOneTimeSetup()
		{
			SourceContext.CreateProductionAndImportData(_sourceProductionName);
			return Task.CompletedTask;
		}

		protected override Task SuiteSpecificSetup() => Task.CompletedTask;
		protected override Task SuiteSpecificTearDown() => Task.CompletedTask;


		[SetUp]
		public override async Task SetUp()
		{
			await base.SetUp().ConfigureAwait(false);
			_profileAction = new IntegrationPointProfileAction(Driver, SourceContext.WorkspaceName);
		}
		
		[IdentifiedTest("666b2a65-7d30-42e2-82ae-e00bae92fba0")]
		[RetryOnError]
		[TestType.Error]
		public void Profile_ShouldDisplayWarning_WhenNameWithIllegalCharacters()
		{
			const string illegalName = "Name < > : \" \\ / | ? *TAB";
			const string expectedErrorMessage = "Field cannot contain special characters such as: < > : \" \\ / | ? * TAB";

			// Arrange & Act
			ExportFirstPage firstPage = _profileAction.GoToFirstPageIntegrationPointProfile();
			firstPage.Name = illegalName;
			firstPage.ClickNext();

			// Assert
			firstPage.GetErrorLabels().Select(e => e.Text).Single().Should().Contain(expectedErrorMessage);
		}

		[IdentifiedTest("76d69332-ba3b-4679-b71f-e1fd41f3eb3e")]
		[RetryOnError]
		[TestType.Error]
		public async Task CopyProfile_ShouldDisplayError_WhenSavedSearchIsMissing()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderSavedSearchModel();
			_profileAction.CreateNewRelativityProviderIntegrationPointProfile(model);

			string newDestination = $"RIP New destination_{_timeStamp}";
			WorkspaceRef newWorkspace = await Workspace.CreateWorkspaceAsync(newDestination, SourceContext.WorkspaceName)
				.ConfigureAwait(false);
			string profileModelName = "Created From Profile";

			IntegrationPointGeneralModel profileModel = new IntegrationPointGeneralModel(profileModelName)
			{
				DestinationProvider = model.DestinationProvider,
				Profile = model.Name
			};

			//Act
			GeneralPage generalPage = new GeneralPage(Driver).PassWelcomeScreen().ChooseWorkspace(newDestination);
			IntegrationPointProfilePage integrationPointsPage = generalPage.GoToIntegrationPointProfilePage();
			ExportFirstPage firstPage = integrationPointsPage.CreateNewIntegrationPointProfile();

			firstPage.Name = profileModel.Name;
			firstPage.Destination = profileModel.DestinationProvider;
			firstPage.ProfileObject = profileModel.Profile;

			//Assert
			const string errorMessage =
				"Issue(s) occured while loading the profile.\r\n"
				+ "20.004 Saved search is not available or has been secured from this user. Contact your system administrator. Click here for more information.";
			firstPage.PageMessageText.Should()
				.Be(errorMessage);

			//TearDown
			await Workspace.DeleteWorkspaceAsync(newWorkspace.ArtifactID).ConfigureAwait(false);
		}

		[IdentifiedTest("e4d45cc5-3b75-405d-b6e0-caf151136d02")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		[TestType.MainFlow]
		public async Task CopyProfile_ShouldDisplayCorrectValues_WhenSourceSavedSearch()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderSavedSearchModel();
			_profileAction.CreateNewRelativityProviderIntegrationPointProfile(model);

			string newDestination = $"RIP New destination {_timeStamp}";
			WorkspaceRef newWorkspace = await Workspace.CreateWorkspaceAsync(newDestination, SourceContext.WorkspaceName)
				.ConfigureAwait(false);

			//Act
			new GeneralPage(Driver)
				.PassWelcomeScreen()
				.ChooseWorkspace(newDestination)
				.GoToIntegrationPointProfilePage();

			IWebElement resultLinkLinkName = Driver.FindElementEx(By.LinkText(model.Name));
			resultLinkLinkName.ClickEx(Driver);

			IntegrationPointDetailsPage detailsPage = new IntegrationPointDetailsPage(Driver);
			ExportFirstPage firstPage = detailsPage.EditIntegrationPoint();

			//Assert
			firstPage.Name.Should().Be(model.Name);
			firstPage.Destination.Should().Be(model.DestinationProvider);

			PushToRelativitySecondPage secondPage = firstPage.GoToNextPagePush();
			secondPage.SourceSelect.Should().Be(model.SourceTypeTextUi());
			secondPage.GetSelectedSavedSearch().Trim().Should().Be(string.Empty); //empty
			secondPage.DestinationWorkspace.Should().Be(model.DestinationWorkspace);
			secondPage.FolderLocationSelectText.Should().Be(string.Empty); //this should always be empty;

			await Workspace.DeleteWorkspaceAsync(newWorkspace.ArtifactID).ConfigureAwait(false);
		}

		[IdentifiedTest("62c16b2a-ed88-419d-a4e9-252550fde0fc")]
		[RetryOnError]
		[TestType.MainFlow]
		public async Task CopyProfile_ShouldDisplayCorrectValues_WhenSourceImageAndProducedImagePrecedence()
		{
			// Arrange
			RelativityProviderModel model = CreateRelativityProviderSavedSearchImagesModel();
			model.MultiSelectFieldOverlay =
				RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;
			_profileAction.CreateNewRelativityProviderIntegrationPointProfile(model);

			string newDestination = $"RIP New destination {_timeStamp}";
			WorkspaceRef newWorkspace = await Workspace.CreateWorkspaceAsync(newDestination, SourceContext.WorkspaceName)
				.ConfigureAwait(false);

			//Act
			new GeneralPage(Driver)
				.PassWelcomeScreen()
				.ChooseWorkspace(newDestination)
				.GoToIntegrationPointProfilePage();
			IWebElement resultLinkLinkName = Driver.FindElementEx(By.LinkText(model.Name));
			resultLinkLinkName.ClickEx(Driver);

			IntegrationPointDetailsPage detailsPage = new IntegrationPointDetailsPage(Driver);
			ExportFirstPage firstPage = detailsPage.EditIntegrationPoint();

			//Assert
			PushToRelativitySecondPage secondPage = firstPage.GoToNextPagePush();

			secondPage.SelectSavedSearch(model.SavedSearch);
			secondPage.FolderLocationSelect.ChooseRootElement();
			PushToRelativityThirdPage thirdPage = secondPage.GoToNextPage();
			
			thirdPage.SelectCopyImagesYesElement.Selected.Should().BeTrue();
			thirdPage.SelectImagePrecedence.Should().Be(EnumExtensions.GetDescription(ImagePrecedence.ProducedImages));
			thirdPage.ProductionPrecedenceText.Should().Be("Select...");

			await Workspace.DeleteWorkspaceAsync(newWorkspace.ArtifactID).ConfigureAwait(false);
		}

		[IdentifiedTest("b5c2ffc1-06a6-47c7-92fb-8a13c1f87061")]
		[RetryOnError]
		[TestType.Error]
		public async Task CopyProfile_ShouldDisplayError_WhenApplyingCopiedProfileSavedSearchedFolder()
		{
			const string emptySavedSearchErrorMessage =
				"The saved search is no longer accessible. Please verify your settings or create a new Integration Point.";
			const string emptyLocationErrorMessage = "This field is required.";

			//Arrange
			RelativityProviderModel model = CreateRelativityProviderSavedSearchModel();
			_profileAction.CreateNewRelativityProviderIntegrationPointProfile(model);

			string newDestination = $"RIP New destination {_timeStamp}";

			WorkspaceRef newWorkspace = await Workspace.CreateWorkspaceAsync(newDestination, SourceContext.WorkspaceName)
				.ConfigureAwait(false);

			//Act
			new GeneralPage(Driver)
				.PassWelcomeScreen()
				.ChooseWorkspace(newDestination)
				.GoToIntegrationPointProfilePage();

			IWebElement resultLinkLinkName = Driver.FindElementEx(By.LinkText(model.Name));
			resultLinkLinkName.ClickEx(Driver);

			IntegrationPointDetailsPage detailsPage = new IntegrationPointDetailsPage(Driver);
			ExportFirstPage firstPage = detailsPage.EditIntegrationPoint();

			
			PushToRelativitySecondPage secondPage = firstPage.GoToNextPagePush();
			secondPage.GoToNextPage();
			Driver.SwitchToFrameEx("configurationFrame");

			//Assert
			List<string> listOfVisibleWarningsTexts = secondPage.listOfValidationErrorsElements.Where( i => i.GetCssValue("display").Equals("inline-block")).Select( i => i.Text).ToList();
			listOfVisibleWarningsTexts.Should().Contain(emptySavedSearchErrorMessage);
			listOfVisibleWarningsTexts.Should().Contain(emptyLocationErrorMessage);

			await Workspace.DeleteWorkspaceAsync(newWorkspace.ArtifactID).ConfigureAwait(false);
		}

	}
}
