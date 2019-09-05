using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	using Common;
	using global::Relativity.Services.Folder;
	using IntegrationPoint.Tests.Core.TestHelpers;
	using IntegrationPoint.Tests.Core.Validators;
	using NUnit.Framework;
	using TestContext = Configuration.TestContext;

	public class RelativityProviderTestsBase : UiTest
	{
		protected TestContext DestinationContext { get; set; }
		protected IntegrationPointsAction PointsAction { get; private set; }
		protected IFolderManager FolderManager { get; set; }
		protected INativesService NativesService { get; set; }
		protected IImagesService ImageService { get; set; }
		protected IProductionImagesService ProductionImageService { get; set; }
		protected IRelativityObjectManagerFactory ObjectManagerFactory { get; set; }

		[OneTimeSetUp]
		public virtual void OneTimeSetUp()
		{
			SourceContext.ExecuteRelativityFolderPathScript();
			FolderManager = SourceContext.Helper.CreateProxy<IFolderManager>();
			NativesService = new NativesService(SourceContext.Helper);
			ImageService = new ImagesService(SourceContext.Helper);
			ProductionImageService = new ProductionImagesService(SourceContext.Helper);
			ObjectManagerFactory = new RelativityObjectManagerFactory(SourceContext.Helper);
		}

		[SetUp]
		public virtual void SetUp()
		{
			DestinationContext = new TestContext().CreateTestWorkspace();
			PointsAction = new IntegrationPointsAction(Driver, SourceContext);
		}

		[TearDown]
		public void TearDownDestinationContext()
		{
			if (DestinationContext != null)
			{
				if (string.IsNullOrEmpty(SharedVariables.UiUseThisExistingWorkspace))
				{
					Workspace.DeleteWorkspace(DestinationContext.GetWorkspaceId());
				}

				DestinationContext.TearDown();
			}
		}

		protected DocumentsValidator CreateDocumentsEmptyValidator()
		{
			return new PushDocumentsValidator(SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId());
		}

		protected DocumentsValidator CreateOnlyDocumentsWithImagesValidator()
		{
			return new PushOnlyWithImagesValidator(SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId());
		}

		protected DocumentsValidator CreateDocumentsForFieldValidator()
		{
			return new PushDocumentsValidator(SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForField(DestinationContext.GetWorkspaceId(), FolderManager));
		}

		protected DocumentsValidator CreateDocumentsForFolderTreeValidator()
		{
			DocumentPathValidator documentPathValidator = DocumentPathValidator.CreateForFolderTree(
				SourceContext.GetWorkspaceId(),
				DestinationContext.GetWorkspaceId(),
				FolderManager);
			return new PushDocumentsValidator(SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(documentPathValidator);
		}

		protected DocumentsValidator CreateDocumentsForRootValidator()
		{
			DocumentPathValidator documentPathValidator = DocumentPathValidator.CreateForRoot(
				DestinationContext.GetWorkspaceId(),
				FolderManager);
			return new PushDocumentsValidator(SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(documentPathValidator);
		}

		protected DocumentsValidator CreateDocumentsForRootWithFolderNameValidator()
		{
			DocumentPathValidator documentPathValidator = DocumentPathValidator.CreateForRoot(
				DestinationContext.GetWorkspaceId(),
				FolderManager,
				"NATIVES");
			return new PushDocumentsValidator(SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(documentPathValidator);
		}
	}
}