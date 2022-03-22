using Atata;
using System;
using System.IO;
using System.Collections.Generic;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Common.Extensions;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Objects;
using Relativity.Testing.Framework.Api;
using System.Linq;
using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncTestsImplementation
	{
		private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
		private readonly Dictionary<string, Workspace> _destinationWorkspaces = new Dictionary<string, Workspace>();

		public SyncTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
		{
			_testsImplementationTestFixture = testsImplementationTestFixture;
		}

		public void OnSetUpFixture()
		{
			RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile());
		}

		public void OnTearDownFixture()
		{
			foreach (KeyValuePair<string, Workspace> destinationWorkspace in _destinationWorkspaces)
			{
				RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace.Value);
			}
		}

		public void SavedSearchNativesAndMetadataGoldFlow()
		{
			// Arrange
			_testsImplementationTestFixture.LoginAsStandardUser();

			string integrationPointName = nameof(SavedSearchNativesAndMetadataGoldFlow);

			Workspace destinationWorkspace = CreateDestinationWorkspace();
			const int keywordSearchDocumentsCount = 5;
			KeywordSearch keywordSearch = new KeywordSearch
			{
				Name = nameof(SavedSearchNativesAndMetadataGoldFlow),
				SearchCriteria = new CriteriaCollection
				{
					Conditions = new List<BaseCriteria>
					{
						new Criteria
						{
							Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.GreaterThanOrEqualTo, "AZIPPER_0007291")
						},
						new Criteria
						{
							Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.LessThanOrEqualTo, "AZIPPER_0007491")
						}
					}
				}
			};
			RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(_testsImplementationTestFixture.Workspace.ArtifactID, keywordSearch);

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPoint(integrationPointName,
				destinationWorkspace, keywordSearch.Name, copyNativesMode: RelativityProviderCopyNativeFiles.PhysicalFiles);

			integrationPointViewPage = integrationPointViewPage.RunIntegrationPoint(integrationPointName);

			string expectedDestinationCaseTag = $"This Instance - {destinationWorkspace.Name} - {destinationWorkspace.ArtifactID}";
			string expectedSourceCaseTag = $"This Instance - {_testsImplementationTestFixture.Workspace.Name} - {_testsImplementationTestFixture.Workspace.ArtifactID}";
			string expectedSourceJobTag = $"{integrationPointName} - {GetJobId(_testsImplementationTestFixture.Workspace.ArtifactID, integrationPointName)}";

			var sourceDocs = GetDocumentsTagsDataFromSourceWorkspace(_testsImplementationTestFixture.Workspace.ArtifactID);
			var destinationDocs = GetDocumentsTagsDataFromDestinationWorkspace(destinationWorkspace.ArtifactID);

			// Assert
			int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(integrationPointName);
			int workspaceDocumentCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(destinationWorkspace.ArtifactID).Length;

			transferredItemsCount.Should().Be(workspaceDocumentCount).And.Be(keywordSearchDocumentsCount);

			GetCorrectlyTaggedDocumentsCount(sourceDocs, "Relativity Destination Case", expectedDestinationCaseTag).Should().Be(transferredItemsCount);
			GetCorrectlyTaggedDocumentsCount(destinationDocs, "Relativity Source Case", expectedSourceCaseTag).Should().Be(transferredItemsCount);
			GetCorrectlyTaggedDocumentsCount(destinationDocs, "Relativity Source Job", expectedSourceJobTag).Should().Be(transferredItemsCount);
		}

		public void ProductionImagesGoldFlow()
		{
			// Arrange
			_testsImplementationTestFixture.LoginAsStandardUser();

			string integrationPointName = $"{nameof(ProductionImagesGoldFlow)} - {Guid.NewGuid()}";

			Workspace destinationWorkspace = CreateDestinationWorkspace();

			KeywordSearch keywordSearch = RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(_testsImplementationTestFixture.Workspace.ArtifactID, new KeywordSearch
			{
				Name = nameof(ProductionImagesGoldFlow),
				SearchCriteria = new CriteriaCollection
				{
					Conditions = new List<BaseCriteria>
					{
						new Criteria
						{
							Condition = new CriteriaCondition(new NamedArtifact { Name = "Has Native" }, ConditionOperator.Is, true)
						},
						new Criteria
						{
							Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.GreaterThanOrEqualTo, "AZIPPER_0007494")
						},
						new Criteria
						{
							Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.LessThanOrEqualTo, "AZIPPER_0007748")
						}
					}
				}
			});

			ProductionPlaceholder productionPlaceholder = new ProductionPlaceholder
			{
				Name = nameof(ProductionImagesGoldFlow),
				PlaceholderType = PlaceholderType.Image,
				FileName = "Image",
				FileData = Convert.ToBase64String(File.ReadAllBytes(DataFiles.PLACEHOLDER_IMAGE_PATH))
			};
			RelativityFacade.Instance.Resolve<IProductionPlaceholderService>().Create(_testsImplementationTestFixture.Workspace.ArtifactID, productionPlaceholder);

			const int productionDocumentsCount = 5;
            Testing.Framework.Models.Production production = new Testing.Framework.Models.Production
			{
				Name = nameof(ProductionImagesGoldFlow),
				Numbering = new ProductionNumbering
				{
					NumberingType = NumberingType.OriginalImage,
					BatesPrefix = "Prefix",
					BatesSuffix = "Suffix",
					NumberOfDigitsForDocumentNumbering = 7,
					BatesStartNumber = 6,
					AttachmentRelationalField = new NamedArtifact()
				},

				DataSources = new List<ProductionDataSource>
				{
					new ProductionDataSource
					{
						Name = nameof(ProductionImagesGoldFlow),
						ProductionType = Testing.Framework.Models.ProductionType.ImagesAndNatives,
						SavedSearch = new NamedArtifact
						{
							ArtifactID = keywordSearch.ArtifactID
						},
						UseImagePlaceholder = UseImagePlaceholderOption.AlwaysUseImagePlaceholder,
						Placeholder = new NamedArtifact
						{
							ArtifactID = productionPlaceholder.ArtifactID
						},
					}
				}
			};
			RelativityFacade.Instance.ProduceProduction(_testsImplementationTestFixture.Workspace, production);
			
			// Act
			IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage
				.CreateProductionToFolderIntegrationPoint(integrationPointName, destinationWorkspace, production);

			integrationPointViewPage = integrationPointViewPage.RunIntegrationPoint(integrationPointName);

			string expectedDestinationCaseTag = $"This Instance - {destinationWorkspace.Name} - {destinationWorkspace.ArtifactID}";
			string expectedSourceCaseTag = $"This Instance - {_testsImplementationTestFixture.Workspace.Name} - {_testsImplementationTestFixture.Workspace.ArtifactID}";
			string expectedSourceJobTag = $"{integrationPointName} - {GetJobId(_testsImplementationTestFixture.Workspace.ArtifactID, integrationPointName)}";

			var sourceDocs = GetDocumentsTagsDataFromSourceWorkspace(_testsImplementationTestFixture.Workspace.ArtifactID);
			var destinationDocs = GetDocumentsTagsDataFromDestinationWorkspace(destinationWorkspace.ArtifactID);

			// Assert
			int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(integrationPointName);
			int workspaceDocumentCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(destinationWorkspace.ArtifactID).Length;

			transferredItemsCount.Should().Be(workspaceDocumentCount).And.Be(productionDocumentsCount);

			GetCorrectlyTaggedDocumentsCount(sourceDocs, "Relativity Destination Case", expectedDestinationCaseTag).Should().Be(transferredItemsCount);
			GetCorrectlyTaggedDocumentsCount(destinationDocs, "Relativity Source Case", expectedSourceCaseTag).Should().Be(transferredItemsCount);
			GetCorrectlyTaggedDocumentsCount(destinationDocs, "Relativity Source Job", expectedSourceJobTag).Should().Be(transferredItemsCount);
		}

		public void EntitiesPushGoldFlow()
		{
			// Arrange
			_testsImplementationTestFixture.LoginAsStandardUser();

			Workspace destinationWorkspace = CreateDestinationWorkspace();

			string integrationPointName = nameof(EntitiesPushGoldFlow);

			// Act
			IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
			IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

			IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage
				.CreateSyncRdoIntegrationPoint(integrationPointName, destinationWorkspace, IntegrationPointTransferredObjects.Entity, "Entities - Legal Hold View");

			// Assert
		}

		private Workspace CreateDestinationWorkspace()
		{
			string workspaceName = $"SYNC - {Guid.NewGuid()}";

			Workspace workspace = RelativityFacade.Instance.CreateWorkspace(workspaceName, _testsImplementationTestFixture.Workspace.Name);

			_destinationWorkspaces.Add(workspaceName, workspace);

			workspace.InstallLegalHold();

			return workspace;
		}

		private bool FieldTagMatchesExpectedValue(RelativityObject doc, string fieldName, string expectedTagValue)
        {
			var fieldValue = doc.FieldValues.Where(f => f.Field.Name == fieldName).FirstOrDefault().Value;			
			return fieldValue == null ? false : ((IList<RelativityObjectValue>)fieldValue).Where(x => x.Name == expectedTagValue).Any();
		}

		private int GetCorrectlyTaggedDocumentsCount(List<RelativityObject> documents, string taggedField, string tagValue)
		{
			return documents.Where(x => FieldTagMatchesExpectedValue(x, taggedField, tagValue)).Count();			
        }

		private List<RelativityObject> GetDocumentsTagsDataFromSourceWorkspace(int workspaceId)
		{
			FieldRef[] fields = new FieldRef[] { new FieldRef { Name = "Relativity Destination Case" } };
			return GetDocumentsWithSelectedFields(workspaceId, fields);			
		}

		private List<RelativityObject> GetDocumentsTagsDataFromDestinationWorkspace(int workspaceId)
		{			
			FieldRef[] fields = new FieldRef[] { new FieldRef { Name = "Relativity Source Case" },
				new FieldRef { Name = "Relativity Source Job" } };

			return GetDocumentsWithSelectedFields(workspaceId, fields);
		}

		private List<RelativityObject> GetDocumentsWithSelectedFields(int workspaceId, FieldRef[] fields)
		{
			using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
					Fields = fields
				};

				return objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue)
					.GetAwaiter().GetResult().Objects.ToList();
			}
		}

		private int GetJobId(int workspaceId, string jobName)
        {
			using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.JobHistoryGuid },
					Fields = new FieldRef[] { new FieldRef { Name = "Job ID" } },
					Condition = $"(('Name' LIKE '{jobName}'))"
			};
		
				var result =  objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue)
					.GetAwaiter().GetResult().Objects.ToList();

				return result.FirstOrDefault().ArtifactID;
			}		
        }
	}
}
