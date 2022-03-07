using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal class SnapshotQueryRequestProviderTests
	{
		private Mock<ISnapshotQueryConfiguration> _configurationFake;
		private Mock<IPipelineSelector> _pipelineSelectorFake;
		private Mock<IFieldManager> _fieldManagerFake;
		private Mock<ISourceServiceFactoryForAdmin> _sourceServiceFactoryForAdmin;
		private Mock<IObjectManager> _objectManager;
		private Mock<IChoiceQueryManager> _choiceQueryManager;

		private SnapshotQueryRequestProvider _sut;

		private readonly IEnumerable<FieldInfoDto> _expectedFields = new[]
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
			_configurationFake.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Document);

			_pipelineSelectorFake = new Mock<IPipelineSelector>();

			_objectManager = new Mock<IObjectManager>();
			_choiceQueryManager = new Mock<IChoiceQueryManager>();

			_sourceServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			_sourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_sourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IChoiceQueryManager>()).ReturnsAsync(_choiceQueryManager.Object);

			_fieldManagerFake = new Mock<IFieldManager>();
			_fieldManagerFake.Setup(x => x.GetDocumentTypeFieldsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_expectedFields.ToList());
			_fieldManagerFake.Setup(x => x.GetObjectIdentifierFieldAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_expectedIdentifierField);

			_fieldManagerFake.Setup(x => x.GetMappedFieldsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_expectedFields.ToList());

			_sut = new SnapshotQueryRequestProvider(
				_configurationFake.Object,
				_pipelineSelectorFake.Object,
				_fieldManagerFake.Object,
				_sourceServiceFactoryForAdmin.Object,
				new EmptyLogger()
				);
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
				_expectedFields.Select(x => new FieldRef {Name = x.SourceFieldName});

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
				_expectedFields.Select(x => new FieldRef { Name = x.SourceFieldName });

			// Act
			QueryRequest request = await _sut.GetRequestForCurrentPipelineAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifyQueryRequest(request, expectedDocumentRetryCondition, expectedFieldRefs);
		}

		[TestCase(10, new [] {1}, true, "('ArtifactId' IN SAVEDSEARCH 10) AND (('Production::Image Count' > 0) OR ('Has Images' == CHOICE 1034243))")]
		[TestCase(10, new int[0], true, "('ArtifactId' IN SAVEDSEARCH 10) AND ('Has Images' == CHOICE 1034243)")]
		[TestCase(10, new [] {1}, false, "('ArtifactId' IN SAVEDSEARCH 10) AND ('Production::Image Count' > 0)")]
		[TestCase(10, new int[0], false, "('ArtifactId' IN SAVEDSEARCH 10) AND ('Has Images' == CHOICE 1034243)")]
		public async Task GetRequestForCurrentPipelineAsync_ShouldPrepareQueryRequest_WhenImageFlowIsSelected(
			int dataSourceArtifactId, int[] productionImagePrecedence, bool includeOriginalImages, string expectedCondition)
		{
			// Arrange
			_configurationFake.SetupGet(x => x.DataSourceArtifactId).Returns(dataSourceArtifactId);
			_configurationFake.SetupGet(x => x.ProductionImagePrecedence).Returns(productionImagePrecedence);
			_configurationFake.SetupGet(x => x.IncludeOriginalImageIfNotFoundInProductions).Returns(includeOriginalImages);
            _configurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(123456);

            QueryResult fieldArtifactIdQueryResult = new QueryResult()
            {
                Objects = new List<RelativityObject>()
                {
                    new RelativityObject()
                    {
                        Name = "Has Images",
                        ArtifactID = 1003672
                    }
                }
            };
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(fieldArtifactIdQueryResult);

            List<Services.ChoiceQuery.Choice> fieldChoicesList = new List<Services.ChoiceQuery.Choice>()
            {
                new Services.ChoiceQuery.Choice()
                {
                    Name = "Yes",
                    ArtifactID = 1034243
                },
                new Services.ChoiceQuery.Choice()
                {
                    Name = "No",
                    ArtifactID = 1034244
                }
            };
            _choiceQueryManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(fieldChoicesList);

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
			_configurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(123456);

			QueryResult fieldArtifactIdQueryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						Name = "Has Images",
						ArtifactID = 1003672
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(fieldArtifactIdQueryResult);

			List<Services.ChoiceQuery.Choice> fieldChoicesList = new List<Services.ChoiceQuery.Choice>()
			{
				new Services.ChoiceQuery.Choice()
				{
					Name = "Yes",
					ArtifactID = 1034243
				},
				new Services.ChoiceQuery.Choice()
				{
					Name = "No",
					ArtifactID = 1034244
				}
			};
			_choiceQueryManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(fieldChoicesList);

			_pipelineSelectorFake.Setup(x => x.GetPipeline())
				.Returns((ISyncPipeline)Activator.CreateInstance(typeof(SyncImageRetryPipeline)));

			string expectedImageRetryCondition = $"(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{jobHistoryToRetryArtifactId}])) AND " +
			                                     $"('ArtifactId' IN SAVEDSEARCH {dataSourceArtifactId}) AND ('Has Images' == CHOICE 1034243)";

			IEnumerable<FieldRef> expectedFieldRefs =
				new[] { _expectedIdentifierField }.Select(x => new FieldRef { Name = x.SourceFieldName });

			// Act
			QueryRequest request = await _sut.GetRequestForCurrentPipelineAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifyQueryRequest(request, expectedImageRetryCondition, expectedFieldRefs);
		}

		[Test]
		public void GetRequestForCurrentPipelineAsync_ShouldThrowError_WhenCannotFindYesChoice()
		{
			// Arrange
			const int dataSourceArtifactId = 10;
			const int jobHistoryToRetryArtifactId = 20;

			_configurationFake.SetupGet(x => x.DataSourceArtifactId).Returns(dataSourceArtifactId);
			_configurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(jobHistoryToRetryArtifactId);
			_configurationFake.SetupGet(x => x.ProductionImagePrecedence).Returns(Array.Empty<int>());
			_configurationFake.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(123456);

			QueryResult fieldArtifactIdQueryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						Name = "Has Images",
						ArtifactID = 1003672
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(fieldArtifactIdQueryResult);

			List<Services.ChoiceQuery.Choice> fieldChoicesList = new List<Services.ChoiceQuery.Choice>()
			{
				new Services.ChoiceQuery.Choice()
				{
					Name = "No",
					ArtifactID = 1034244
				}
			};
			_choiceQueryManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(fieldChoicesList);

			_pipelineSelectorFake.Setup(x => x.GetPipeline())
				.Returns((ISyncPipeline)Activator.CreateInstance(typeof(SyncImageRunPipeline)));

			// Assert
			SyncException syncException = Assert.ThrowsAsync<SyncException>(async () => await _sut.GetRequestForCurrentPipelineAsync(CancellationToken.None).ConfigureAwait(false));
			syncException.Message.Should().Be("Unable to find choice with \"Yes\" name for \"Has Images\" field - this system field is in invalid state");
		}
		
		[Test]
		public async Task GetRequestForCurrentPipelineAsync_ShouldPrepareQueryRequest_WhenNonDocumentFlowIsSelected()
		{
			// Arrange
			const int dataSourceArtifactId = 10;

			_configurationFake.SetupGet(x => x.DataSourceArtifactId).Returns(dataSourceArtifactId);
			_configurationFake.SetupGet(x => x.RdoArtifactTypeId).Returns(420);
			
			_pipelineSelectorFake.Setup(x => x.GetPipeline())
				.Returns((ISyncPipeline) Activator.CreateInstance(typeof(SyncNonDocumentRunPipeline)));

			string expectedDocumentCondition = $"('ArtifactId' IN VIEW {dataSourceArtifactId})";

			IEnumerable<FieldRef> expectedFieldRefs =
				_expectedFields.Select(x => new FieldRef {Name = x.SourceFieldName});

			// Act
			QueryRequest request = await _sut.GetRequestForCurrentPipelineAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			VerifyQueryRequest(request, expectedDocumentCondition, expectedFieldRefs);
		}

		[Test]
		public async Task GetRequestForLinkingNonDocumentObjectsAsync_ShouldPrepareQuery()
		{
			// Arrange
			List<FieldInfoDto> fieldsForLinks = GetFieldsForLinks().ToList();

			_fieldManagerFake.Setup(x => x.GetMappedFieldsNonDocumentForLinksAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(fieldsForLinks);
			
			
			// Act
			QueryRequest request = await _sut.GetRequestForLinkingNonDocumentObjectsAsync(CancellationToken.None).ConfigureAwait(false);

			// Assert
			const string expectedDocumentCondition = "('Link 1' ISSET) OR ('Link 2' ISSET)";
			IEnumerable<FieldRef> expectedFieldRefs = fieldsForLinks.Select(dto => new FieldRef {Name = dto.SourceFieldName});
			VerifyQueryRequest(request, expectedDocumentCondition, expectedFieldRefs);
		}

		private IEnumerable<FieldInfoDto> GetFieldsForLinks()
		{
			yield return new FieldInfoDto(SpecialFieldType.None, "Id", "Id", true, false);
			yield return new FieldInfoDto(SpecialFieldType.None, "Link 1", "Link 1", false, false);
			yield return new FieldInfoDto(SpecialFieldType.None, "Link 2", "Link 2", false, false);
		}

		private void VerifyQueryRequest(QueryRequest actualRequest, 
			string expectedCondition, IEnumerable<FieldRef> expectedFields)
		{
			actualRequest.ObjectType.ArtifactTypeID.Should().Be(_configurationFake.Object.RdoArtifactTypeId);
			actualRequest.Condition.Should().Be(expectedCondition);
			actualRequest.Fields.Should().BeEquivalentTo(expectedFields);
		}
	}
}
