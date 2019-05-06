using System;
using System.IO;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.StreamWrappers;
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
			_apiLogMock.Setup(x => x.ForContext<SelfDisposingStream>()).Returns(_apiLogMock.Object);
			_apiLogMock.Setup(x => x.ForContext<SelfRecreatingStream>()).Returns(_apiLogMock.Object);
			_secretStoreHelperMock = new Mock<ISecretStoreHelper>();
			_objectManagerFacadeFactoryMock = new Mock<Data.Facades.IObjectManagerFacadeFactory>();
			_objectManagerFacadeMock = new Mock<Data.Facades.IObjectManagerFacade>();
			_sut = new RelativityObjectManager(
				_WORKSPACE_ARTIFACT_ID,
				_apiLogMock.Object,
				_secretStoreHelperMock.Object,
				_objectManagerFacadeFactoryMock.Object);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StreamLongText_ItShouldRethrowIntegrationPointException(bool isUnicode)
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

			Action action = () =>
				_sut.StreamLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
					isUnicode,
					ExecutionIdentity.System);

			action.ShouldThrow<IntegrationPointsException>();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StreamLongText_ItShouldThrowExceptionWrappedInIntegrationPointException(bool isUnicode)
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

			Action action = () =>
				_sut.StreamLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
					isUnicode,
					ExecutionIdentity.System);

			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void StreamLongText_ItShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade_WhenFieldIsNotUnicode()
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

			Stream result = _sut.StreamLongText(
				_REL_OBJECT_ARTIFACT_ID,
				new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
				false,
				ExecutionIdentity.System);

			result.Should().BeOfType<SelfDisposingStream>();
			var selfDisposingStream = (SelfDisposingStream)result;
			Stream innerStream = selfDisposingStream.InnerStream;
			innerStream.Should().BeOfType<SelfRecreatingStream>();
			var selfRecreatingStream = (SelfRecreatingStream)innerStream;
			selfRecreatingStream.InnerStream.Should().Be(expectedStream);
		}

		[Test]
		public void StreamLongText_ItShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade_WhenFieldIsUnicode()
		{
			// arrange
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

			// act
			Stream result = _sut.StreamLongText(
				_REL_OBJECT_ARTIFACT_ID,
				new FieldRef { ArtifactID = _FIELD_ARTIFACT_ID },
				isUnicode: true,
				executionIdentity: ExecutionIdentity.System);

			result.Should().BeOfType<SelfDisposingStream>();
			var selfDisposingStream = (SelfDisposingStream) result;
			Stream innerStream1 = selfDisposingStream.InnerStream;
			innerStream1.Should().BeOfType<AsciiToUnicodeStream>();
			var asciiToUnicodeStream = (AsciiToUnicodeStream) innerStream1;
			Stream innerStream2 = asciiToUnicodeStream.AsciiStream;
			innerStream2.Should().BeOfType<SelfRecreatingStream>();
			var selfRecreatingStream = (SelfRecreatingStream) innerStream2;
			selfRecreatingStream.InnerStream.Should().Be(expectedStream);
		}
	}
}
