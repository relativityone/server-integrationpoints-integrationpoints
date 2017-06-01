using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    public class ChoiceServiceTests : TestBase
    {
        private List<Choice> _populatedChoicesList;
        private List<Choice> _emptyChoicesList;
        private Guid _guid;
        private Artifact _artifactWithoutFieldNamedName;
        private Artifact _artifactWithFieldNamedName;
        private List<Artifact> _artifactListWith3ProperElements;
        private const int _PROPER_ARTIFACT_ID = 1;
        private const string _PROPER_ARTIFACT_FIELD_VALUE = "proper value";
        private const int _RDO_TYPE_ID = 462398;
        private const string _PROPER_ARTIFACT_TYPE_NAME = "Field";

        public override void FixtureSetUp()
        {
            base.FixtureSetUp();
            _guid = new Guid();
        }

        public override void SetUp()
        {
            _populatedChoicesList = new List<Choice>
            {
                new Choice(),
                new Choice(),
            };
            _emptyChoicesList = new List<Choice>();
            _artifactWithFieldNamedName = new Artifact
            {
                ArtifactID = _PROPER_ARTIFACT_ID,
                Fields = new List<Field>
                {
                    new Field
                    {
                        Name = Constants.Fields.Name,
                        Value = _PROPER_ARTIFACT_FIELD_VALUE
                    },
                    new Field {Name = "differentName"}
                }
            };
            _artifactWithoutFieldNamedName = new Artifact
            {
                ArtifactID = 3,
                Fields = new List<Field>
                {
                    new Field {Name = "differentName"}
                }
            };
            _artifactListWith3ProperElements = new List<Artifact>
            {
                _artifactWithFieldNamedName,
                _artifactWithFieldNamedName,
                _artifactWithFieldNamedName,
                _artifactWithoutFieldNamedName,
                _artifactWithoutFieldNamedName,
            };
        }

        [Test]
        public void GetChoicesOnFieldInt_QueryReturnsList_ReturnsList()
        {
            var query = Substitute.For<IChoiceQuery>();
            query.GetChoicesOnField(1).ReturnsForAnyArgs(_populatedChoicesList);
            var service = new ChoiceService(query);

            List<Choice> result = service.GetChoicesOnField(1);

            Assert.AreEqual(result.Count, _populatedChoicesList.Count);
        }

        [Test]
        public void GetChoicesOnFieldInt_QueryReturnsEmptyList_ReturnsEmptyList()
        {
            var query = Substitute.For<IChoiceQuery>();
            query.GetChoicesOnField(1).ReturnsForAnyArgs(_emptyChoicesList);
            var service = new ChoiceService(query);

            List<Choice> result = service.GetChoicesOnField(1);

            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public void GetChoicesOnFieldInt_QueryReturnsNull_ReturnsNull()
        {
            var query = Substitute.For<IChoiceQuery>();
            query.GetChoicesOnField(1).ReturnsForAnyArgs(_ => null);
            var service = new ChoiceService(query);

            List<Choice> result = service.GetChoicesOnField(1);

            Assert.IsNull(result);
        }

        [Test]
        public void GetChoicesOnFieldGuid_QueryReturnsList_ReturnsList()
        {
            var query = Substitute.For<IChoiceQuery>();
            query.GetChoicesOnField(_guid).ReturnsForAnyArgs(_populatedChoicesList);
            var service = new ChoiceService(query);

            List<Choice> result = service.GetChoicesOnField(_guid);

            Assert.AreEqual(result.Count, _populatedChoicesList.Count);
        }

        [Test]
        public void GetChoicesOnFieldGuid_QueryReturnsEmptyList_ReturnsEmptyList()
        {
            var query = Substitute.For<IChoiceQuery>();
            query.GetChoicesOnField(_guid).ReturnsForAnyArgs(_emptyChoicesList);
            var service = new ChoiceService(query);

            List<Choice> result = service.GetChoicesOnField(_guid);

            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public void GetChoicesOnFieldGuid_QueryReturnsNull_ReturnsNull()
        {
            var query = Substitute.For<IChoiceQuery>();
            query.GetChoicesOnField(_guid).ReturnsForAnyArgs(_ => null);
            var service = new ChoiceService(query);

            List<Choice> result = service.GetChoicesOnField(_guid);

            Assert.IsNull(result);
        }

        [Test]
        public void ConvertToFieldEntries_ListWithArtifactWithProperFieldName_ReturnsProperFieldEntryList()
        {
            var artifacts = new List<Artifact> {_artifactWithFieldNamedName};
            var query = Substitute.For<IChoiceQuery>();
            var service = new ChoiceService(query);

            List<FieldEntry> result = service.ConvertToFieldEntries(artifacts);

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].IsRequired, false);
            Assert.AreEqual(result[0].DisplayName, _PROPER_ARTIFACT_FIELD_VALUE);
            Assert.AreEqual(result[0].FieldIdentifier, _PROPER_ARTIFACT_ID.ToString());
        }

        [Test]
        public void ConvertToFieldEntries_ListWithArtifactWithoutProperFieldName_ReturnsEmptyList()
        {
            var artifacts = new List<Artifact> { _artifactWithoutFieldNamedName };
            var query = Substitute.For<IChoiceQuery>();
            var service = new ChoiceService(query);

            List<FieldEntry> result = service.ConvertToFieldEntries(artifacts);

            Assert.AreEqual(result.Count, 0);
        }
        
        [Test]
        public void ConvertToFieldEntries_EmptyList_ReturnsEmptyList()
        {
            var artifacts = new List<Artifact>();
            var query = Substitute.For<IChoiceQuery>();
            var service = new ChoiceService(query);

            List<FieldEntry> result = service.ConvertToFieldEntries(artifacts);

            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public void ConvertToFieldEntries_ListWith3ProperElems_ReturnsListWithCount3()
        {
            var query = Substitute.For<IChoiceQuery>();
            var service = new ChoiceService(query);

            List<FieldEntry> result = service.ConvertToFieldEntries(_artifactListWith3ProperElements);

            Assert.AreEqual(result.Count, 3);
        }

        [Test]
        public void GetChoiceFields_QueryReturnsListWithProperArtifact_ReturnsList()
        {
            var artifacts = new List<Artifact> { _artifactWithFieldNamedName };
            var choiceQuery = Substitute.For<IChoiceQuery>();
            choiceQuery.GetChoicesByQuery(Arg.Any<Query>()).Returns(artifacts);
            var service = new ChoiceService(choiceQuery);

            List<FieldEntry> result = service.GetChoiceFields(_RDO_TYPE_ID);

            choiceQuery.Received().GetChoicesByQuery(Arg.Is<Query>(arg => arg.ArtifactTypeName == _PROPER_ARTIFACT_TYPE_NAME));
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].IsRequired, false);
            Assert.AreEqual(result[0].DisplayName, _PROPER_ARTIFACT_FIELD_VALUE);
            Assert.AreEqual(result[0].FieldIdentifier, _PROPER_ARTIFACT_ID.ToString());
        }

        [Test]
        public void GetChoiceFields_QueryReturnsListWithoutProperArtifact_ReturnsEmptyList()
        {
            var artifacts = new List<Artifact> { _artifactWithoutFieldNamedName };
            var choiceQuery = Substitute.For<IChoiceQuery>();
            choiceQuery.GetChoicesByQuery(Arg.Any<Query>()).Returns(artifacts);
            var service = new ChoiceService(choiceQuery);

            List<FieldEntry> result = service.GetChoiceFields(_RDO_TYPE_ID);

            choiceQuery.Received().GetChoicesByQuery(Arg.Is<Query>(arg => arg.ArtifactTypeName == _PROPER_ARTIFACT_TYPE_NAME));
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public void GetChoiceFields_QueryReturnsEmptyList_ReturnsEmptyList()
        {
            var artifacts = new List<Artifact>();
            var choiceQuery = Substitute.For<IChoiceQuery>();
            choiceQuery.GetChoicesByQuery(Arg.Any<Query>()).Returns(artifacts);
            var service = new ChoiceService(choiceQuery);

            List<FieldEntry> result = service.GetChoiceFields(_RDO_TYPE_ID);

            choiceQuery.Received().GetChoicesByQuery(Arg.Is<Query>(arg => arg.ArtifactTypeName == _PROPER_ARTIFACT_TYPE_NAME));
            Assert.AreEqual(result.Count, 0);
        }

        [Test]
        public void GetChoiceFields_QueryReturnsListWith3ProperElements_ReturnsListWithCount3()
        {
            var choiceQuery = Substitute.For<IChoiceQuery>();
            choiceQuery.GetChoicesByQuery(Arg.Any<Query>()).Returns(_artifactListWith3ProperElements);
            var service = new ChoiceService(choiceQuery);

            List<FieldEntry> result = service.GetChoiceFields(_RDO_TYPE_ID);

            choiceQuery.Received().GetChoicesByQuery(Arg.Is<Query>(arg => arg.ArtifactTypeName == _PROPER_ARTIFACT_TYPE_NAME));
            Assert.AreEqual(result.Count, 3);
        }
    }
}
