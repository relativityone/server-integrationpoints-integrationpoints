using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
			_objectManagerFacadeMock = new Mock<Data.Facades.IObjectManagerFacade>();
			_objectManagerFacadeFactoryMock = new Mock<Data.Facades.IObjectManagerFacadeFactory>();
			_objectManagerFacadeFactoryMock
				.Setup(x => x.Create(It.IsAny<ExecutionIdentity>()))
				.Returns(_objectManagerFacadeMock.Object);
			_sut = new RelativityObjectManager(
				_WORKSPACE_ARTIFACT_ID,
				_apiLogMock.Object,
				_secretStoreHelperMock.Object,
				_objectManagerFacadeFactoryMock.Object);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StreamLongText_ShouldRethrowIntegrationPointException(bool isUnicode)
		{
			// arrange
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.Throws<IntegrationPointsException>();

			// action
			Action action = () =>
				_sut.StreamLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
					isUnicode,
					ExecutionIdentity.System);

			// assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void StreamLongText_ShouldThrowExceptionWrappedInIntegrationPointException(bool isUnicode)
		{
			// arrange
			_objectManagerFacadeMock
				.Setup(x =>
					x.StreamLongTextAsync(
						It.IsAny<int>(),
						It.IsAny<RelativityObjectRef>(),
						It.IsAny<FieldRef>()))
				.Throws<Exception>();

			// action
			Action action = () =>
				_sut.StreamLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
					isUnicode,
					ExecutionIdentity.System);

			// assert
			action.ShouldThrow<IntegrationPointsException>();
		}

		[Test]
		public void StreamLongText_ShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade_WhenFieldIsNotUnicode()
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

			// action
			Stream result = _sut.StreamLongText(
					_REL_OBJECT_ARTIFACT_ID,
					new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
					false,
					ExecutionIdentity.System);

			// assert
			result.Should().BeOfType<SelfDisposingStream>();
			var selfDisposingStream = (SelfDisposingStream)result;
			Stream innerStream = selfDisposingStream.InnerStream;
			innerStream.Should().BeOfType<SelfRecreatingStream>();
			var selfRecreatingStream = (SelfRecreatingStream)innerStream;
			selfRecreatingStream.InnerStream.Should().Be(expectedStream);
		}

		[Test]
		public void StreamLongText_ShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade_WhenFieldIsUnicode()
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

			// act
			Stream result = _sut.StreamLongText(
				_REL_OBJECT_ARTIFACT_ID,
				new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
				isUnicode: true,
				executionIdentity: ExecutionIdentity.System);

			// assert
			result.Should().BeOfType<SelfDisposingStream>();
			var selfDisposingStream = (SelfDisposingStream)result;
			Stream innerStream1 = selfDisposingStream.InnerStream;
			innerStream1.Should().BeOfType<AsciiToUnicodeStream>();
			var asciiToUnicodeStream = (AsciiToUnicodeStream)innerStream1;
			Stream innerStream2 = asciiToUnicodeStream.AsciiStream;
			innerStream2.Should().BeOfType<SelfRecreatingStream>();
			var selfRecreatingStream = (SelfRecreatingStream)innerStream2;
			selfRecreatingStream.InnerStream.Should().Be(expectedStream);
		}

		[Test]
		public void MassUpdateAsync_ShouldRethrowIntegrationPointException()
		{
			// arrange
			var expectedException = new IntegrationPointsException();
			_objectManagerFacadeMock
				.Setup(x =>
					x.UpdateAsync(
						It.IsAny<int>(),
						It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
						It.IsAny<MassUpdateOptions>()))
				.Throws(expectedException);

			// action
			Func<Task> massUpdateAction = () =>
				_sut.MassUpdateAsync(
					Enumerable.Empty<int>(),
					It.IsAny<IEnumerable<FieldRefValuePair>>(),
					It.IsAny<FieldUpdateBehavior>(),
					It.IsAny<ExecutionIdentity>());

			// assert
			massUpdateAction.ShouldThrow<IntegrationPointsException>()
				.Which.Should().Be(expectedException);
		}

		[Test]
		public void MassUpdateAsync_ShouldWrapExceptionInIntegrationPointException()
		{
			// arrange
			var expectedInnerException = new Exception();
			_objectManagerFacadeMock
				.Setup(x =>
					x.UpdateAsync(
						It.IsAny<int>(),
						It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
						It.IsAny<MassUpdateOptions>()))
				.Throws(expectedInnerException);

			// action
			Func<Task> massUpdateAction = () =>
				_sut.MassUpdateAsync(
					Enumerable.Empty<int>(),
					It.IsAny<IEnumerable<FieldRefValuePair>>(),
					It.IsAny<FieldUpdateBehavior>(),
					It.IsAny<ExecutionIdentity>());

			// assert
			massUpdateAction.ShouldThrow<IntegrationPointsException>()
				.Which.InnerException.Should().Be(expectedInnerException);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task MassUpdateAsync_ShouldReturnValueFromObjectManagerFacade(bool isSuccess)
		{
			// arrange
			var massUpdateResult = new MassUpdateResult
			{
				Success = isSuccess
			};
			_objectManagerFacadeMock
				.Setup(x =>
					x.UpdateAsync(
						It.IsAny<int>(),
						It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
						It.IsAny<MassUpdateOptions>()))
				.ReturnsAsync(massUpdateResult);

			// action
			bool actualResult = await _sut.MassUpdateAsync(
					Enumerable.Empty<int>(),
					It.IsAny<IEnumerable<FieldRefValuePair>>(),
					It.IsAny<FieldUpdateBehavior>(),
					It.IsAny<ExecutionIdentity>())
				.ConfigureAwait(false);

			// assert
			actualResult.Should().Be(isSuccess);
		}

		[Test]
		public async Task MassUpdateAsync_ShouldSendProperRequest()
		{
			// arrange
			var massUpdateResult = new MassUpdateResult
			{
				Success = true
			};

			IList<int> objectIDs = Enumerable.Range(0, 5).ToList();

			FieldRefValuePair[] fields =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						ArtifactID = 1
					},
					Value = "one"
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						ArtifactID = 2
					},
					Value = "two"
				}
			};

			FieldUpdateBehavior updateBehavior = FieldUpdateBehavior.Merge;

			_objectManagerFacadeMock
				.Setup(x =>
					x.UpdateAsync(
						It.IsAny<int>(),
						It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
						It.IsAny<MassUpdateOptions>()))
				.ReturnsAsync(massUpdateResult);

			// action
			await _sut.MassUpdateAsync(
					objectIDs,
					fields,
					updateBehavior,
					It.IsAny<ExecutionIdentity>())
				.ConfigureAwait(false);

			// assert
			Func<MassUpdateByObjectIdentifiersRequest, bool> requestVerifier = request =>
			{
				bool isValid = true;
				isValid &= request.Objects.Select(x => x.ArtifactID).SequenceEqual(objectIDs);
				isValid &= request.FieldValues.SequenceEqual(fields);
				return isValid;
			};

			Func<MassUpdateOptions, bool> updateOptionsVerifier = options =>
				options.UpdateBehavior == updateBehavior;

			_objectManagerFacadeMock.Verify(x => x.UpdateAsync(
				_WORKSPACE_ARTIFACT_ID,
				It.Is<MassUpdateByObjectIdentifiersRequest>(request => requestVerifier(request)),
				It.Is<MassUpdateOptions>(options => updateOptionsVerifier(options)))
			);
		}
	}
}
