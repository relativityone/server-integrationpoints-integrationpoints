using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
    public sealed class RelativitySourceCaseTagRepositoryTests
    {
        private Mock<IFederatedInstance> _federatedInstance;
        private Mock<ITagNameFormatter> _tagNameFormatter;
        private Mock<IObjectManager> _objectManager;

        private RelativitySourceCaseTagRepository _sut;

        [SetUp]
        public void SetUp()
        {
            var serviceFactory = new Mock<IDestinationServiceFactoryForUser>();
            _federatedInstance = new Mock<IFederatedInstance>();
            _objectManager = new Mock<IObjectManager>();
            _tagNameFormatter = new Mock<ITagNameFormatter>();
            _tagNameFormatter.Setup(x => x.FormatWorkspaceDestinationTagName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("foo bar");
            serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

            _sut = new RelativitySourceCaseTagRepository(serviceFactory.Object, new EmptyLogger());
        }

        [Test]
        public async Task ItShouldReadExistingDestinationWorkspaceTag()
        {
            Guid caseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
            Guid instanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
            Guid sourceWorkspaceNameGuid = new Guid("A16F7BEB-B3B0-4658-BB52-1C801BA920F0");

            const string tagName = "name";
            const string sourceInstanceName = "instance";
            const string sourceWorkspaceName = "workspace";
            const int sourceWorkspaceArtifactId = 2;

            QueryResult queryResult = new QueryResult();
            RelativityObject relativityObject = new RelativityObject
            {
                Name = tagName,
                FieldValues = new List<FieldValuePair>()
                {
                    new FieldValuePair()
                    {
                        Field = new Field() { Guids = new List<Guid>() { instanceNameFieldGuid } },
                        Value = sourceInstanceName
                    },
                    new FieldValuePair()
                    {
                        Field = new Field() { Guids = new List<Guid>() { sourceWorkspaceNameGuid } },
                        Value = sourceWorkspaceName
                    },
                    new FieldValuePair()
                    {
                        Field = new Field() { Guids = new List<Guid>() { caseIdFieldNameGuid } },
                        Value = sourceWorkspaceArtifactId
                    }
                }
            };

            queryResult.Objects.Add(relativityObject);
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);

            // act
            RelativitySourceCaseTag tag = await _sut.ReadAsync(0, 0, string.Empty, CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.AreEqual(relativityObject.ArtifactID, tag.ArtifactId);
            Assert.AreEqual(tagName, tag.Name);
            Assert.AreEqual(sourceInstanceName, tag.SourceInstanceName);
            Assert.AreEqual(sourceWorkspaceName, tag.SourceWorkspaceName);
            Assert.AreEqual(sourceWorkspaceArtifactId, tag.SourceWorkspaceArtifactId);
        }

        [Test]
        public async Task ItShouldReturnNullWhenReadingNotExistingTag()
        {
            QueryResult queryResult = new QueryResult();
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);

            // act
            RelativitySourceCaseTag tag = await _sut.ReadAsync(0, 0, string.Empty, CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.IsNull(tag);
        }

        [Test]
        public void ItShouldThrowRepositoryExceptionWhenReadServiceCallFails()
        {
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).Throws<ServiceException>();

            // act
            Func<Task> action = async () => await _sut.ReadAsync(0, 0, string.Empty, CancellationToken.None).ConfigureAwait(false);

            // assert
            action.Should().Throw<RelativitySourceCaseTagRepositoryException>().WithInnerException<ServiceException>();
        }

        [Test]
        public void ItShouldThrowRepositoryExceptionWhenReadFails()
        {
            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).Throws<InvalidOperationException>();

            // act
            Func<Task> action = async () => await _sut.ReadAsync(0, 0, string.Empty, CancellationToken.None).ConfigureAwait(false);

            // assert
            action.Should().Throw<RelativitySourceCaseTagRepositoryException>().WithInnerException<InvalidOperationException>();
        }


        [Test]
        public async Task ItShouldCreateDestinationWorkspaceTag()
        {
            const int tagArtifactId = 1;
            const int sourceWorkspaceArtifactId = 2;
            const int destinationWorkspaceArtifactId = 3;
            const string sourceWorkspaceName = "workspace";
            const string sourceInstanceName = "instance";
            const string sourceTagName = "Source tag name";

            CreateResult createResult = new CreateResult()
            {
                Object = new RelativityObject()
                {
                    ArtifactID = tagArtifactId,
                }
            };
            _objectManager.Setup(x => x.CreateAsync(destinationWorkspaceArtifactId, It.IsAny<CreateRequest>())).ReturnsAsync(createResult);
            _federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(sourceInstanceName);
            var tagToCreate = new RelativitySourceCaseTag
            {
                Name = sourceTagName,
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                SourceWorkspaceName = sourceWorkspaceName,
                SourceInstanceName = sourceInstanceName
            };

            // act
            RelativitySourceCaseTag createdTag = await _sut.CreateAsync(destinationWorkspaceArtifactId, tagToCreate).ConfigureAwait(false);

            // assert
            Assert.AreEqual(tagArtifactId, createdTag.ArtifactId);
            Assert.AreEqual(sourceTagName, createdTag.Name);
            Assert.AreEqual(sourceWorkspaceName, createdTag.SourceWorkspaceName);
            Assert.AreEqual(sourceInstanceName, createdTag.SourceInstanceName);
            Assert.AreEqual(sourceWorkspaceArtifactId, createdTag.SourceWorkspaceArtifactId);
        }

        [Test]
        public void ItShouldThrowRepositoryExceptionWhenCreatingTagServiceCallFails()
        {
            _objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<ServiceException>();

            // act
            Func<Task> action = async () => await _sut.CreateAsync(0, new RelativitySourceCaseTag()).ConfigureAwait(false);

            // assert
            action.Should().Throw<RelativitySourceCaseTagRepositoryException>().WithInnerException<ServiceException>();
        }

        [Test]
        public void ItShouldThrowRepositoryExceptionWhenCreatingTagFails()
        {
            _objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<InvalidOperationException>();

            // act
            Func<Task> action = async () => await _sut.CreateAsync(0, new RelativitySourceCaseTag()).ConfigureAwait(false);

            // assert
            action.Should().Throw<RelativitySourceCaseTagRepositoryException>().WithInnerException<InvalidOperationException>();
        }

        [Test]
        public async Task ItShouldUpdateTag()
        {
            Guid caseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
            Guid instanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
            Guid sourceWorkspaceNameGuid = new Guid("A16F7BEB-B3B0-4658-BB52-1C801BA920F0");

            const int destinationWorkspaceArtifactId = 1;

            const int tagArtifactId = 2;
            const int sourceInstanceArtifactId = 3;
            const string sourceInstanceName = "instance";
            const int sourceWorkspaceArtifactId = 4;
            const string sourceWorkspaceName = "workspace";

            string sourceTagName = $"{sourceInstanceName} - {sourceWorkspaceName} - {sourceWorkspaceArtifactId}";
            _tagNameFormatter.Setup(x => x.FormatWorkspaceDestinationTagName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(sourceTagName);

            RelativitySourceCaseTag tag = new RelativitySourceCaseTag()
            {
                ArtifactId = tagArtifactId,
                Name = sourceTagName,
                SourceInstanceName = sourceInstanceName,
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                SourceWorkspaceName = sourceWorkspaceName
            };

            _federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(sourceInstanceName);
            _federatedInstance.Setup(x => x.GetInstanceIdAsync()).ReturnsAsync(sourceInstanceArtifactId);

            // act
            await _sut.UpdateAsync(destinationWorkspaceArtifactId, tag).ConfigureAwait(false);

            // assert

            _objectManager.Verify(x => x.UpdateAsync(destinationWorkspaceArtifactId, It.Is<UpdateRequest>(request =>
                VerifyUpdateRequest(request, tagArtifactId,
                    f => "Name".Equals(f.Field.Name, StringComparison.InvariantCulture) && string.Equals(f.Value, sourceTagName),
                    f => caseIdFieldNameGuid.Equals(f.Field.Guid) && string.Equals(f.Value, sourceWorkspaceArtifactId),
                    f => instanceNameFieldGuid.Equals(f.Field.Guid) && Equals(f.Value, sourceInstanceName),
                    f => sourceWorkspaceNameGuid.Equals(f.Field.Guid) && string.Equals(f.Value, sourceWorkspaceName)
                    ))));
        }

        private bool VerifyUpdateRequest(UpdateRequest request, int tagArtifactId, params System.Predicate<FieldRefValuePair>[] predicates)
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
            Func<Task> action = async () => await _sut.UpdateAsync(0, new RelativitySourceCaseTag()).ConfigureAwait(false);

            // assert
            action.Should().Throw<RelativitySourceCaseTagRepositoryException>().WithInnerException<ServiceException>();
        }

        [Test]
        public void ItShouldThrowRepositoryExceptionWhenUpdatingTagFails()
        {
            _objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>())).Throws<InvalidOperationException>();

            // act
            Func<Task> action = async () => await _sut.UpdateAsync(0, new RelativitySourceCaseTag()).ConfigureAwait(false);

            // assert
            action.Should().Throw<RelativitySourceCaseTagRepositoryException>().WithInnerException<InvalidOperationException>();
        }
    }
}
