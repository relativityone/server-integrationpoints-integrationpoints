﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Transfer;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	public class ImageFileRepositoryTests : SystemTest
	{
		private WorkspaceRef _workspace;
		private ImportHelper _importHelper;

		private IImageFileRepository _sut;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

			_importHelper = new ImportHelper(ServiceFactory);

			var container = ContainerHelper.Create(new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _workspace.ArtifactID
			},
				cb => cb.RegisterInstance(Logger).As<ISyncLog>()
				);

			_sut = container.Resolve<IImageFileRepository>();
		}

		[TearDown]
		public async Task TearDown()
		{
			// Deleting documents to restore the workspace to empty state between tests
			await Environment.DeleteAllDocumentsInWorkspace(_workspace).ConfigureAwait(false);
		}

		[IdentifiedTest("50ee0c92-60e1-46c4-bd7e-2c83b952e51b")]
		public async Task CalculateImagesTotalSizeAsync_ShouldCalculateCorrectSize()
		{
			// Arrange
			Dataset dataset = Dataset.Images;
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			await _importHelper.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);
			var request = GetQueryRequest();

			// Act
			long calculatedImagesSize = await _sut.CalculateImagesTotalSizeAsync(_workspace.ArtifactID, request,
				new QueryImagesOptions { IncludeOriginalImageIfNotFoundInProductions = true }).ConfigureAwait(false);

			// Assert
			long expectedImagesSize = dataset.GetTotalFilesSize();

			calculatedImagesSize.Should().Be(expectedImagesSize);
		}

		[IdentifiedTest("ab852f80-fda3-4347-a5c1-29d595c7030d")]
		public async Task CalculateImagesTotalSizeAsync_ShouldCalculateCorrectSize_ForProduction()
		{
			// Arrange
			var productionId = await CreateAndImportProduction(Dataset.ImagesBig);
			var request = GetQueryRequest();

			// Act
			long calculatedImagesSize = await _sut.CalculateImagesTotalSizeAsync(_workspace.ArtifactID, request,
				new QueryImagesOptions { ProductionIds = new List<int> { productionId } }).ConfigureAwait(false);

			// Assert
			long expectedImagesSize = Dataset.ImagesBig.GetTotalFilesSize();

			calculatedImagesSize.Should().Be(expectedImagesSize);
		}

		[IdentifiedTest("c6ef001e-68b7-438d-9697-8406bf56797c")]
		public async Task CalculateImagesTotalSizeAsync_ShouldIncludeOriginalImagesWhenEnabled()
		{
			// Arrange
			var productionId = await CreateAndImportProduction(Dataset.Images);

			Dataset dataset = Dataset.ThreeImages;
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			await _importHelper.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);

			var request = GetQueryRequest();

			// Act
			long calculatedImagesSize = await _sut.CalculateImagesTotalSizeAsync(_workspace.ArtifactID, request,
				new QueryImagesOptions { ProductionIds = new List<int> { productionId }, IncludeOriginalImageIfNotFoundInProductions = true }).ConfigureAwait(false);

			// Assert
			long expectedImagesSize = Dataset.Images.GetTotalFilesSize() + Dataset.ThreeImages.GetTotalFilesSize();

			calculatedImagesSize.Should().Be(expectedImagesSize);
		}

		[IdentifiedTest("dc9a2ed8-092a-4e2f-8476-6e578a127b3c")]
		public async Task CalculateImagesTotalSizeAsync_ShouldRespectProductionPrecedence()
		{
			// Arrange
			var singleDocumentProductionId = await CreateAndImportProduction(Dataset.SingleDocumentProduction);
			var twoDocumentProductionId = await CreateAndImportProduction(Dataset.TwoDocumentProduction);

			Dataset dataset = Dataset.ThreeImages;
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			await _importHelper.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);

			var request = GetQueryRequest();

			// Act
			long calculatedImagesSize = await _sut.CalculateImagesTotalSizeAsync(_workspace.ArtifactID, request,
				new QueryImagesOptions { ProductionIds = new List<int> { singleDocumentProductionId, twoDocumentProductionId }, IncludeOriginalImageIfNotFoundInProductions = true }).ConfigureAwait(false);

			// Assert
			long expectedImagesSize = GetExpectedSizeWithPrecedense(new[]
				{Dataset.SingleDocumentProduction, Dataset.TwoDocumentProduction, Dataset.ThreeImages});

			calculatedImagesSize.Should().Be(expectedImagesSize);
		}

		private long GetExpectedSizeWithPrecedense(Dataset[] datasets)
		{
			var sizes = new Dictionary<string, long>();

			foreach (var image in datasets.SelectMany(x => x.GetFiles()))
			{
				if (!sizes.ContainsKey(image.Name))
				{
					sizes.Add(image.Name, image.Length);
				}
			}

			return sizes.Sum(x => x.Value);
		}

		private async Task<int> CreateAndImportProduction(Dataset dataset, string productionName = "")
		{
			if (string.IsNullOrEmpty(productionName))
			{
				productionName = dataset.Name + "_" + DateTime.Now.ToLongTimeString() + "_" + DateTime.Now.Ticks;
			}

			int productionId = await Environment.CreateProductionAsync(_workspace.ArtifactID, productionName).ConfigureAwait(false);

			var dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			await _importHelper.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper, productionId).ConfigureAwait(false);

			return productionId;
		}

		private QueryRequest GetQueryRequest()
		{
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID
				}
			};
			return request;
		}
	}
}
