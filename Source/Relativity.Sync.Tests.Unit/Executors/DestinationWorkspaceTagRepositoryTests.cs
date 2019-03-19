using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class DestinationWorkspaceTagRepositoryTests
	{
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;
		private Mock<IFederatedInstance> _federatedInstance;
		private Mock<ISyncLog> _logger;

		private DestinationWorkspaceTagRepository _sut;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_federatedInstance = new Mock<IFederatedInstance>();
			_logger = new Mock<ISyncLog>();

			_sut = new DestinationWorkspaceTagRepository(_serviceFactory.Object, _federatedInstance.Object, _logger.Object);
		}

		[Test]
		public async Task ItShouldReadExistingDestinationWorkspaceTag()
		{
			Guid destinationWorkspaceNameGuid = new Guid("348d7394-2658-4da4-87d0-8183824adf98");
			Guid destinationInstanceNameGuid = new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");
			Guid destinationWorkspaceArtifactIdGuid = new Guid("207e6836-2961-466b-a0d2-29974a4fad36");

			Mock<IObjectManager> objectManager = new Mock<IObjectManager>();
			QueryResult queryResult = new QueryResult();
			const int destinationWorkspaceId = 2;
			RelativityObject relativityObject = new RelativityObject
			{
				ArtifactID = 1,
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field(){Guids = new List<Guid>(){ destinationInstanceNameGuid}},
						Value = "destination instance"
					},
					new FieldValuePair()
					{
						Field = new Field(){Guids = new List<Guid>(){ destinationWorkspaceNameGuid}},
						Value = "destination workspace"
					},
					new FieldValuePair()
					{
						Field = new Field(){Guids = new List<Guid>(){ destinationWorkspaceArtifactIdGuid}},
						Value = destinationWorkspaceId
					}
				}
			};

			queryResult.Objects.Add(relativityObject);
			objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			// act
			DestinationWorkspaceTag tag = await _sut.ReadAsync(0, 0).ConfigureAwait(false);

			// assert
			Assert.AreEqual(relativityObject.ArtifactID, tag.ArtifactId);
		}
	}
}