using FluentAssertions;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class ObjectTypeControllerTests
	{
		private const int _WORKSPACE_ID = 111;
		private Mock<ICPHelper> _helperMock;
		private Mock<IObjectManager> _objectManagerMock;

		private ObjectTypeController _sut;

		[SetUp]
		public void SetUp()
		{
			Mock<IAPILog> loggerFake = new Mock<IAPILog>();

			_objectManagerMock = new Mock<IObjectManager>();
			_helperMock = new Mock<ICPHelper>();
			_helperMock.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<ObjectTypeController>()).Returns(loggerFake.Object);
			_helperMock.Setup(x => x.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser)).Returns(_objectManagerMock.Object);
			_sut = new ObjectTypeController(_helperMock.Object)
			{
				Request = new HttpRequestMessage()
			};

			_sut.Request.SetConfiguration(new HttpConfiguration());
		}

		[TestCase(2, 5, 11, true)]
		[TestCase(1, 5, 5, false)]
		[TestCase(1, 5, 6, true)]
		public async Task GetViews_ShouldRetrieveViews(int page, int pageSize, int totalResults, bool expectedHasMoreResults)
		{
			// Arrange
			const int rdoArtifactTypeId = 222;
			const string rdoName = "My RDO";
			const string search = "My";

			_objectManagerMock
				.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType &&
					q.Condition == $"'Artifact Type ID' == {rdoArtifactTypeId}"), 0, 1))
				.ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
					{
						new RelativityObject()
						{
							Name = rdoName
						}
					}
				});

			_objectManagerMock
				.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int)ArtifactType.View &&
					q.Condition == $"'Object Type' == '{rdoName}' AND 'Name' LIKE '%{search}%'"), (page - 1) * pageSize, pageSize))
				.ReturnsAsync(new QueryResult()
				{
					Objects = Enumerable.Range(0, pageSize).Select(x => new RelativityObject()
					{
						Name =  $"View {x}"
					}).ToList(),
					TotalCount = totalResults
				});

			// Act
			HttpResponseMessage response = await _sut.GetViews(_WORKSPACE_ID, rdoArtifactTypeId, search, page, pageSize).ConfigureAwait(false);

			// Assert
			ViewResultsModel viewResultsModel = await response.Content.ReadAsAsync<ViewResultsModel>().ConfigureAwait(false);
			viewResultsModel.Results.Count().Should().Be(pageSize);
			viewResultsModel.TotalResults.Should().Be(totalResults);
			viewResultsModel.HasMoreResults.Should().Be(expectedHasMoreResults);
		}

		[Test]
		public async Task GetView_ShouldRetrieveView()
		{
			// Arrange
			const int rdoArtifactTypeId = 222;
			const string rdoName = "My RDO";
			const int viewId = 333;
			const string viewName = "My View Name";

			_objectManagerMock
				.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType &&
					q.Condition == $"'Artifact Type ID' == {rdoArtifactTypeId}"), 0, 1))
				.ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
					{
						new RelativityObject()
						{
							Name = rdoName
						}
					}
				});

			_objectManagerMock
				.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(q =>
						q.ObjectType.ArtifactTypeID == (int)ArtifactType.View &&
						q.Condition == $"'Object Type' == '{rdoName}' AND 'Artifact ID' == {viewId}"), 0, 1))
				.ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
					{
						new RelativityObject()
						{
							ArtifactID = viewId,
							Name = viewName
						}
					}
				});

			// Act
			HttpResponseMessage response = await _sut.GetView(_WORKSPACE_ID, rdoArtifactTypeId, viewId).ConfigureAwait(false);

			// Assert
			ViewModel viewModel = await response.Content.ReadAsAsync<ViewModel>().ConfigureAwait(false);
			viewModel.DisplayName.Should().Be(viewName);
			viewModel.Value.Should().Be(viewId);
		}

		[Test]
		public async Task GetDestinationArtifactTypeID_ShouldReturnArtifactIdWhenExistsInDestinationWorkspace()
		{
			// Arrange
			const int destinationWorkspaceId = 222;
			const int rdoArtifactTypeId = 333;
			const int rdoArtifactId = 444;
			const string rdoName = "My RDO";

			_objectManagerMock
				.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType &&
					q.Condition == $"'Artifact Type ID' == {rdoArtifactTypeId}"), 0, 1))
				.ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
					{
						new RelativityObject()
						{
							Name = rdoName
						}
					}
				});

			_objectManagerMock
				.Setup(x => x.QueryAsync(destinationWorkspaceId, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType &&
					q.Condition == $"'Name' == '{rdoName}'"), 0, 1))
				.ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
					{
						new RelativityObject()
                        {
							ArtifactID = rdoArtifactId
                        }
					}
				});

			// Act
			int response = await _sut.GetDestinationArtifactTypeID(_WORKSPACE_ID, destinationWorkspaceId, rdoArtifactTypeId).ConfigureAwait(false);

			// Assert
			response.Should().Be(rdoArtifactId);
		}

		[Test]
		public async Task GetDestinationArtifactTypeID_ShouldNotReturnWhenDoesntExistsInDestinationWorkspace()
		{
			// Arrange
			const int destinationWorkspaceId = 222;
			const int rdoArtifactTypeId = 333;
			const string rdoName = "My RDO";

			_objectManagerMock
				.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType &&
					q.Condition == $"'Artifact Type ID' == {rdoArtifactTypeId}"), 0, 1))
				.ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
					{
						new RelativityObject()
						{
							Name = rdoName,
						}
					}
				});

			_objectManagerMock
				.Setup(x => x.QueryAsync(destinationWorkspaceId, It.Is<QueryRequest>(q =>
					q.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType &&
					q.Condition == $"'Name' == '{rdoName}'"), 0, 1))
				.ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
				});

			// Act
			int response = await _sut.GetDestinationArtifactTypeID(_WORKSPACE_ID, destinationWorkspaceId, rdoArtifactTypeId).ConfigureAwait(false);

			// Assert
			response.Should().Be(-1);
		}
	}
}
