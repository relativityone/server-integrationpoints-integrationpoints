using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class RelativityObjectManagerTests
	{
		private Mock<IAPILog> _apiLogMock;
		private Mock<ISecretStoreHelper> _secretStoreHelperMock;
		private Mock<Data.Facades.IObjectManagerFacadeFactory> _objectManagerFacadeFactoryMock;
		private Mock<Data.Facades.IObjectManagerFacade> _objectManagerFacadeMock;
		private RelativityObjectManager _sut;

		private const int _WORKSPACE_ARTIFACT_ID = 12345;
		private const int _REL_OBJECT_ARTIFACT_ID = 10;
		private const int _FIELD_ARTIFACT_ID = 789;

		[SetUp]
		public void SetUp()
		{
			_apiLogMock = new Mock<IAPILog>();
			_apiLogMock.Setup(x => x.ForContext<RelativityObjectManager>()).Returns(_apiLogMock.Object);
			_secretStoreHelperMock = new Mock<ISecretStoreHelper>();
			_objectManagerFacadeFactoryMock = new Mock<Data.Facades.IObjectManagerFacadeFactory>();
			_objectManagerFacadeMock = new Mock<Data.Facades.IObjectManagerFacade>();
			_sut = new RelativityObjectManager(
				_WORKSPACE_ARTIFACT_ID,
				_apiLogMock.Object,
				_secretStoreHelperMock.Object,
				_objectManagerFacadeFactoryMock.Object);
		}

		[Test]
		public void StreamLongTextAsync_ItShouldRethrowIntegrationPointException()
		{
			_objectManagerFacadeMock
				.Setup(x => 
					x.StreamLongTextAsync(
						It.IsAny<int>(), 
						It.IsAny<RelativityObjectRef>(), 
						It.IsAny<FieldRef>()))
				.Throws<IntegrationPointsException>();

			_objectManagerFacadeFactoryMock
				.Setup(x => x.Create(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerFacadeMock.Object);

			Func<Task> action = async () => 
				await _sut.StreamLongTextAsync(
					_REL_OBJECT_ARTIFACT_ID,
					_FIELD_ARTIFACT_ID,
					ExecutionIdentity.System);

			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void StreamLongTextAsync_ItShouldThrowExceptionWrappedInIntegrationPointException()
		{
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.Throws<Exception>();

			_objectManagerFacadeFactoryMock
				.Setup(x => x.Create(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerFacadeMock.Object);

			Func<Task> action = async () => 
				await _sut.StreamLongTextAsync(
					_REL_OBJECT_ARTIFACT_ID,
					_FIELD_ARTIFACT_ID,
					ExecutionIdentity.System);

			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public async Task StreamLongTextAsync_ItShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade()
		{
			Stream expectedStream = new Mock<Stream>().Object;
			var keplerStreamMock = new Mock<IKeplerStream>();
			keplerStreamMock.Setup(x => x.GetStreamAsync()).ReturnsAsync(expectedStream);
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.ReturnsAsync(keplerStreamMock.Object);

			_objectManagerFacadeFactoryMock
				.Setup(x => x.Create(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerFacadeMock.Object);

			Stream result = await _sut.StreamLongTextAsync(
					_REL_OBJECT_ARTIFACT_ID,
					_FIELD_ARTIFACT_ID,
					ExecutionIdentity.System);

			result.Should().Be(expectedStream);
		}

	}
}
