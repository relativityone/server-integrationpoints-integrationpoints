using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.Interfaces.ViewField.Models;

namespace kCura.IntegrationPoints.Data.Tests.Repositories
{
	[TestFixture]
	public class ViewFieldRepositoryTests
	{
		private Mock<IViewFieldManager> _viewFieldManagerMock;
		private Mock<IExternalServiceInstrumentationProvider> _instrumentationProviderMock;
		private Mock<IExternalServiceSimpleInstrumentation> _instrumentationMock;

		private const int _WORKSPACE_ID = 1002345;
		private const int _ARTIFACT_TYPE_ID = 10;
		private const int _VIEW_ARTIFACT_ID = 1006789;

		[SetUp]
		public void SetUp()
		{
			_viewFieldManagerMock = new Mock<IViewFieldManager>();
			_instrumentationMock = new Mock<IExternalServiceSimpleInstrumentation>();
			_instrumentationProviderMock = new Mock<IExternalServiceInstrumentationProvider>();
			_instrumentationProviderMock.Setup(x => x.CreateSimple(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(_instrumentationMock.Object);
		}

		[Test]
		public void GetAllViewFieldsByArtifactTypeID_ReturnsProperResult_WhenCallIsSuccessful()
		{
			// arrange
			_instrumentationMock.Setup(x => x.Execute(It.IsAny<Func<ViewFieldResponse[]>>()))
				.Returns<Func<ViewFieldResponse[]>>(y => y.Invoke());

			var viewFieldResponse = new ViewFieldResponse();
			ViewFieldResponse[] expectedResult = { viewFieldResponse };
			Task<ViewFieldResponse[]> expectedResultTask = Task.FromResult(expectedResult);
			_viewFieldManagerMock.Setup(x => x.GetAllViewFieldsByArtifactTypeIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID))
				.Returns(expectedResultTask);
			var viewFieldRepository =
				new ViewFieldRepository(_viewFieldManagerMock.Object, _instrumentationProviderMock.Object, _WORKSPACE_ID);

			// act
			ViewFieldResponse[] actualResult = viewFieldRepository.GetAllViewFieldsByArtifactTypeID(_ARTIFACT_TYPE_ID);

			// assert
			_viewFieldManagerMock.Verify(x => x.GetAllViewFieldsByArtifactTypeIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID),
				Times.Once);
			_instrumentationProviderMock.Verify(
				x => x.CreateSimple(
					ExternalServiceTypes.KEPLER,
					nameof(IViewFieldManager),
					nameof(IViewFieldManager.GetAllViewFieldsByArtifactTypeIDAsync)),
				Times.Once);
			_instrumentationMock.Verify(x => x.Execute(It.IsAny<Func<ViewFieldResponse[]>>()), Times.Once);
			actualResult.Length.Should().Be(1);
			actualResult[0].Should().Be(expectedResult[0]);
		}

		[Test]
		public void GetAllViewFieldsByArtifactTypeID_IsInstrumentedProperly_WhenCallThrowsException()
		{
			// arrange
			_instrumentationMock.Setup(x => x.Execute(It.IsAny<Func<ViewFieldResponse[]>>()))
				.Returns<Func<ViewFieldResponse[]>>(y => y.Invoke());

			_viewFieldManagerMock.Setup(x => x.GetAllViewFieldsByArtifactTypeIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID))
				.Throws<Exception>();
			var viewFieldRepository =
				new ViewFieldRepository(_viewFieldManagerMock.Object, _instrumentationProviderMock.Object, _WORKSPACE_ID);

			// act
			Action action = () => viewFieldRepository.GetAllViewFieldsByArtifactTypeID(_ARTIFACT_TYPE_ID);

			// assert
			action.ShouldThrow<Exception>();
			_viewFieldManagerMock.Verify(x => x.GetAllViewFieldsByArtifactTypeIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID),
				Times.Once);
			_instrumentationProviderMock.Verify(
				x => x.CreateSimple(
					ExternalServiceTypes.KEPLER,
					nameof(IViewFieldManager),
					nameof(IViewFieldManager.GetAllViewFieldsByArtifactTypeIDAsync)),
				Times.Once);
			_instrumentationMock.Verify(x => x.Execute(It.IsAny<Func<ViewFieldResponse[]>>()), Times.Once);
		}

		[Test]
		public void GetViewFieldsByArtifactTypeIDAndViewArtifactID_ReturnsProperResult_WhenCallIsSuccessful()
		{
			// arrange
			_instrumentationMock.Setup(x => x.Execute(It.IsAny<Func<ViewFieldIDResponse[]>>()))
				.Returns<Func<ViewFieldIDResponse[]>>(y => y.Invoke());

			var viewFieldIDResponse = new ViewFieldIDResponse();
			ViewFieldIDResponse[] expectedResult = { viewFieldIDResponse };
			Task<ViewFieldIDResponse[]> expectedResultTask = Task.FromResult(expectedResult);
			_viewFieldManagerMock
				.Setup(x => x.GetViewFieldsByArtifactTypeIDAndViewArtifactIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID,
					_VIEW_ARTIFACT_ID, true)).Returns(expectedResultTask);
			var viewFieldRepository =
				new ViewFieldRepository(_viewFieldManagerMock.Object, _instrumentationProviderMock.Object, _WORKSPACE_ID);

			// act
			ViewFieldIDResponse[] actualResult =
				viewFieldRepository.GetViewFieldsByArtifactTypeIDAndViewArtifactID(_ARTIFACT_TYPE_ID, _VIEW_ARTIFACT_ID, true);

			// assert
			_viewFieldManagerMock.Verify(
				x => x.GetViewFieldsByArtifactTypeIDAndViewArtifactIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _VIEW_ARTIFACT_ID,
					true), Times.Once);
			_instrumentationProviderMock.Verify(
				x => x.CreateSimple(
					ExternalServiceTypes.KEPLER,
					nameof(IViewFieldManager),
					nameof(IViewFieldManager.GetViewFieldsByArtifactTypeIDAndViewArtifactIDAsync)),
				Times.Once);
			_instrumentationMock.Verify(x => x.Execute(It.IsAny<Func<ViewFieldIDResponse[]>>()), Times.Once);
			actualResult.Length.Should().Be(1);
			actualResult[0].Should().Be(expectedResult[0]);
		}

		[Test]
		public void GetViewFieldsByArtifactTypeIDAndViewArtifactID_IsInstrumentedProperly_WhenCallThrowsException()
		{
			// arrange
			_instrumentationMock.Setup(x => x.Execute(It.IsAny<Func<ViewFieldIDResponse[]>>()))
				.Returns<Func<ViewFieldIDResponse[]>>(y => y.Invoke());

			_viewFieldManagerMock
				.Setup(x => x.GetViewFieldsByArtifactTypeIDAndViewArtifactIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID,
					_VIEW_ARTIFACT_ID, true)).Throws<Exception>();
			var viewFieldRepository =
				new ViewFieldRepository(_viewFieldManagerMock.Object, _instrumentationProviderMock.Object, _WORKSPACE_ID);

			// act
			Action action = () =>
				viewFieldRepository.GetViewFieldsByArtifactTypeIDAndViewArtifactID(_ARTIFACT_TYPE_ID, _VIEW_ARTIFACT_ID, true);

			// assert
			action.ShouldThrow<Exception>();
			_viewFieldManagerMock.Verify(
				x => x.GetViewFieldsByArtifactTypeIDAndViewArtifactIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, _VIEW_ARTIFACT_ID,
					true), Times.Once);
			_instrumentationProviderMock.Verify(
				x => x.CreateSimple(
					ExternalServiceTypes.KEPLER,
					nameof(IViewFieldManager),
					nameof(IViewFieldManager.GetViewFieldsByArtifactTypeIDAndViewArtifactIDAsync)),
				Times.Once);
			_instrumentationMock.Verify(x => x.Execute(It.IsAny<Func<ViewFieldIDResponse[]>>()), Times.Once);
		}
	}
}
