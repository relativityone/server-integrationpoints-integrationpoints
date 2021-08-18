﻿using Atata;
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
			foreach (var destinationWorkspace in _destinationWorkspaces)
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
			var integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
			var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

			var integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPoint(integrationPointName,
				destinationWorkspace, keywordSearch, copyNativesMode: RelativityProviderCopyNativeFiles.PhysicalFiles);

			integrationPointViewPage = RunIntegrationPoint(integrationPointViewPage, integrationPointName);

			// Assert
			int transferredItemsCount = GetTransferredItemsCount(integrationPointViewPage, integrationPointName);
			int workspaceDocumentCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(destinationWorkspace.ArtifactID).Length;

			transferredItemsCount.Should().Be(workspaceDocumentCount).And.Be(keywordSearchDocumentsCount);
		}

		public void ProductionImagesGoldFlow()
		{
			// Arrange
			_testsImplementationTestFixture.LoginAsStandardUser();

			string integrationPointName = nameof(ProductionImagesGoldFlow);

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
			var production = new Testing.Framework.Models.Production
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
			var integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
			var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

			var integrationPointViewPage = integrationPointEditPage.CreateProductionToFolderIntegrationPoint(
				integrationPointName, destinationWorkspace, production);

			integrationPointViewPage = RunIntegrationPoint(integrationPointViewPage, integrationPointName);

			// Assert
			int transferredItemsCount = GetTransferredItemsCount(integrationPointViewPage, integrationPointName);
			int workspaceDocumentCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(destinationWorkspace.ArtifactID).Length;

			transferredItemsCount.Should().Be(workspaceDocumentCount).And.Be(productionDocumentsCount);
		}

		private Workspace CreateDestinationWorkspace()
		{
			string workspaceName = $"SYNC - {Guid.NewGuid()}";

			Workspace workspace = RelativityFacade.Instance.CreateWorkspace(workspaceName, _testsImplementationTestFixture.Workspace.Name);

			_destinationWorkspaces.Add(workspaceName, workspace);

			return workspace;
		}

		private static IntegrationPointViewPage RunIntegrationPoint(IntegrationPointViewPage integrationPointViewPage, string integrationPointName)
		{
			return integrationPointViewPage.Run.WaitTo.Within(60).BeVisible().
				Run.ClickAndGo().
				OK.ClickAndGo().
				WaitUntilJobCompleted(integrationPointName);
		}

		private static int GetTransferredItemsCount(IntegrationPointViewPage integrationPointViewPage, string integrationPointName)
		{
			return Int32.Parse(integrationPointViewPage.Status.Table.Rows[y => y.Name == integrationPointName].ItemsTransferred.Content.Value);
		}
	}
}
