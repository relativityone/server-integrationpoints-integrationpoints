using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Exceptions;
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
		private Mock<ITagNameFormatter> _tagNameFormatter;
		private Mock<IObjectManager> _objectManager;

		private DestinationWorkspaceTagRepository _sut;
		private static readonly Guid _destinationInstanceArtifactIdGuid = new Guid("323458db-8a06-464b-9402-af2516cf47e0");

		[SetUp]
		public void SetUp()
		{
			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_federatedInstance = new Mock<IFederatedInstance>();
			_logger = new Mock<ISyncLog>();
			_objectManager = new Mock<IObjectManager>();
			_tagNameFormatter = new Mock<ITagNameFormatter>();
			_tagNameFormatter.Setup(x => x.FormatWorkspaceDestinationTagName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("foo bar");
			_serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_sut = new DestinationWorkspaceTagRepository(_serviceFactory.Object, _federatedInstance.Object, _tagNameFormatter.Object, _logger.Object);
		}

		[Test]
		public async Task ItShouldReadExistingDestinationWorkspaceTag()
		{
			Guid destinationWorkspaceNameGuid = new Guid("348d7394-2658-4da4-87d0-8183824adf98");
			Guid destinationInstanceNameGuid = new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");
			Guid destinationWorkspaceArtifactIdGuid = new Guid("207e6836-2961-466b-a0d2-29974a4fad36");

			const int destinationWorkspaceId = 2;
			const string destinationInstanceName = "destination instance";
			const string destinationWorkspaceName = "destination workspace";

			QueryResult queryResult = new QueryResult();
			RelativityObject relativityObject = new RelativityObject
			{
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field() {Guids = new List<Guid>() {destinationInstanceNameGuid}},
						Value = destinationInstanceName
					},
					new FieldValuePair()
					{
						Field = new Field() {Guids = new List<Guid>() {destinationWorkspaceNameGuid}},
						Value = destinationWorkspaceName
					},
					new FieldValuePair()
					{
						Field = new Field() {Guids = new List<Guid>() {destinationWorkspaceArtifactIdGuid}},
						Value = destinationWorkspaceId
					}
				}
			};

			queryResult.Objects.Add(relativityObject);
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);

			// act
			DestinationWorkspaceTag tag = await _sut.ReadAsync(0, 0, CancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.AreEqual(relativityObject.ArtifactID, tag.ArtifactId);
			Assert.AreEqual(destinationWorkspaceId, tag.DestinationWorkspaceArtifactId);
			Assert.AreEqual(destinationWorkspaceName, tag.DestinationWorkspaceName);
			Assert.AreEqual(destinationInstanceName, tag.DestinationInstanceName);
		}

		[Test]
		public async Task ItShouldReturnNullWhenReadingNotExistingTag()
		{
			QueryResult queryResult = new QueryResult();
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), 
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);

			// act
			DestinationWorkspaceTag tag = await _sut.ReadAsync(0, 0, CancellationToken.None).ConfigureAwait(false);

			// assert
			Assert.IsNull(tag);
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenReadServiceCallFails()
		{
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).Throws<ServiceException>();

			// act
			Func<Task> action = async () => await _sut.ReadAsync(0, 0, CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagRepositoryException>().WithInnerException<ServiceException>();
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenReadFails()
		{
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.ReadAsync(0, 0, CancellationToken.None).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagRepositoryException>().WithInnerException<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldCreateDestinationWorkspaceTag()
		{
			const int tagArtifactId = 1;
			const int sourceWorkspaceArtifactId = 2;
			const int destinationWorkspaceArtifactId = 3;
			const string destinationWorkspaceName = "workspace";
			const string destinationInstanceName = "instance";

			CreateResult createResult = new CreateResult()
			{
				Object = new RelativityObject()
				{
					ArtifactID = tagArtifactId,
				}
			};
			_objectManager.Setup(x => x.CreateAsync(sourceWorkspaceArtifactId, It.IsAny<CreateRequest>())).ReturnsAsync(createResult);
			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(destinationInstanceName);

			// act
			DestinationWorkspaceTag createdTag = await _sut.CreateAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);

			// assert
			Assert.AreEqual(tagArtifactId, createdTag.ArtifactId);
			Assert.AreEqual(destinationWorkspaceName, createdTag.DestinationWorkspaceName);
			Assert.AreEqual(destinationInstanceName, createdTag.DestinationInstanceName);
			Assert.AreEqual(destinationWorkspaceArtifactId, createdTag.DestinationWorkspaceArtifactId);
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenCreatingTagServiceCallFails()
		{
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<ServiceException>();

			// act
			Func<Task> action = async () => await _sut.CreateAsync(0, 0, string.Empty).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagRepositoryException>().WithInnerException<ServiceException>();
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenCreatingTagFails()
		{
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.CreateAsync(0, 0, string.Empty).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagRepositoryException>().WithInnerException<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldUpdateTag()
		{
			Guid nameGuid = new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5");
			Guid destinationWorkspaceNameGuid = new Guid("348d7394-2658-4da4-87d0-8183824adf98");
			Guid destinationInstanceNameGuid = new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");
			Guid destinationWorkspaceArtifactidGuid = new Guid("207e6836-2961-466b-a0d2-29974a4fad36");

			const int tagArtifactId = 1;
			const int sourceWorkspaceArtifactId = 2;
			const int destinationWorkspaceArtifactId = 3;
			const int destinationInstanceArtifactId = 4;
			const string destinationInstanceName = "instance";
			const string destinationWorkspaceName = "workspace";
			string destinationTagName = $"{destinationInstanceName} - {destinationWorkspaceName} - {destinationWorkspaceArtifactId}";
			_tagNameFormatter.Setup(x => x.FormatWorkspaceDestinationTagName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(destinationTagName);

			DestinationWorkspaceTag destinationWorkspaceTag = new DestinationWorkspaceTag()
			{
				ArtifactId = tagArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				DestinationWorkspaceName = destinationWorkspaceName,
				DestinationInstanceName = destinationInstanceName,
			};

			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(destinationInstanceName);
			_federatedInstance.Setup(x => x.GetInstanceIdAsync()).ReturnsAsync(destinationInstanceArtifactId);

			// act
			await _sut.UpdateAsync(sourceWorkspaceArtifactId, destinationWorkspaceTag).ConfigureAwait(false);

			// assert

			_objectManager.Verify(x => x.UpdateAsync(sourceWorkspaceArtifactId, It.Is<UpdateRequest>(request => 
				VerifyUpdateRequest(request, tagArtifactId,
					f => nameGuid.Equals(f.Field.Guid) && string.Equals(f.Value, destinationTagName),
					f => destinationWorkspaceNameGuid.Equals(f.Field.Guid) && string.Equals(f.Value, destinationWorkspaceName),
					f => destinationWorkspaceArtifactidGuid.Equals(f.Field.Guid) && Equals(f.Value, destinationWorkspaceArtifactId),
					f => destinationInstanceNameGuid.Equals(f.Field.Guid) && string.Equals(f.Value, destinationInstanceName),
					f => _destinationInstanceArtifactIdGuid.Equals(f.Field.Guid) && Equals(f.Value, destinationInstanceArtifactId)))));
		}

		private bool VerifyUpdateRequest(UpdateRequest request, int tagArtifactId, params Predicate<FieldRefValuePair>[] predicates)
		{
			List<FieldRefValuePair> fields = request.FieldValues.ToList();
			bool checkPredicates = true;
			foreach (Predicate<FieldRefValuePair> predicate in predicates)
			{
				checkPredicates = checkPredicates && fields.Exists(predicate);
			}
			return request.Object.ArtifactID == tagArtifactId && 
					fields.Count == predicates.Length &&
					checkPredicates;
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenUpdatingTagServiceCallFails()
		{
			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>())).Throws<ServiceException>();

			// act
			Func<Task> action = async () => await _sut.UpdateAsync(0, new DestinationWorkspaceTag()).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagRepositoryException>().WithInnerException<ServiceException>();
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenUpdatingTagFails()
		{
			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.UpdateAsync(0, new DestinationWorkspaceTag()).ConfigureAwait(false);

			// assert
			action.Should().Throw<DestinationWorkspaceTagRepositoryException>().WithInnerException<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldBuildProperQueryForLocalInstance()
		{
			// ARRANGE
			const int localInstanceId = -1;
			const int sourceWorkspaceId = 123;
			const int destinationWorkspaceId = 234;
			
			_federatedInstance.Setup(fi => fi.GetInstanceIdAsync()).ReturnsAsync(localInstanceId);
			QueryResult queryResult = new QueryResult();
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), 
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);

			// ACT
			await _sut.ReadAsync(sourceWorkspaceId, destinationWorkspaceId, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			string properQueryFragment = $"NOT '{_destinationInstanceArtifactIdGuid}' ISSET";
			_objectManager.Verify(
				om => om.QueryAsync(sourceWorkspaceId, It.Is<QueryRequest>(q => q.Condition.Contains(properQueryFragment)), 0, 1, CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()),
				Times.Once);
		}

		[Test]
		public async Task ItShouldBuildProperQueryForFederatedInstance()
		{
			// ARRANGE
			const int federatedInstanceId = 456;
			const int sourceWorkspaceId = 123;
			const int destinationWorkspaceId = 234;
			
			_federatedInstance.Setup(fi => fi.GetInstanceIdAsync()).ReturnsAsync(federatedInstanceId);
			QueryResult queryResult = new QueryResult();
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), 
				CancellationToken.None, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);

			// ACT
			await _sut.ReadAsync(sourceWorkspaceId, destinationWorkspaceId, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			string properQueryFragment = $"'{_destinationInstanceArtifactIdGuid}' == {federatedInstanceId}";
			_objectManager.Verify(
				om => om.QueryAsync(sourceWorkspaceId, It.Is<QueryRequest>(q => q.Condition.Contains(properQueryFragment)), 0, 1, CancellationToken.None, It.IsAny<IProgress<ProgressReport>>()),
				Times.Once);
		}
	}
}