using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.API;
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
				toggleProvider: null,
				cb => cb.RegisterInstance(Logger).As<IAPILog>()
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
