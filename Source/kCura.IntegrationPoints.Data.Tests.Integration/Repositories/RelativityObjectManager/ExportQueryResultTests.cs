using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using NUnit.Framework;
using Relativity;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories.RelativityObjectManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ExportQueryResultTests
	{
		private int _workspaceId;
		private TestHelper _helper;
		private IRelativityObjectManager _relativityObjectManager;

		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_workspaceId = Workspace.FindWorkspaceByName(Rsapi.CreateRsapiClient(), "Functional Tests Template").ArtifactID;
			_helper = new TestHelper();
			_relativityObjectManager = CreateObjectManager();
		}

		[Test]
		public async Task ExportQueryResult_ShouldDeleteExport_WhenDisposed()
		{
			// Arrange
			QueryRequest query = PrepareFieldsQueryRequest();

			Guid runId = Guid.Empty;

			// Act
			using (IExportQueryResult exportQueryResult = await _relativityObjectManager.QueryWithExportAsync(query, 0).ConfigureAwait(false))
			{
				runId = exportQueryResult.ExportResult.RunID;
			}

			Func<Task> action = () => _relativityObjectManager.RetrieveResultsBlockFromExportAsync(runId, 1, 0);

			// Assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[TestCase(1)]
		[TestCase(10)]
		[TestCase(100)]
		public async Task GetNextBlockAsync_ShouldReadFullBlockSize(int blockSize)
		{
			// Arrange
			QueryRequest query = PrepareFieldsQueryRequest();

			IEnumerable<RelativityObjectSlim> results = null;

			// Act
			using (IExportQueryResult exportQueryResult = await _relativityObjectManager.QueryWithExportAsync(query, 0).ConfigureAwait(false))
			{
				results = await exportQueryResult.GetNextBlockAsync(0, blockSize).ConfigureAwait(false);
			}

			// Assert
			results.Count().Should().Be(blockSize);
		}

		[Test]
		public async Task GetNextBlockAsync_ShouldReadAllObjects()
		{
			// Arrange
			const int blockSize = 50;
			QueryRequest query = PrepareFieldsQueryRequest();

			List<RelativityObjectSlim> results = new List<RelativityObjectSlim>();
			RelativityObjectSlim[] partialResults = null;
			int expectedNumberOfFields;

			// Act
			using (IExportQueryResult exportQueryResult =
				await _relativityObjectManager.QueryWithExportAsync(query, 0).ConfigureAwait(false))
			{
				expectedNumberOfFields = (int)exportQueryResult.ExportResult.RecordCount;
				int start = 0;
				do
				{
					partialResults = (await exportQueryResult.GetNextBlockAsync(start, blockSize).ConfigureAwait(false)).ToArray();
					results.AddRange(partialResults);
					start += partialResults.Length;
				}
				while (partialResults.Any());
			}

			// Assert
			results.Count().Should().Be(expectedNumberOfFields);
		}

		[Test]
		public async Task GetAllResultsAsync_ShouldReadAllObjects()
		{
			// Arrange
			QueryRequest query = PrepareFieldsQueryRequest();

			List<RelativityObjectSlim> results = new List<RelativityObjectSlim>();
			int expectedNumberOfFields;

			// Act
			using (IExportQueryResult exportQueryResult =
				await _relativityObjectManager.QueryWithExportAsync(query, 0).ConfigureAwait(false))
			{
				expectedNumberOfFields = (int)exportQueryResult.ExportResult.RecordCount;
				results = (await exportQueryResult.GetAllResultsAsync().ConfigureAwait(false)).ToList();
			}

			// Assert
			results.Count().Should().Be(expectedNumberOfFields);
		}

		[Test]
		public async Task GetAllResultsAsync_ShouldReadAllObjectsTwice()
		{
			// Arrange
			QueryRequest query = PrepareFieldsQueryRequest();

			// Act
			using (IExportQueryResult exportQueryResult =
				await _relativityObjectManager.QueryWithExportAsync(query, 0).ConfigureAwait(false))
			{
				await exportQueryResult.GetAllResultsAsync().ConfigureAwait(false);

				RelativityObjectSlim[] secondResults = (await exportQueryResult.GetAllResultsAsync()).ToArray();

				// Assert
				secondResults.Length.Should().Be((int)exportQueryResult.ExportResult.RecordCount);
			}
		}

		private IRelativityObjectManager CreateObjectManager()
		{
			var factory = new RelativityObjectManagerFactory(_helper);
			return factory.CreateRelativityObjectManager(_workspaceId);
		}

		private QueryRequest PrepareFieldsQueryRequest()
		{
			int fieldArtifactTypeID = (int)ArtifactType.Field;
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = fieldArtifactTypeID
				},
				Condition = $"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID}",
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}
	}
}
