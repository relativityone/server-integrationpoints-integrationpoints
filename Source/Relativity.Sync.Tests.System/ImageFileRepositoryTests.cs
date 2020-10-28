using System;
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
	internal sealed class ImageFileRepositoryTests : SystemTest
	{
		private WorkspaceRef _workspace;

		private IImageFileRepository _sut;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

			var container = ContainerHelper.Create(new ConfigurationStub
				{
					SourceWorkspaceArtifactId = _workspace.ArtifactID
				},
				cb => cb.RegisterInstance(Logger).As<ISyncLog>()
			);

			_sut = container.Resolve<IImageFileRepository>();
		}

		[TearDown]
		public Task TearDown()
		{
			// Deleting documents to restore the workspace to empty state between tests
			return Environment.DeleteAllDocumentsInWorkspaceAsync(_workspace);
		}

		[IdentifiedTest("2f71008b-89bc-4322-b700-ccd941c9b463")]
		public async Task QueryImagesForDocumentsAsync_ShouldHandleMultipleImagesPerDocument()
		{
			// Arrange
			Dataset dataset = Dataset.MultipleImagesPerDocument;
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			await ImportHelper.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);
			var documentIds = await GetAllDocumentArtifactIdsAsync().ConfigureAwait(false);

			// Act
			ImageFile[] retrievedImages = (await _sut.QueryImagesForDocumentsAsync(_workspace.ArtifactID, documentIds.ToArray(), new QueryImagesOptions { IncludeOriginalImageIfNotFoundInProductions = true }).ConfigureAwait(false)).ToArray();

			// Assert
			int expectedImagesCount = dataset.GetFiles().Count();
			int expectedDistinctDocumentsCount =
				dataset.GetFiles().Select(x => dataset.GetControlNumber(x)).Distinct().Count();

			retrievedImages.Length.Should().Be(expectedImagesCount);
			retrievedImages.Select(x => x.Filename).Should().BeEquivalentTo(dataset.GetFiles().Select(x => x.Name));
			retrievedImages.Select(x => x.DocumentArtifactId).Distinct().Count().Should()
				.Be(expectedDistinctDocumentsCount);
		}

		[IdentifiedTest("50ee0c92-60e1-46c4-bd7e-2c83b952e51b")]
		public async Task CalculateImagesTotalSizeAsync_ShouldCalculateCorrectSize()
		{
			// Arrange
			Dataset dataset = Dataset.Images;
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			await ImportHelper.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);
			QueryRequest request = GetQueryRequest();

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
			int productionId = await CreateAndImportProductionAsync(_workspace.ArtifactID, Dataset.ImagesBig).ConfigureAwait(false);
			var request = GetQueryRequest();

			// Act
			long calculatedImagesSize = await _sut.CalculateImagesTotalSizeAsync(_workspace.ArtifactID, request,
				new QueryImagesOptions { ProductionIds = new[] { productionId } }).ConfigureAwait(false);

			// Assert
			long expectedImagesSize = Dataset.ImagesBig.GetTotalFilesSize();

			calculatedImagesSize.Should().Be(expectedImagesSize);
		}

		[IdentifiedTest("c6ef001e-68b7-438d-9697-8406bf56797c")]
		public async Task CalculateImagesTotalSizeAsync_ShouldIncludeOriginalImagesWhenEnabled()
		{
			// Arrange
			int productionId = await CreateAndImportProductionAsync(_workspace.ArtifactID, Dataset.Images).ConfigureAwait(false);

			Dataset dataset = Dataset.ThreeImages;
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			await ImportHelper.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);

			QueryRequest request = GetQueryRequest();

			// Act
			long calculatedImagesSize = await _sut.CalculateImagesTotalSizeAsync(_workspace.ArtifactID, request,
				new QueryImagesOptions { ProductionIds = new[] { productionId }, IncludeOriginalImageIfNotFoundInProductions = true }).ConfigureAwait(false);

			// Assert
			long expectedImagesSize = Dataset.Images.GetTotalFilesSize() + Dataset.ThreeImages.GetTotalFilesSize();

			calculatedImagesSize.Should().Be(expectedImagesSize);
		}

		[IdentifiedTest("dc9a2ed8-092a-4e2f-8476-6e578a127b3c")]
		public async Task CalculateImagesTotalSizeAsync_ShouldRespectProductionPrecedence()
		{
			// Arrange
			int singleDocumentProductionId = await CreateAndImportProductionAsync(_workspace.ArtifactID, Dataset.SingleDocumentProduction).ConfigureAwait(false);
			int twoDocumentProductionId = await CreateAndImportProductionAsync(_workspace.ArtifactID, Dataset.TwoDocumentProduction).ConfigureAwait(false);

			Dataset dataset = Dataset.ThreeImages;
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			await ImportHelper.ImportDataAsync(_workspace.ArtifactID, dataTableWrapper).ConfigureAwait(false);

			QueryRequest request = GetQueryRequest();

			// Act
			long calculatedImagesSize = await _sut.CalculateImagesTotalSizeAsync(_workspace.ArtifactID, request,
				new QueryImagesOptions { ProductionIds = new[] { singleDocumentProductionId, twoDocumentProductionId }, IncludeOriginalImageIfNotFoundInProductions = true }).ConfigureAwait(false);

			// Assert
			long expectedImagesSize = GetExpectedSizeWithPrecedence(new[]
				{Dataset.SingleDocumentProduction, Dataset.TwoDocumentProduction, Dataset.ThreeImages});

			calculatedImagesSize.Should().Be(expectedImagesSize);
		}

		private long GetExpectedSizeWithPrecedence(Dataset[] datasets)
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

		private async Task<IEnumerable<int>> GetAllDocumentArtifactIdsAsync()
		{
			using (var om = ServiceFactory.CreateProxy<IObjectManager>())
			{
				var result = await om.QuerySlimAsync(_workspace.ArtifactID, GetQueryRequest(), 1, 1000)
					.ConfigureAwait(false);

				return result.Objects.Select(x => x.ArtifactID);
			}
		}
	}
}
