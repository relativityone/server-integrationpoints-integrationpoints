using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Transfer;
using NotImplementedException = System.NotImplementedException;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal class SnapshotQueryRequestProviderTests
	{
		private Mock<ISnapshotQueryConfiguration> _configurationFake;
		private Mock<IPipelineSelector> _pipelineSelectorFake;
		private Mock<IFieldManager> _fieldManagerFake;

		private SnapshotQueryRequestProvider _sut;

		private readonly IEnumerable<FieldInfoDto> _expectedDocumentTypeFields = new[]
		{
			FieldInfoDto.DocumentField("Field1", null, false),
			FieldInfoDto.DocumentField("Field2", null, false),
			FieldInfoDto.DocumentField("Field3", null, false),
		};

		private readonly FieldInfoDto _expectedIdentifierField = FieldInfoDto.DocumentField("Field", null, true);

		[SetUp]
		public void SetUp()
		{
			_configurationFake = new Mock<ISnapshotQueryConfiguration>();

			_pipelineSelectorFake = new Mock<IPipelineSelector>();

			_fieldManagerFake = new Mock<IFieldManager>();
			_fieldManagerFake.Setup(x => x.GetDocumentTypeFieldsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_expectedDocumentTypeFields.ToList());
			_fieldManagerFake.Setup(x => x.GetObjectIdentifierFieldAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_expectedIdentifierField);

			_sut = new SnapshotQueryRequestProvider(
				_configurationFake.Object,
				_pipelineSelectorFake.Object,
				_fieldManagerFake.Object);
		}

		[Test]
		public async Task GetRequestForCurrentPipelineAsync_ShouldPrepareQueryRequest_WhenDocumentFlowIsSelected()
		{
			// Arrange
			const int dataSourceArtifactId = 10;

			_configurationFake.SetupGet(x => x.DataSourceArtifactId).Returns(dataSourceArtifactId);
			
			_pipelineSelectorFake.Setup(x => x.GetPipeline())
				.Returns((ISyncPipeline) Activator.CreateInstance(typeof(SyncDocumentRunPipeline)));

			string expectedDocumentCondition = $"('ArtifactId' IN SAVEDSEARCH {dataSourceArtifactId})";

			IEnumerable<FieldRef> expectedFieldRefs =
				_expectedDocumentTypeFields.Select(x => new FieldRef {Name = x.SourceFieldName});

			// Act
			QueryRequest request = await _sut.GetRequestForCurrentPipelineAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifyQueryRequest(request, expectedDocumentCondition, expectedFieldRefs);
		}

		[Test]
		public async Task GetRequestForCurrentPipelineAsync_ShouldPrepareQueryRequest_WhenDocumentRetryFlowIsSelected()
		{
			// Arrange
			const int dataSourceArtifactId = 10;
			const int jobHistoryToRetryArtifactId = 20;

			_configurationFake.SetupGet(x => x.DataSourceArtifactId).Returns(dataSourceArtifactId);
			_configurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(jobHistoryToRetryArtifactId);

			_pipelineSelectorFake.Setup(x => x.GetPipeline())
				.Returns((ISyncPipeline)Activator.CreateInstance(typeof(SyncDocumentRetryPipeline)));

			string expectedDocumentRetryCondition = $"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{jobHistoryToRetryArtifactId}])) AND " +
			                                        $"('ArtifactId' IN SAVEDSEARCH {dataSourceArtifactId})";

			IEnumerable<FieldRef> expectedFieldRefs =
				_expectedDocumentTypeFields.Select(x => new FieldRef { Name = x.SourceFieldName });

			// Act
			QueryRequest request = await _sut.GetRequestForCurrentPipelineAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifyQueryRequest(request, expectedDocumentRetryCondition, expectedFieldRefs);
		}

		[TestCase(10, new [] {1}, true, "('ArtifactId' IN SAVEDSEARCH 10) AND (('Production::Image Count' > 0) OR ('Has Images' == CHOICE 5002224A-59F9-4C19-AA57-3765BDBFB676))")]
		[TestCase(10, new int[0], true, "('ArtifactId' IN SAVEDSEARCH 10) AND ('Has Images' == CHOICE 5002224A-59F9-4C19-AA57-3765BDBFB676)")]
		[TestCase(10, new [] {1}, false, "('ArtifactId' IN SAVEDSEARCH 10) AND ('Production::Image Count' > 0)")]
		[TestCase(10, new int[0], false, "('ArtifactId' IN SAVEDSEARCH 10) AND ('Has Images' == CHOICE 5002224A-59F9-4C19-AA57-3765BDBFB676)")]
		public async Task GetRequestForCurrentPipelineAsync_ShouldPrepareQueryRequest_WhenImageFlowIsSelected(
			int dataSourceArtifactId, int[] productionImagePrecedence, bool includeOriginalImages, string expectedCondition)
		{
			// Arrange
			_configurationFake.SetupGet(x => x.DataSourceArtifactId).Returns(dataSourceArtifactId);
			_configurationFake.SetupGet(x => x.ProductionImagePrecedence).Returns(productionImagePrecedence);
			_configurationFake.SetupGet(x => x.IncludeOriginalImageIfNotFoundInProductions).Returns(includeOriginalImages);

			_pipelineSelectorFake.Setup(x => x.GetPipeline())
				.Returns((ISyncPipeline)Activator.CreateInstance(typeof(SyncImageRunPipeline)));

			IEnumerable<FieldRef> expectedFieldRefs =
				new []{ _expectedIdentifierField }.Select(x => new FieldRef { Name = x.SourceFieldName });

			// Act
			QueryRequest request = await _sut.GetRequestForCurrentPipelineAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifyQueryRequest(request, expectedCondition, expectedFieldRefs);
		}

		[Test]
		public async Task GetRequestForCurrentPipelineAsync_ShouldPrepareQueryRequest_WhenImageRetryFlowIsSelected()
		{
			// Arrange
			const int dataSourceArtifactId = 10;
			const int jobHistoryToRetryArtifactId = 20;

			_configurationFake.SetupGet(x => x.DataSourceArtifactId).Returns(dataSourceArtifactId);
			_configurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(jobHistoryToRetryArtifactId);
			_configurationFake.SetupGet(x => x.ProductionImagePrecedence).Returns(Array.Empty<int>());

			_pipelineSelectorFake.Setup(x => x.GetPipeline())
				.Returns((ISyncPipeline)Activator.CreateInstance(typeof(SyncImageRetryPipeline)));

			string expectedImageRetryCondition = $"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{jobHistoryToRetryArtifactId}])) AND " +
			                                     $"('ArtifactId' IN SAVEDSEARCH {dataSourceArtifactId}) AND ('Has Images' == CHOICE 5002224A-59F9-4C19-AA57-3765BDBFB676)";

			IEnumerable<FieldRef> expectedFieldRefs =
				new[] { _expectedIdentifierField }.Select(x => new FieldRef { Name = x.SourceFieldName });

			// Act
			QueryRequest request = await _sut.GetRequestForCurrentPipelineAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifyQueryRequest(request, expectedImageRetryCondition, expectedFieldRefs);
		}

		private void VerifyQueryRequest(QueryRequest actualRequest, 
			string expectedCondition, IEnumerable<FieldRef> expectedFields)
		{
			actualRequest.ObjectType.ArtifactTypeID.Should().Be((int) ArtifactType.Document);
			actualRequest.Condition.Should().Be(expectedCondition);
			actualRequest.Fields.Should().BeEquivalentTo(expectedFields);
		}
	}
}
