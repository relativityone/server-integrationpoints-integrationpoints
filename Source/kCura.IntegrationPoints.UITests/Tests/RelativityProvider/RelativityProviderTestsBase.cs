﻿using System;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.Relativity.Client;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
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
            await CreateFixedLengthFieldsWithSpecialCharacters(SourceContext.GetWorkspaceId(), SourceFieldManager).ConfigureAwait(false);
			await SourceContext.RetrieveMappableFields().ConfigureAwait(false);
		}

		[SetUp]
		public virtual async Task SetUp()
		{
			DestinationContext = new TestContext().CreateTestWorkspace();
			DestinationFieldManager = DestinationContext.Helper.CreateProxy<IFieldManager>();
			await CreateFixedLengthFieldsWithSpecialCharacters(DestinationContext.GetWorkspaceId(), DestinationFieldManager).ConfigureAwait(false);
			await DestinationContext.RetrieveMappableFields().ConfigureAwait(false);
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

        protected async Task CreateFixedLengthFieldsWithSpecialCharacters(int workspaceID, IFieldManager fieldManager)
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