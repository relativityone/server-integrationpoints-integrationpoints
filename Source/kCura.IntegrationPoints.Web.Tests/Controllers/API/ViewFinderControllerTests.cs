using FluentAssertions;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
	public class ViewFinderControllerTests
	{
		private const int _WORKSPACE_ID = 111;
		private Mock<ICPHelper> _helperMock;
		private Mock<IObjectManager> _objectManagerMock;

		private ViewFinderController _sut;

		[SetUp]
		public void SetUp()
		{
			_objectManagerMock = new Mock<IObjectManager>();
			_helperMock = new Mock<ICPHelper>();
			_helperMock.Setup(x => x.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser)).Returns(_objectManagerMock.Object);
			_sut = new ViewFinderController(_helperMock.Object)
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
	}
}
