using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Converters
{
    [TestFixture, Category("Unit")]
    public class RelativityObjectExtensionsTests
    {
        [Test]
        public void ToWorkspaceDTO_ShouldReturnNullForNullInput()
        {
            // arrange
            RelativityObject input = null;

            // act
            WorkspaceDTO result = input.ToWorkspaceDTO();

            // assert
            result.Should().BeNull("because input was null");
        }

        [Test]
        public void ToWorkspaceDTO_ShouldConvertValidObject()
        {
            // arrange
            const int artifactID = 421521;
            const string nameFieldValue = "Field Name";
            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = CreateFieldValuePairsWithGivenFieldInTheMiddle(
                    WorkspaceFieldsConstants.NAME_FIELD,
                    nameFieldValue)
            };

            // act
            WorkspaceDTO result = input.ToWorkspaceDTO();

            // assert
            result.ArtifactId.Should().Be(artifactID);
            result.Name.Should().Be(nameFieldValue);
        }

        [Test]
        public void ToWorkspaceDTO_ShouldThrowArgumentExceptionWhenFieldsValuesAreNull()
        {
            // arrange
            const int artifactID = 421521;
            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = null
            };

            // act
            Action convertAction = () => input.ToWorkspaceDTO();

            // assert
            string expectedErrorMessage = $"{nameof(RelativityObject)} does not represent valid {nameof(WorkspaceDTO)} - missing fields values";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToWorkspaceDTO_ShouldThrowArgumentExceptionWhenNameFieldValueIsMissing()
        {
            // arrange
            const int artifactID = 421521;
            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = CreateFieldValuePairsWithWrongEntries()
            };

            // act
            Action convertAction = () => input.ToWorkspaceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(WorkspaceDTO)} - missing '{WorkspaceFieldsConstants.NAME_FIELD}' value";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToWorkspaceDTO_ShouldThrowArgumentExceptionWhenNameFieldValueHasWrongType()
        {
            // arrange
            const int artifactID = 421521;
            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = CreateFieldValuePairsWithGivenFieldInTheMiddle(
                    WorkspaceFieldsConstants.NAME_FIELD,
                    value: 12412)
            };

            // act
            Action convertAction = () => input.ToWorkspaceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(WorkspaceDTO)} - wrong '{WorkspaceFieldsConstants.NAME_FIELD}' type";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToWorkspaceDTO_ShouldThrowArgumentExceptionWhenNameFieldIsDuplicated()
        {
            // arrange
            const int artifactID = 421521;
            const string name = "relativity case";

            List<FieldValuePair> fieldsValues = CreateFieldValuePairsWithGivenFieldInTheMiddle(
                WorkspaceFieldsConstants.NAME_FIELD,
                value: name);
            fieldsValues.Add(CreateFieldValuePair(WorkspaceFieldsConstants.NAME_FIELD, name));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldsValues
            };

            // act
            Action convertAction = () => input.ToWorkspaceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(WorkspaceDTO)} - duplicated '{WorkspaceFieldsConstants.NAME_FIELD}' field";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToWorkspaceDTO_ShouldConvertNullFieldValue()
        {
            // arrange
            const int artifactID = 421521;
            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = CreateFieldValuePairsWithGivenFieldInTheMiddle(
                    WorkspaceFieldsConstants.NAME_FIELD,
                    value: null)
            };

            // act
            WorkspaceDTO result = input.ToWorkspaceDTO();

            // assert
            result.Name.Should().BeNull();
        }

        [Test]
        public void ToWorkspaceDTOs_ShouldReturnNullForNullInput()
        {
            // arrange
            IEnumerable<RelativityObject> input = null;

            // act
            IEnumerable<WorkspaceDTO> results = input.ToWorkspaceDTOs();

            // assert
            results.Should().BeNull("because input was null");
        }

        [Test]
        public void ToSavedSearchDTO_ShouldReturnNullForNullInput()
        {
            // arrange
            RelativityObject input = null;

            // act
            SavedSearchDTO result = input.ToSavedSearchDTO();

            // assert
            result.Should().BeNull("because input was null");
        }

        [Test]
        public void ToSavedSearchDTO_ShouldConvertValidObject()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;
            const string savedSearchName = "all docs";
            const string owner = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, savedSearchName));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, owner));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = fieldValuePairs
            };

            // act
            SavedSearchDTO result = input.ToSavedSearchDTO();

            // assert
            result.ArtifactId.Should().Be(artifactID);
            result.ParentContainerId.Should().Be(parentArtifactID);
            result.Name.Should().Be(savedSearchName);
            result.Owner.Should().Be(owner);
        }

        [Test]
        public void ToSavedSearchDTO_ShouldConvertNullFieldValues()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, value: null));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, value: null));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = fieldValuePairs
            };

            // act
            SavedSearchDTO result = input.ToSavedSearchDTO();

            // assert
            result.ArtifactId.Should().Be(artifactID);
            result.ParentContainerId.Should().Be(parentArtifactID);
            result.Name.Should().BeNull("because input value was null");
            result.Owner.Should().BeNull("input value was null");
        }

        [Test]
        public void ToSavedSearchDTO_ShouldThrowArgumentExceptionWhenFieldValuesAreNull()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = null
            };

            // act
            Action convertAction = () => input.ToSavedSearchDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(SavedSearchDTO)} - missing fields values";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToSavedSearchDTO_ShouldThrowArgumentExceptionWhenNameFieldIsMissing()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, value: null));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToSavedSearchDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(SavedSearchDTO)} - missing '{SavedSearchFieldsConstants.NAME_FIELD}' value";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToSavedSearchDTO_ShouldThrowArgumentExceptionWhenOwnerFieldIsMissing()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, value: null));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToSavedSearchDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(SavedSearchDTO)} - missing '{SavedSearchFieldsConstants.OWNER_FIELD}' value";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToSavedSearchDTO_ShouldThrowArgumentExceptionWhenNameFieldHasWrongType()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;
            const string owner = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, value: 1234));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, owner));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToSavedSearchDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(SavedSearchDTO)} - wrong '{SavedSearchFieldsConstants.NAME_FIELD}' type";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToSavedSearchDTO_ShouldThrowArgumentExceptionWhenNameFieldIsDuplicated()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;
            const string owner = "rip user";
            const string name = "all docs";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, name));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, name));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, owner));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToSavedSearchDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(SavedSearchDTO)} - duplicated '{SavedSearchFieldsConstants.NAME_FIELD}' field";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToSavedSearchDTO_ShouldThrowArgumentExceptionWhenOwnerFieldHasWrongType()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;
            const string savedSearchName = "all docs";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, savedSearchName));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, value: 1234));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToSavedSearchDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(SavedSearchDTO)} - wrong '{SavedSearchFieldsConstants.OWNER_FIELD}' type";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToSavedSearchDTO_ShouldThrowArgumentExceptionWhenOwnerFieldIsDuplicated()
        {
            // arrange
            const int artifactID = 23123;
            const int parentArtifactID = 4121;
            const string owner = "rip user";
            const string name = "all docs";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, name));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, owner));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, owner));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = new RelativityObjectRef
                {
                    ArtifactID = parentArtifactID
                },
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToSavedSearchDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(SavedSearchDTO)} - duplicated '{SavedSearchFieldsConstants.OWNER_FIELD}' field";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToSavedSearchDTO_ShouldThrowArgumentExceptionWhenParentObjectIsNull()
        {
            // arrange
            const int artifactID = 23123;
            const string savedSearchName = "all docs";
            const string owner = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.NAME_FIELD, savedSearchName));
            fieldValuePairs.Add(CreateFieldValuePair(SavedSearchFieldsConstants.OWNER_FIELD, owner));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                ParentObject = null,
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToSavedSearchDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(SavedSearchDTO)} - missing '{nameof(RelativityObject.ParentObject)}' value";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToSavedSearchDTOs_ShouldReturnNullForNullInput()
        {
            // arrange
            IEnumerable<RelativityObject> input = null;

            // act
            IEnumerable<SavedSearchDTO> results = input.ToSavedSearchDTOs();

            // assert
            results.Should().BeNull("because input was null");
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldReturnNullForNullInput()
        {
            // arrange
            RelativityObject input = null;

            // act
            FederatedInstanceDto result = input.ToFederatedInstanceDTO();

            // assert
            result.Should().BeNull("because input was null");
        }

        [TestCase("http://federated.relativity.com/Relativity")]
        [TestCase("abcd")]
        public void ToFederatedInstanceDTO_ShouldConvertValidObject(string federatedInstanceUrl)
        {
            // arrange
            const int artifactID = 23123;
            const string federatedInstanceName = "relativity one instance";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.NAME_FIELD, federatedInstanceName));
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD, federatedInstanceUrl));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldValuePairs
            };

            // act
            FederatedInstanceDto result = input.ToFederatedInstanceDTO();

            // assert
            result.ArtifactId.Should().Be(artifactID);
            result.Name.Should().Be(federatedInstanceName);
            result.InstanceUrl.Should().Be(federatedInstanceUrl);
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldConvertNullFieldValues()
        {
            // arrange
            const int artifactID = 23123;

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.NAME_FIELD, value: null));
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD, value: null));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldValuePairs
            };

            // act
            FederatedInstanceDto result = input.ToFederatedInstanceDTO();

            // assert
            result.ArtifactId.Should().Be(artifactID);
            result.Name.Should().BeNull("because input value was null");
            result.InstanceUrl.Should().BeNull("because input value was null");
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldThrowArgumentExceptionWhenFieldValuesAreNull()
        {
            // arrange
            const int artifactID = 23123;

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = null
            };

            // act
            Action convertAction = () => input.ToFederatedInstanceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(FederatedInstanceDto)} - missing fields values";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldThrowArgumentExceptionWhenNameFieldHasWrongType()
        {
            // arrange
            const int artifactID = 23123;
            const string federatedInstanceUrl = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.NAME_FIELD, value: 1234));
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD, federatedInstanceUrl));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToFederatedInstanceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(FederatedInstanceDto)} - wrong '{FederatedInstanceFieldsConstants.NAME_FIELD}' type";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldThrowArgumentExceptionWhenNameFieldIsMissing()
        {
            // arrange
            const int artifactID = 23123;
            const string federatedInstanceUrl = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD, federatedInstanceUrl));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToFederatedInstanceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(FederatedInstanceDto)} - missing '{FederatedInstanceFieldsConstants.NAME_FIELD}' value";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldThrowArgumentExceptionWhenNameFieldIsDuplicated()
        {
            // arrange
            const int artifactID = 23123;
            const string federatedInstanceName = "relInstance";
            const string federatedInstanceUrl = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.NAME_FIELD, federatedInstanceName));
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.NAME_FIELD, federatedInstanceName));
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD, federatedInstanceUrl));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToFederatedInstanceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(FederatedInstanceDto)} - duplicated '{FederatedInstanceFieldsConstants.NAME_FIELD}' field";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldThrowArgumentExceptionWhenInstanceUrlFieldHasWrongType()
        {
            // arrange
            const int artifactID = 23123;
            const string federatedInstanceName = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.NAME_FIELD, federatedInstanceName));
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD, value: 932));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToFederatedInstanceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(FederatedInstanceDto)} - wrong '{FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD}' type";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldThrowArgumentExceptionWhenInstanceUrlFieldIsDuplicated()
        {
            // arrange
            const int artifactID = 23123;
            const string federatedInstanceName = "relInstance";
            const string federatedInstanceUrl = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.NAME_FIELD, federatedInstanceName));
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD, federatedInstanceUrl));
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD, federatedInstanceUrl));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToFederatedInstanceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(FederatedInstanceDto)} - duplicated '{FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD}' field";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToFederatedInstanceDTO_ShouldThrowArgumentExceptionWhenInstanceUrlFieldIsMissing()
        {
            // arrange
            const int artifactID = 23123;
            const string federatedInstanceName = "rip user";

            List<FieldValuePair> fieldValuePairs = CreateFieldValuePairsWithWrongEntries();
            fieldValuePairs.Add(CreateFieldValuePair(FederatedInstanceFieldsConstants.NAME_FIELD, federatedInstanceName));

            var input = new RelativityObject
            {
                ArtifactID = artifactID,
                FieldValues = fieldValuePairs
            };

            // act
            Action convertAction = () => input.ToFederatedInstanceDTO();

            // assert
            string expectedErrorMessage =
                $"{nameof(RelativityObject)} does not represent valid {nameof(FederatedInstanceDto)} - missing '{FederatedInstanceFieldsConstants.INSTANCE_URL_FIELD}' value";
            convertAction.ShouldThrow<ArgumentException>().WithMessage(expectedErrorMessage);
        }

        [Test]
        public void ToFederatedInstanceDTOs_ShouldReturnNullForNullInput()
        {
            // arrange
            IEnumerable<RelativityObject> input = null;

            // act
            IEnumerable<FederatedInstanceDto> results = input.ToFederatedInstanceDTOs();

            // assert
            results.Should().BeNull("because input was null");
        }

        private List<FieldValuePair> CreateFieldValuePairsWithGivenFieldInTheMiddle(string fieldName, object value)
        {
            return new List<FieldValuePair>
            {
                CreateFieldValuePair("Wrong field", "wrong value"),
                CreateFieldValuePair(fieldName, value),
                CreateFieldValuePair("one more wrong file", "another wrong value")
            };
        }

        private List<FieldValuePair> CreateFieldValuePairsWithWrongEntries()
        {
            return new List<FieldValuePair>
            {
                CreateFieldValuePair("Wrong field", "wrong value"),
                CreateFieldValuePair("Another wrong field", "another wrong value"),
                CreateFieldValuePair("one more wrong file", "another wrong value")
            };
        }

        private FieldValuePair CreateFieldValuePair(string fieldName, object value)
        {
            return new FieldValuePair
            {
                Field = new Field { Name = fieldName },
                Value = value
            };
        }
    }
}
