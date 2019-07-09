using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class SyncFieldManagerTests
	{
		private Mock<IDestinationServiceFactoryForAdmin> _serviceFactory;
		private Mock<IArtifactGuidManager> _artifactGuidManager;
		private Mock<IObjectManager> _objectManager;
		private Mock<IFieldManager> _fieldManager;
		private SyncFieldManager _instance;
		private Guid _guid;

		private const int _WORKSPACE_ID = 1;

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<IDestinationServiceFactoryForAdmin>();
			_artifactGuidManager = new Mock<IArtifactGuidManager>();
			_objectManager = new Mock<IObjectManager>();
			_fieldManager = new Mock<IFieldManager>();
			_serviceFactory.Setup(x => x.CreateProxyAsync<IArtifactGuidManager>()).ReturnsAsync(_artifactGuidManager.Object);
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_serviceFactory.Setup(x => x.CreateProxyAsync<IFieldManager>()).ReturnsAsync(_fieldManager.Object);
			_instance = new SyncFieldManager(_serviceFactory.Object, new EmptyLogger());
			_guid = Guid.NewGuid();
		}

		[Test]
		public async Task ItShouldReadExistingObjectTypeGuid()
		{
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(true);

			// act
			await _instance.EnsureFieldsExistAsync(_WORKSPACE_ID, new Dictionary<Guid, BaseFieldRequest>()).ConfigureAwait(false);

			// assert
			_objectManager.Verify(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
			_fieldManager.Verify(x => x.CreateWholeNumberFieldAsync(It.IsAny<int>(), It.IsAny<WholeNumberFieldRequest>()), Times.Never);
			_fieldManager.Verify(x => x.CreateFixedLengthFieldAsync(It.IsAny<int>(), It.IsAny<FixedLengthFieldRequest>()), Times.Never);
			_fieldManager.Verify(x => x.CreateMultipleObjectFieldAsync(It.IsAny<int>(), It.IsAny<MultipleObjectFieldRequest>()), Times.Never);
			_artifactGuidManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<Guid>>()), Times.Never);
		}

		[Test]
		public async Task ItShouldQueryExistingFieldByName()
		{
			const int artifactId = 2;
			const string name = "Fancy Field";
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						ArtifactID = artifactId,
						Name = name
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(request =>
				request.ObjectType.ArtifactTypeID == (int)ArtifactType.Field &&
				request.Condition.Contains(name)), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult).Verifiable();
			BaseFieldRequest fieldRequest = new BaseFieldRequest()
			{
				Name = name
			};

			// act
			await _instance.EnsureFieldsExistAsync(_WORKSPACE_ID, new Dictionary<Guid, BaseFieldRequest>() { { _guid, fieldRequest } }).ConfigureAwait(false);

			// assert
			_objectManager.Verify();
		}

		[Test]
		public async Task ItShouldAssignGuid()
		{
			const int artifactId = 2;
			const string name = "Fancy Field";
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						ArtifactID = artifactId,
						Name = name
					}
				}
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(request =>
				request.ObjectType.ArtifactTypeID == (int)ArtifactType.Field &&
				request.Condition.Contains(name)), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult).Verifiable();
			BaseFieldRequest fieldRequest = new BaseFieldRequest()
			{
				Name = name
			};

			// act
			await _instance.EnsureFieldsExistAsync(_WORKSPACE_ID, new Dictionary<Guid, BaseFieldRequest>() { { _guid, fieldRequest } }).ConfigureAwait(false);

			// assert
			_objectManager.Verify();
			_artifactGuidManager.Verify(x => x.CreateSingleAsync(_WORKSPACE_ID, artifactId, It.Is<List<Guid>>(list => list.Contains(_guid))));
		}

		[Test]
		public async Task ItShouldCreateNewWholeNumberField()
		{
			const string name = "My Field";
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(request =>
				request.ObjectType.ArtifactTypeID == (int)ArtifactType.Field &&
				request.Condition.Contains(name)), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult).Verifiable();
			WholeNumberFieldRequest fieldRequest = new WholeNumberFieldRequest()
			{
				Name = name
			};

			// act
			await _instance.EnsureFieldsExistAsync(_WORKSPACE_ID, new Dictionary<Guid, BaseFieldRequest>() { { _guid, fieldRequest } }).ConfigureAwait(false);

			// assert
			_objectManager.Verify();
			_fieldManager.Verify(x => x.CreateWholeNumberFieldAsync(_WORKSPACE_ID, fieldRequest));
		}

		[Test]
		public async Task ItShouldCreateNewFixedLengthTextField()
		{
			const string name = "My Field";
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(request =>
				request.ObjectType.ArtifactTypeID == (int)ArtifactType.Field &&
				request.Condition.Contains(name)), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult).Verifiable();
			FixedLengthFieldRequest fieldRequest = new FixedLengthFieldRequest()
			{
				Name = name
			};

			// act
			await _instance.EnsureFieldsExistAsync(_WORKSPACE_ID, new Dictionary<Guid, BaseFieldRequest>() { { _guid, fieldRequest } }).ConfigureAwait(false);

			// assert
			_objectManager.Verify();
			_fieldManager.Verify(x => x.CreateFixedLengthFieldAsync(_WORKSPACE_ID, fieldRequest));
		}

		[Test]
		public async Task ItShouldCreateNewMultipleObjectField()
		{
			const string name = "My Field";
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(request =>
				request.ObjectType.ArtifactTypeID == (int)ArtifactType.Field &&
				request.Condition.Contains(name)), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult).Verifiable();
			MultipleObjectFieldRequest fieldRequest = new MultipleObjectFieldRequest()
			{
				Name = name
			};

			// act
			await _instance.EnsureFieldsExistAsync(_WORKSPACE_ID, new Dictionary<Guid, BaseFieldRequest>() { { _guid, fieldRequest } }).ConfigureAwait(false);

			// assert
			_objectManager.Verify();
			_fieldManager.Verify(x => x.CreateMultipleObjectFieldAsync(_WORKSPACE_ID, fieldRequest));
		}

		[Test]
		public void ItShouldThrowExceptionWhenCreatingUnsupportedFieldType()
		{
			const string name = "My Field";
			_artifactGuidManager.Setup(x => x.GuidExistsAsync(_WORKSPACE_ID, _guid)).ReturnsAsync(false);
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(request =>
				request.ObjectType.ArtifactTypeID == (int)ArtifactType.Field &&
				request.Condition.Contains(name)), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult).Verifiable();
			SingleChoiceFieldRequest fieldRequest = new SingleChoiceFieldRequest()
			{
				Name = name
			};

			// act
			Func<Task> action = async () => await _instance.EnsureFieldsExistAsync(_WORKSPACE_ID, new Dictionary<Guid, BaseFieldRequest>() { { _guid, fieldRequest } }).ConfigureAwait(false);

			// assert
			action.Should().Throw<NotSupportedException>();
		}

		[Test]
		public void ItShouldNotThrowWhenPassingNullDictionary()
		{
			// act
			Func<Task> action = async () => await _instance.EnsureFieldsExistAsync(_WORKSPACE_ID, null).ConfigureAwait(false);

			// assert
			action.Should().NotThrow();
		}
	}
}