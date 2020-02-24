using System;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.UITests.Configuration.Helpers;
using kCura.Relativity.Client;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects.DataContracts;
using ArtifactType = Relativity.ArtifactType;

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
		protected IFieldManager SourceFieldManager { get; set; }
		protected IFieldManager DestinationFieldManager { get; set; }
		protected INativesService NativesService { get; set; }
		protected IImagesService ImageService { get; set; }
		protected IProductionImagesService ProductionImageService { get; set; }
		protected IRelativityObjectManagerFactory ObjectManagerFactory { get; set; }

		[OneTimeSetUp]
		public virtual async Task OneTimeSetUp()
		{
			SourceContext.ExecuteRelativityFolderPathScript();
			FolderManager = SourceContext.Helper.CreateProxy<IFolderManager>();
			SourceFieldManager = SourceContext.Helper.CreateProxy<IFieldManager>();
			NativesService = new NativesService(SourceContext.Helper);
			ImageService = new ImagesService(SourceContext.Helper);
			ProductionImageService = new ProductionImagesService(SourceContext.Helper);
			ObjectManagerFactory = new RelativityObjectManagerFactory(SourceContext.Helper);
            await CreateFixedLengthFieldsWithSpecialCharactersAsync(SourceContext.GetWorkspaceId(), SourceFieldManager).ConfigureAwait(false);
			await SourceContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
		}

		[SetUp]
		public virtual async Task SetUp()
		{
			DestinationContext = new TestContext().CreateTestWorkspace();
			DestinationFieldManager = DestinationContext.Helper.CreateProxy<IFieldManager>();
			await CreateFixedLengthFieldsWithSpecialCharactersAsync(DestinationContext.GetWorkspaceId(), DestinationFieldManager).ConfigureAwait(false);
			await DestinationContext.RetrieveMappableFieldsAsync().ConfigureAwait(false);
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

        protected async Task CreateFixedLengthFieldsWithSpecialCharactersAsync(int workspaceID, IFieldManager fieldManager)
        {
            char[] specialCharacters = @"!@#$%^&*()-_+= {}|\/;'<>,.?~`".ToCharArray();
            for (int i = 0; i < specialCharacters.Length; i++)
            {
                char special = specialCharacters[i];
                string generatedFieldName = $"aaaaa{special}{i}";
                var fixedLengthTextFieldRequest = new FixedLengthFieldRequest
                {
                    ObjectType = new ObjectTypeIdentifier {ArtifactTypeID = (int) ArtifactType.Document},
                    Name = $"{generatedFieldName} FLT",
                    Length = 255
				};

				var longTextFieldRequest = new LongTextFieldRequest
				{
					ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
					Name = $"{generatedFieldName} LTF"
				};

                await fieldManager.CreateLongTextFieldAsync(workspaceID, longTextFieldRequest).ConfigureAwait(false);
                await fieldManager.CreateFixedLengthFieldAsync(workspaceID, fixedLengthTextFieldRequest).ConfigureAwait(false);
            }
            Guid randomNumber = Guid.NewGuid();
			var longTextRandomNameFieldRequest = new LongTextFieldRequest
			{
				ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
                Name = $"{randomNumber} LTF"
			};
            await fieldManager.CreateLongTextFieldAsync(workspaceID, longTextRandomNameFieldRequest).ConfigureAwait(false);
		}
        public async Task<FieldObject> GetFieldObjectFromWorkspaceAsync(string fieldName, TestContext workspaceContext)//change name
        {
            QueryRequest fieldsRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
                Condition = $"'Object Type Artifact Type ID' == 10 AND 'Name' == '{fieldName}'",
                Fields = new[]
                {
                    new FieldRef {Name = "Name"},
                    new FieldRef {Name = "Field Type"},
                    new FieldRef {Name = "Length"},
                    new FieldRef {Name = "Keywords"},
                    new FieldRef {Name = "Is Identifier"},
                    new FieldRef {Name = "Open To Associations"}
				}
            };

            ResultSet<RelativityObject> foundField =
                await workspaceContext.ObjectManager.QueryAsync(fieldsRequest, 0, 50).ConfigureAwait(false);
            RelativityObject firstFoundField = foundField.Items.First();
            
			return new FieldObject(firstFoundField);
        }

        public async Task SetRandomNameToFLTFieldAsync(string fieldName, TestContext workspaceContext, IFieldManager workspaceFieldManager)
        {
            string newRandomFieldName = GetRandomName(fieldName);

            FieldObject fieldToBeChanged = await GetFieldObjectFromWorkspaceAsync(fieldName, workspaceContext).ConfigureAwait(false);
            var fixedLengthTextFieldUpdateRequest = new FixedLengthFieldRequest
            {
                ObjectType = new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Document },
                Name = $"{newRandomFieldName}",
                Length = fieldToBeChanged.Length
            };
            await workspaceFieldManager
				.UpdateFixedLengthFieldAsync(workspaceContext.GetWorkspaceId(), fieldToBeChanged.ArtifactID,
                    fixedLengthTextFieldUpdateRequest).ConfigureAwait(false);
        }

        private string GetRandomName(string fieldName)
        {
            const int _NAME_MAX_LENGTH = 49;
            string randomName = $"{fieldName}" + Guid.NewGuid();
            return randomName.Substring(0, randomName.Length <= _NAME_MAX_LENGTH ? randomName.Length : _NAME_MAX_LENGTH);
        }


		public async Task SetRandomNameToFLTFieldSourceWorkspaceAsync(string fieldName)
        {
            await SetRandomNameToFLTFieldAsync(fieldName, SourceContext, SourceFieldManager).ConfigureAwait(false);
        }

        public async Task SetRandomNameToFLTFieldDestinationWorkspaceAsync(string fieldName)
        {
            await SetRandomNameToFLTFieldAsync(fieldName, DestinationContext, DestinationFieldManager).ConfigureAwait(false);
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