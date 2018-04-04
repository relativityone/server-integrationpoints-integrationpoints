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
		protected IntegrationPointsAction PointsAction { get; set; }
		protected IFolderManager FolderManager { get; set; }
		protected INativesService NativesService { get; set; }
		protected IImagesService ImageService { get; set; }

		protected override void ContextSetUp()
		{
			Context.ExecuteRelativityFolderPathScript();
		}

		[OneTimeSetUp]
		public virtual void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();

			FolderManager = Context.Helper.CreateAdminProxy<IFolderManager>();
			NativesService = new NativesService(Context.Helper);
			ImageService = new ImagesService(Context.Helper);
			PointsAction = new IntegrationPointsAction(Driver, Context);
		}

		[SetUp]
		public virtual void SetUp()
		{
			DestinationContext = new TestContext()
				.CreateWorkspace();
		}

		[TearDown]
		public virtual void TearDown()
		{
			DestinationContext?.TearDown();
		}

		protected DocumentsValidator CreateDocumentsEmptyValidator()
		{
			return new PushDocumentsValidator(Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId());
		}

		protected DocumentsValidator CreateOnlyDocumentsWithImagesValidator()
		{
			return new PushOnlyWithImagesValidator(Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId());
		}

		protected DocumentsValidator CreateDocumentsForFieldValidator()
		{
			return new PushDocumentsValidator(Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForField(DestinationContext.GetWorkspaceId(), FolderManager));
		}

		protected DocumentsValidator CreateDocumentsForFolderTreeValidator()
		{
			return new PushDocumentsValidator(Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForFolderTree(Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), FolderManager));
		}

		protected DocumentsValidator CreateDocumentsForRootValidator()
		{
			return new PushDocumentsValidator(Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForRoot(DestinationContext.GetWorkspaceId(), FolderManager));
		}

		protected DocumentsValidator CreateDocumentsForRootWithFolderNameValidator()
		{
			return new PushDocumentsValidator(Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(DocumentPathValidator.CreateForRoot(DestinationContext.GetWorkspaceId(), FolderManager, "NATIVES"));
		}
	}
}