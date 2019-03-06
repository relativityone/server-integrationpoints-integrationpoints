using FluentAssertions;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Helpers;
using Moq;
using NUnit.Framework;
using System.Web;

namespace kCura.IntegrationPoints.Web.Tests.Helpers
{
	[TestFixture]
	public class RelativityUrlHelperTests
	{
		private const string _APPLICATION_ROOT_PATH = "/IntegrationPoints";
		private const string _OBJECT_TYPE_NAME = "Documents";
		private const int _OBJECT_TYPE_ID = 53421;
		private const int _WORKSPACE_ID = 842123;
		private const int _ARTIFACT_ID = 3249343;

		[Test]
		public void ShouldReturnProperViewUrl()
		{
			// arrange
			var httpRequest = new Mock<HttpRequestBase>();
			httpRequest.Setup(x => x.ApplicationPath).Returns(_APPLICATION_ROOT_PATH);

			var objectTypeRepositoryMock = new Mock<IObjectTypeRepository>();
			objectTypeRepositoryMock
				.Setup(x => x.GetObjectTypeID(_OBJECT_TYPE_NAME))
				.Returns(_OBJECT_TYPE_ID);
			var objectTypeService = new ObjectTypeService(objectTypeRepositoryMock.Object);

			var sut = new RelativityUrlHelper(httpRequest.Object, objectTypeService);

			// act
			string actualViewUrl = sut.GetRelativityViewUrl(_WORKSPACE_ID, _ARTIFACT_ID, _OBJECT_TYPE_NAME);

			// assert
			string expectedViewUrl = $"/IntegrationPoints/Case/Mask/View.aspx?AppID={_WORKSPACE_ID}&ArtifactID={_ARTIFACT_ID}&ArtifactTypeID={_OBJECT_TYPE_ID}";
			actualViewUrl.Should().Be(expectedViewUrl);
		}
	}
}
