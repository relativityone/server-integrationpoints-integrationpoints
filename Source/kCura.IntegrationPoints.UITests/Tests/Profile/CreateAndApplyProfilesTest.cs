using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.Profile
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.EXPORT_TO_RELATIVITY)]
	[Category(TestCategory.PROFILE)]
	internal class CreateAndApplyProfilesTest : RelativityProviderTestsBase
	{
		private IntegrationPointProfileAction _profileAction;

		private static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
			new Tuple<string, string>("Extracted Text", "Extracted Text"),
			new Tuple<string, string>("Title", "Title")
		};

		private RelativityProviderModel CreateRelativityProviderModel(
			string name = null,
			RelativityProviderModel.CopyNativeFilesEnum copyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No)
		{
			RelativityProviderModel model = new RelativityProviderModel(name ?? TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyNativeFiles = copyNativeFiles,
				FieldMapping = DefaultFieldsMapping
			};

			return model;
		}

		[SetUp]
		public override async Task SetUp()
		{
			await base.SetUp().ConfigureAwait(false);
			_profileAction = new IntegrationPointProfileAction(Driver, SourceContext);
		}

		[IdentifiedTest("77d6c730-3f8f-4d18-83e4-640b09e16a75")]
		[RetryOnError]
		public void Profile_ShouldCreateProfileFromExistingIPAndIPFromThisProfile()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			IntegrationPointDetailsPage integrationPointDetailsPage =
				PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			integrationPointDetailsPage.SaveAsAProfileIntegrationPoint();

			string profileModelName = "Created From Profile";
			IntegrationPointGeneralModel profileModel = new IntegrationPointGeneralModel(profileModelName);
			profileModel.DestinationProvider = "Relativity";
			profileModel.Profile = model.Name;

			//Act
			IntegrationPointDetailsPage integrationPointCreatedFromProfileDetailsPage =
				PointsAction.CreateNewRelativityProviderIntegrationPointFromProfile(profileModel);

			//Assert
			PropertiesTable generalProperties =
				integrationPointCreatedFromProfileDetailsPage.SelectGeneralPropertiesTable();
			RelativityProviderModel expectedModel = CreateRelativityProviderModel(profileModelName);
			SavedSearchToFolderValidator validator = new SavedSearchToFolderValidator();
			validator.ValidateSummaryPage(generalProperties, expectedModel, SourceContext, DestinationContext, false);
		}

		[IdentifiedTest("e4d45cc5-3b75-405d-b6e0-caf151136d02")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		public void Profile_ShouldCreateNewProfileAndIPFromThisProfile()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel(TestContext.CurrentContext.Test.Name + @"_!@$^&()-+= {};',.~`");
			_profileAction.CreateNewRelativityProviderIntegrationPointProfile(model);

			string profileModelName = "Created From Profile";
			IntegrationPointGeneralModel profileModel = new IntegrationPointGeneralModel(profileModelName)
			{
				DestinationProvider = "Relativity",
				Profile = model.Name
			};

			//Act
			IntegrationPointDetailsPage integrationPointDetailsPage =
				PointsAction.CreateNewRelativityProviderIntegrationPointFromProfile(profileModel);

			//Assert
			PropertiesTable generalProperties = integrationPointDetailsPage.SelectGeneralPropertiesTable();
			RelativityProviderModel expectedModel = CreateRelativityProviderModel(profileModelName);
			SavedSearchToFolderValidator validator = new SavedSearchToFolderValidator();
			validator.ValidateSummaryPage(generalProperties, expectedModel, SourceContext, DestinationContext, false);
		}

		[IdentifiedTest("b5da0a5e-8720-46c0-a857-656752aa3f34")]
		[RetryOnError]
		public void Profile_ShouldCreateNewProfileWithLinksAndIPFromThisProfile()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel(copyNativeFiles: RelativityProviderModel.CopyNativeFilesEnum.LinksOnly);
			_profileAction.CreateNewRelativityProviderIntegrationPointProfile(model);

			const string profileModelName = "Created From Profile";
			IntegrationPointGeneralModel profileModel = new IntegrationPointGeneralModel(profileModelName)
			{
				DestinationProvider = "Relativity",
				Profile = model.Name
			};

			//Act
			IntegrationPointDetailsPage integrationPointDetailsPage =
				PointsAction.CreateNewRelativityProviderIntegrationPointFromProfile(profileModel);

			//Assert
			PropertiesTable generalProperties = integrationPointDetailsPage.SelectGeneralPropertiesTable();
			RelativityProviderModel expectedModel = CreateRelativityProviderModel(profileModelName, RelativityProviderModel.CopyNativeFilesEnum.LinksOnly);
			SavedSearchToFolderValidator validator = new SavedSearchToFolderValidator();
			validator.ValidateSummaryPage(generalProperties, expectedModel, SourceContext, DestinationContext, false);
		}
	}
}