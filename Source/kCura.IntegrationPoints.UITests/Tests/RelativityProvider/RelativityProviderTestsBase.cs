using System.Diagnostics;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.UITests.Configuration.Models;
using Relativity.Services.Interfaces.Field;

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
		protected IntegrationPointsAction PointsAction { get; private set; }
		protected IFolderManager FolderManager { get; set; }
		protected IFieldManager SourceFieldManager { get; set; }
		protected IFieldManager DestinationFieldManager { get; set; }
		protected INativesService NativesService { get; set; }
		protected IImagesService ImageService { get; set; }
		protected IProductionImagesService ProductionImageService { get; set; }
		protected IRelativityObjectManagerFactory ObjectManagerFactory { get; set; }

		public RelativityProviderTestsBase(bool shouldImportDocuments = true) : base(shouldImportDocuments: shouldImportDocuments)
		{ }

		[OneTimeSetUp]
		public virtual async Task OneTimeSetUp()
		{
			Log.Information("One TimeSetUp");
			var sw = new Stopwatch();
			sw.Start();
			
			//SourceContext.ExecuteRelativityFolderPathScript();
			FolderManager = SourceContext.Helper.CreateProxy<IFolderManager>();
			SourceFieldManager = SourceContext.Helper.CreateProxy<IFieldManager>();
			NativesService = new NativesService(SourceContext.Helper);
			ImageService = new ImagesService(SourceContext.Helper);
			ProductionImageService = new ProductionImagesService(SourceContext.Helper);
			ObjectManagerFactory = new RelativityObjectManagerFactory(SourceContext.Helper);
			DestinationFieldManager = DestinationContext.Helper.CreateProxy<IFieldManager>();
			await SuiteSpecificOneTimeSetup().ConfigureAwait(false);
			
			sw.Stop();
			Log.Information("One TimeSetUp. Duration: {duration} s", sw.ElapsedMilliseconds/1000);
		}

		[SetUp]
		public virtual async Task SetUp()
		{
			Log.Information("Suite SetUp");
			
			await SuiteSpecificSetup().ConfigureAwait(false);
			PointsAction = new IntegrationPointsAction(Driver, SourceContext.WorkspaceName);
			Log.Information("End Suite SetUp");
		}

		protected virtual Task SuiteSpecificOneTimeSetup() => Task.CompletedTask;

		protected virtual async Task SuiteSpecificSetup()
		{
			if (DestinationContext.WorkspaceId.HasValue)
			{
				return;
			}
			DestinationContext = new TestContext();
			await DestinationContext.CreateTestWorkspaceAsync().ConfigureAwait(false);
			DestinationFieldManager = DestinationContext.Helper.CreateProxy<IFieldManager>();
		}

		[TearDown]
		public virtual Task TearDownDestinationContext()
		{
			Log.Information("TearDownDestinationContext");
			return SuiteSpecificTearDown();
		}

		protected virtual Task SuiteSpecificTearDown()
		{
			DeleteWorkspace(DestinationContext);
			return Task.CompletedTask;
		}

		public Task RenameFieldInSourceWorkspaceAsync(string fieldName, string newFieldName)
		{
			return FieldObject.RenameFieldAsync(fieldName, newFieldName, SourceContext, SourceFieldManager);
		}

        public Task RenameFieldInDestinationWorkspaceAsync(string fieldName, string newFieldName)
        {
	        return FieldObject.RenameFieldAsync(fieldName, newFieldName, DestinationContext, DestinationFieldManager);
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