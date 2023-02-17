using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Field = Relativity.Services.Objects.DataContracts.Field;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class SavedSearchQueryRepositoryTests : TestBase
    {
        private IRelativityObjectManager _objectManager;
        private SavedSearchQueryRepository _subjectUnderTest;

        public override void SetUp()
        {
            _objectManager = Substitute.For<IRelativityObjectManager>();
            _subjectUnderTest = new SavedSearchQueryRepository(_objectManager);
        }

        [Test]
        public void RetrieveSavedSearch_ShouldReturnsNullWhenSavedSearchDoesNotExist()
        {
            _objectManager.Query(Arg.Any<QueryRequest>()).Returns(new List<RelativityObject> { null });

            // act
            SavedSearchDTO actualResult = _subjectUnderTest.RetrieveSavedSearch(654);

            // assert
            Assert.IsNull(actualResult);
        }

        [Test]
        public void RetrieveSavedSearch_ShouldReturnsNonPublicSavedSearch()
        {
            int artifactId = 544242;
            int parentArtifactId = 8765;
            string savedSearchName = "All documents";
            string owner = "Administrator";

            var relativityObject = new RelativityObject
            {
                ArtifactID = artifactId,
                ParentObject = new RelativityObjectRef { ArtifactID = parentArtifactId },
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair { Field = new Field {Name = "Name" }, Value = savedSearchName },
                    new FieldValuePair { Field = new Field {Name = "Owner" }, Value = owner}
                }
            };
            _objectManager.Query(Arg.Any<QueryRequest>()).Returns(new List<RelativityObject> { relativityObject });

            // act
            SavedSearchDTO actualResult = _subjectUnderTest.RetrieveSavedSearch(654);

            // assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(artifactId, actualResult.ArtifactId);
            Assert.AreEqual(savedSearchName, actualResult.Name);
            Assert.AreEqual(owner, actualResult.Owner);
            Assert.IsFalse(actualResult.IsPublic);
        }

        [Test]
        public void RetrieveSavedSearch_ShouldReturnsPublicSavedSearch()
        {
            int artifactId = 544242;
            int parentArtifactId = 8765;
            string savedSearchName = "All documents";

            var relativityObject = new RelativityObject
            {
                ArtifactID = artifactId,
                ParentObject = new RelativityObjectRef { ArtifactID = parentArtifactId },
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair { Field = new Field {Name = "Name" }, Value = savedSearchName },
                    new FieldValuePair { Field = new Field {Name = "Owner" }, Value = null}
                }
            };
            _objectManager.Query(Arg.Any<QueryRequest>()).Returns(new List<RelativityObject> { relativityObject });

            // act
            SavedSearchDTO actualResult = _subjectUnderTest.RetrieveSavedSearch(654);

            // assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(artifactId, actualResult.ArtifactId);
            Assert.AreEqual(savedSearchName, actualResult.Name);
            Assert.IsNull(actualResult.Owner);
            Assert.IsTrue(actualResult.IsPublic);
        }

        [Test]
        public void RetrievePublicSavedSearches_ItShouldReturnsOnlyPublicSavedSearches()
        {
            var firstObject = new RelativityObject
            {
                ArtifactID = 1,
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair { Field = new Field {Name = "Name" }, Value = "Search 1" },
                    new FieldValuePair { Field = new Field {Name = "Owner" }, Value = null}
                },
                ParentObject = new RelativityObjectRef { ArtifactID = 0 }
            };
            var secondObject = new RelativityObject
            {
                ArtifactID = 2,
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair { Field = new Field {Name = "Name" }, Value = "Search 2" },
                    new FieldValuePair { Field = new Field {Name = "Owner" }, Value = "Admin"}
                },
                ParentObject = new RelativityObjectRef { ArtifactID = 0 }
            };
            var thirdObject = new RelativityObject
            {
                ArtifactID = 3,
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair { Field = new Field {Name = "Name" }, Value = "Search 3" },
                    new FieldValuePair { Field = new Field {Name = "Owner" }, Value = ""}
                },
                ParentObject = new RelativityObjectRef { ArtifactID = 0 }
            };
            var queryResult = new List<RelativityObject>
            {
                firstObject,
                secondObject,
                thirdObject
            };

            _objectManager.QueryAsync(Arg.Any<QueryRequest>()).Returns(queryResult);

            // act
            List<SavedSearchDTO> actualResults = _subjectUnderTest.RetrievePublicSavedSearches().ToList();

            // assert
            Assert.AreEqual(2, actualResults.Count);
            Assert.IsTrue(actualResults.Any(x => x.ArtifactId == 1));
            Assert.IsTrue(actualResults.Any(x => x.ArtifactId == 3));
            Assert.IsFalse(actualResults.Any(x => x.ArtifactId == 2));
        }

        [Test]
        public void RetrievePublicSavedSearches_ItShouldReturnProperQueryResult()
        {
            var firstObject = new RelativityObject
            {
                ArtifactID = 1,
                ParentObject = new RelativityObjectRef { ArtifactID = 101 },
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair { Field = new Field {Name = "Name" }, Value = "Search 1" },
                    new FieldValuePair { Field = new Field {Name = "Owner" }, Value = null}
                }
            };
            var secondObject = new RelativityObject
            {
                ArtifactID = 2,
                ParentObject = new RelativityObjectRef { ArtifactID = 102 },
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair { Field = new Field {Name = "Name" }, Value = "Search 2" },
                    new FieldValuePair { Field = new Field {Name = "Owner" }, Value = "Admin"}
                }
            };
            var thirdObject = new RelativityObject
            {
                ArtifactID = 3,
                ParentObject = new RelativityObjectRef { ArtifactID = 103 },
                FieldValues = new List<FieldValuePair>
                {
                    new FieldValuePair { Field = new Field {Name = "Name" }, Value = "Search 3" },
                    new FieldValuePair { Field = new Field {Name = "Owner" }, Value = ""}
                }
            };
            var queryResult = new ResultSet<RelativityObject>
            {
                Items = new List<RelativityObject> { firstObject, secondObject, thirdObject },
                TotalCount = 543,
                ResultCount = 3
            };

            _objectManager.Query(Arg.Any<QueryRequest>(), Arg.Any<int>(), Arg.Any<int>()).Returns(queryResult);

            var request = new SavedSearchQueryRequest("", 1, 5);

            // act
            SavedSearchQueryResult actualResults = _subjectUnderTest.RetrievePublicSavedSearches(request);

            // assert
            Assert.AreEqual(3, actualResults.SavedSearches.Count);
            Assert.IsTrue(actualResults.HasMoreResults);
            Assert.AreEqual(543, actualResults.TotalResults);
        }
    }
}
