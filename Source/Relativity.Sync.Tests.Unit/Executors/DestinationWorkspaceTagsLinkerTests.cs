using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class DestinationWorkspaceTagsLinkerTests
	{
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;

		private DestinationWorkspaceTagLinker _sut;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();

			_sut = new DestinationWorkspaceTagLinker( new ConfigurationStub(), _serviceFactory.Object,new EmptyLogger());
		}

		[Test]
		public async Task ItShouldUpdateTagUsingObjectManager()
		{
			const int sourceWorkspaceArtifactId = 1;
			const int destinationWorkspaceTagArtifactId = 2;
			const int jobArtifactId = 3;
			var objectManager = new Mock<IObjectManager>();
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			// act
			await _sut.LinkDestinationWorkspaceTagToJobHistoryAsync(sourceWorkspaceArtifactId, destinationWorkspaceTagArtifactId, jobArtifactId).ConfigureAwait(false);

			// assert
			objectManager.Verify(x => x.UpdateAsync(sourceWorkspaceArtifactId, It.Is<UpdateRequest>(request => VerifyUpdateRequest(request, destinationWorkspaceTagArtifactId, jobArtifactId))));
		}

		[Test]
		public void ItShouldRethrowWhenLinkFails()
		{
			Mock<IObjectManager> objectManager = new Mock<IObjectManager>();
			objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>())).Throws<InvalidOperationException>();
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			// act
			Func<Task> action = async () => await _sut.LinkDestinationWorkspaceTagToJobHistoryAsync(0, 0, 0).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagsLinkerException>();
		}

		private bool VerifyUpdateRequest(UpdateRequest request, int destinationWorkspaceTagArtifactId, int jobArtifactId)
		{
			Guid destinationWorkspaceInformationGuid = Guid.Parse("20a24c4e-55e8-4fc2-abbe-f75c07fad91b");
			List<FieldRefValuePair> fieldValues = request.FieldValues.ToList();

			return request.Object.ArtifactID == jobArtifactId &&
				fieldValues.Count == 1 &&
				fieldValues[0].Field.Guid.Equals(destinationWorkspaceInformationGuid) &&
				fieldValues[0].Value is RelativityObjectValue[] &&
				(fieldValues[0].Value as RelativityObjectValue[])[0].ArtifactID == destinationWorkspaceTagArtifactId;
		}
	}
}