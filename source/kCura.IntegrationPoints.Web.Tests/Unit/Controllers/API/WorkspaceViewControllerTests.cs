using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers.API
{
	class WorkspaceViewControllerTests
	{
		#region Fields

		private WorkspaceViewController _subjectUnderTest;

		private IRepositoryFactory _repositoryFactoryMock;
		private IErrorRepository _errorRepositoryMock;
		private IViewService _viewServiceMock;

		private const int _WORKSPACE_ID = 12345;
		private const int _ARTIFAC_TYPE_ID = 10;

		private const int _VIEW_ARTIFACT_ID = 10;
		private const string _VIEW_NAME = "View";
		private const bool _VIEW_AVAILABLE = true;

		private List<ViewDTO> _views = new List<ViewDTO>()
		{
			new ViewDTO()
			{
				ArtifactId = _VIEW_ARTIFACT_ID,
				Name = _VIEW_NAME,
				IsAvailableInObjectTab = _VIEW_AVAILABLE
			}
		};

		#endregion //Fields

		[SetUp]
		public void Init()
		{
			_repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
			_errorRepositoryMock = Substitute.For<IErrorRepository>();
			_viewServiceMock = Substitute.For<IViewService>();

			_repositoryFactoryMock.GetErrorRepository().Returns(_errorRepositoryMock);

			_subjectUnderTest = new WorkspaceViewController(_viewServiceMock, _repositoryFactoryMock)
			{
				Request = new HttpRequestMessage()
			};

			_subjectUnderTest.Request.SetConfiguration(new HttpConfiguration());
		}

		[Test]
		public void ItShouldGetViews()
		{
			// Arrange

			_viewServiceMock.GetViewsByWorkspaceAndArtifactType(_WORKSPACE_ID, _ARTIFAC_TYPE_ID).Returns(_views);

			// Act
			HttpResponseMessage httpResponseMessage = _subjectUnderTest
				.GetViewsByWorkspaceAndArtifactType(_WORKSPACE_ID, _ARTIFAC_TYPE_ID);

			// Assert
			List<ViewDTO> retValue;
			httpResponseMessage.TryGetContentValue(out retValue);

			Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			CollectionAssert.AreEquivalent(_views, retValue);
		}
	}
}
