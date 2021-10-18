using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class SyncObjectTypeManagerTests
	{
		private Mock<IDestinationServiceFactoryForAdmin> _serviceFactory;
		private Mock<IArtifactGuidManager> _artifactGuidManager;
		private Mock<IObjectManager> _objectManager;
		private Mock<IObjectTypeManager> _objectTypeManager;
		private SyncObjectTypeManager _instance;
		private Guid _guid;

		private const int _WORKSPACE_ID = 1;
		private const int _ARTIFACT_ID = 2;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<IDestinationServiceFactoryForAdmin>();
			_artifactGuidManager = new Mock<IArtifactGuidManager>();
			_objectManager = new Mock<IObjectManager>();
			_objectTypeManager = new Mock<IObjectTypeManager>();
			_serviceFactory.Setup(x => x.CreateProxyAsync<IArtifactGuidManager>()).ReturnsAsync(_artifactGuidManager.Object);
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectTypeManager>()).ReturnsAsync(_objectTypeManager.Object);
			_instance = new SyncObjectTypeManager(_serviceFactory.Object, new EmptyLogger());
			_guid = Guid.NewGuid();
		}

		[Test]
		public async Task ItShouldReadExistingObjectTypeArtifactId()
		{
			const int artifactId = 2;
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(true);
			_artifactGuidManager.Setup(x => x.ReadSingleArtifactIdAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(artifactId);

			// act
			int actualArtifactId = await _instance.EnsureObjectTypeExistsAsync(_WORKSPACE_ID, _guid, new ObjectTypeRequest()).ConfigureAwait(false);

			// assert
			actualArtifactId.Should().Be(artifactId);
		}

		[Test]
		public async Task ItShouldReturnArtifactIdOfObjectTypeByName()
		{
			const int artifactId = 2;
			const string name = "Fancy Object Type";

			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						ArtifactID = artifactId
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(request =>
				request.ObjectType.ArtifactTypeID == (int) ArtifactType.ObjectType &&
				request.Condition.Contains(name)), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);
			
			// act
			int actualArtifactId = await _instance.EnsureObjectTypeExistsAsync(_WORKSPACE_ID, _guid, new ObjectTypeRequest()
			{
				Name = name
			}).ConfigureAwait(false);

			// assert
			actualArtifactId.Should().Be(artifactId);
		}

		[Test]
		public async Task ItShouldCreateNewObjectType()
		{
			const int artifactId = 2;
			const string name = "My Object Type";
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);
			_objectTypeManager.Setup(x => x.CreateAsync(_WORKSPACE_ID, It.Is<ObjectTypeRequest>(request => request.Name == name))).ReturnsAsync(artifactId);

			// act
			int actualArtifactId = await _instance.EnsureObjectTypeExistsAsync(_WORKSPACE_ID, _guid, new ObjectTypeRequest()
			{
				Name = name
			}).ConfigureAwait(false);

			// assert
			actualArtifactId.Should().Be(artifactId);
		}

		[Test]
		public async Task ItShouldAssignGuid()
		{
			const int artifactId = 2;
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);
			_objectTypeManager.Setup(x => x.CreateAsync(_WORKSPACE_ID, It.IsAny<ObjectTypeRequest>())).ReturnsAsync(artifactId);

			// act
			int actualArtifactId = await _instance.EnsureObjectTypeExistsAsync(_WORKSPACE_ID, _guid, new ObjectTypeRequest()).ConfigureAwait(false);

			// assert
			actualArtifactId.Should().Be(artifactId);
			_artifactGuidManager.Verify(x => x.CreateSingleAsync(_WORKSPACE_ID, artifactId, It.Is<List<Guid>>(guids => guids.Contains(_guid))));
		}

		[Test]
		public async Task ItShouldReturnArtifactTypeIdByArtifactId()
		{
			const int artifactTypeId = 2;

			_objectTypeManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.Is<int>(artifactID => artifactID == _ARTIFACT_ID)))
				.ReturnsAsync(new ObjectTypeResponse()
				{
					ArtifactTypeID = artifactTypeId
				}) ;

			// act
			int actualArtifactTypeId = await _instance.GetObjectTypeArtifactTypeIdAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// assert
			actualArtifactTypeId.Should().Be(artifactTypeId);
		}
	}
}