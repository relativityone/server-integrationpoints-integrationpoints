using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    public class DestinationWorkspaceObjectTypesCreationExecutorTests
    {
        private ConfigurationStub _configuration;
        private ContainerBuilder _containerBuilder;
        private DestinationWorkspaceObjectTypesCreationExecutor _instance;
        private IContainer _container;
        private Mock<IArtifactGuidManager> _artifactGuidManager;
        private Mock<IDestinationServiceFactoryForAdmin> _serviceFactory;
        private Mock<IObjectManager> _objectManager;
        private Mock<IFieldManager> _fieldManager;
        private Mock<IObjectTypeManager> _objectTypeManager;

        private const int _WORKSPACE_ID = 1;
        private const string _SOURCE_WORKSPACE_OBJECT_TYPE_NAME = "Relativity Source Case";
        private const int _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_ARTIFACT_ID = 987;

        private const string _SOURCE_WORKSPACE_CASEID_FIELD_NAME = "Source Workspace Artifact ID";
        private const int _SOURCE_WORKSPACE_CASEID_FIELD_ARTIFACT_ID = 876;

        private const string _SOURCE_WORKSPACE_CASENAME_FIELD_NAME = "Source Workspace Name";
        private const int _SOURCE_WORKSPACE_CASENAME_FIELD_ARTIFACT_ID = 765;

        private const string _SOURCE_WORKSPACE_INSTANCENAME_FIELD_NAME = "Source Instance Name";
        private const int _SOURCE_WORKSPACE_INSTANCENAME_FIELD_ARTIFACT_ID = 654;

        private const string _SOURCE_WORKSPACE_FIELD_ON_DOCUMENT_NAME = "Relativity Source Case";
        private const int _SOURCE_WORKSPACE_FIELD_ON_DOCUMENT_ARTIFACT_ID = 432;

        private const string _SOURCE_JOB_OBJECT_TYPE_NAME = "Relativity Source Job";
        private const int _SOURCE_JOB_OBJECT_TYPE_ARTIFACT_ID = 543;

        private const string _SOURCE_JOB_JOBHISTORYID_FIELD_NAME = "Job History Artifact ID";
        private const int _SOURCE_JOB_JOBHISTORYID_FIELD_ARTIFACT_ID = 321;

        private const string _SOURCE_JOB_JOBHISTORYNAME_FIELD_NAME = "Job History Name";
        private const int _SOURCE_JOB_JOBHISTORYNAME_FIELD_ARTIFACT_ID = 210;

        private const string _SOURCE_JOB_FIELD_ON_DOCUMENT_NAME = "Relativity Source Job";
        private const int _SOURCE_JOB_FIELD_ON_DOCUMENT_ARTIFACT_ID = 109;

        private static readonly Guid SourceWorkspaceFieldOnDocumentGuid = new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f");
        private static readonly Guid SourceWorkspaceObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
        private static readonly Guid CaseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
        private static readonly Guid CaseNameFieldNameGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
        private static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");

        private static readonly Guid SourceJobObjectTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");
        private static readonly Guid JobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");
        private static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
        private static readonly Guid JobHistoryNameFieldGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");

        [SetUp]
        public void SetUp()
        {
            _configuration = new ConfigurationStub
            {
                DestinationWorkspaceArtifactId = _WORKSPACE_ID
            };

            _containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            IntegrationTestsContainerBuilder.MockReportingWithProgress(_containerBuilder);
            _artifactGuidManager = new Mock<IArtifactGuidManager>();
            _objectManager = new Mock<IObjectManager>();
            _objectTypeManager = new Mock<IObjectTypeManager>();
            _fieldManager = new Mock<IFieldManager>();

            _serviceFactory = new Mock<IDestinationServiceFactoryForAdmin>();
            _serviceFactory.Setup(f => f.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            _serviceFactory.Setup(f => f.CreateProxyAsync<IObjectTypeManager>()).ReturnsAsync(_objectTypeManager.Object);
            _serviceFactory.Setup(f => f.CreateProxyAsync<IArtifactGuidManager>()).ReturnsAsync(_artifactGuidManager.Object);
            _serviceFactory.Setup(f => f.CreateProxyAsync<IFieldManager>()).ReturnsAsync(_fieldManager.Object);
            _containerBuilder.RegisterInstance(_configuration).AsImplementedInterfaces();
            _containerBuilder.RegisterInstance(_serviceFactory.Object).As<IDestinationServiceFactoryForAdmin>();
            _container = _containerBuilder.Build();

            _instance = new DestinationWorkspaceObjectTypesCreationExecutor(_container.Resolve<ISyncObjectTypeManager>(),
                _container.Resolve<ISyncFieldManager>(), new EmptyLogger());
        }

        [Test]
        public async Task ItShouldCreateObjectTypesWithFields()
        {
            // Arrange
            SetupForSourceWorkspaceObjectType(false, false);
            SetupForCaseIdField(false, false);
            SetupForCaseNameField(false, false);
            SetupForInstanceNameField(false, false);
            SetupForSourceCaseDocumentField(false, false);

            SetupForSourceJobObjectType(false, false);
            SetupForJobHistoryIdField(false, false);
            SetupForJobHistoryNameField(false, false);
            SetupForSourceJobDocumentField(false, false);

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(_configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyMocks();
        }

        [Test]
        public async Task ItShouldCreateSourceJobObjectTypeWithFields()
        {
            // Arrange
            SetupForSourceWorkspaceObjectType(true, true);
            SetupForCaseIdField(true, true);
            SetupForCaseNameField(true, true);
            SetupForInstanceNameField(true, true);
            SetupForSourceCaseDocumentField(true, true);

            SetupForSourceJobObjectType(false, false);
            SetupForJobHistoryIdField(false, false);
            SetupForJobHistoryNameField(false, false);
            SetupForSourceJobDocumentField(false, false);

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(_configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyMocks();
        }

        [Test]
        public async Task ItShouldCreateMissingFields()
        {
            // Arrange
            SetupForSourceWorkspaceObjectType(true, true);
            SetupForCaseIdField(false, false);
            SetupForCaseNameField(false, false);
            SetupForInstanceNameField(false, false);
            SetupForSourceCaseDocumentField(true, true);

            SetupForSourceJobObjectType(true, true);
            SetupForJobHistoryIdField(false, false);
            SetupForJobHistoryNameField(false, false);
            SetupForSourceJobDocumentField(true, true);

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(_configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyMocks();
        }

        [Test]
        public async Task ItShouldCreateMissingFieldsOnDocument()
        {
            // Arrange
            SetupForSourceWorkspaceObjectType(true, true);
            SetupForCaseIdField(true, true);
            SetupForCaseNameField(true, true);
            SetupForInstanceNameField(true, true);
            SetupForSourceCaseDocumentField(false, false);

            SetupForSourceJobObjectType(true, true);
            SetupForJobHistoryIdField(true, true);
            SetupForJobHistoryNameField(true, true);
            SetupForSourceJobDocumentField(false, false);

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(_configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyMocks();
        }

        [Test]
        public async Task ItShouldAssociateExistingObjectTypesAndFieldsWithGuids()
        {
            // Arrange
            SetupForSourceWorkspaceObjectType(false, true);
            SetupForCaseIdField(false, true);
            SetupForCaseNameField(false, true);
            SetupForInstanceNameField(false, true);
            SetupForSourceCaseDocumentField(false, true);

            SetupForSourceJobObjectType(false, true);
            SetupForJobHistoryIdField(false, true);
            SetupForJobHistoryNameField(false, true);
            SetupForSourceJobDocumentField(false, true);

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(_configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyMocks();
        }

        [Test]
        public async Task ItShouldResultInFailureOnException()
        {
            // Arrange
            ServiceException exception = new ServiceException();

            SetupForSourceWorkspaceObjectType(false, false);
            SetupForCaseIdField(false, false);
            SetupForCaseNameField(false, false);
            SetupForInstanceNameField(false, false);
            SetupForSourceCaseDocumentField(false, false);

            SetupForSourceJobObjectType(false, false);
            SetupForJobHistoryIdField(false, false);
            SetupForJobHistoryNameField(false, false);

            _artifactGuidManager.Setup(agm => agm.GuidExistsAsync(_WORKSPACE_ID, It.Is((Guid guid) => guid == JobHistoryFieldOnDocumentGuid))).ThrowsAsync(exception);

            // Act
            ExecutionResult result = await _instance.ExecuteAsync(_configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Exception.Should().Be(exception);
            VerifyMocks();
        }

        private void SetupForSourceWorkspaceObjectType(bool guidExists, bool objectTypeExists)
        {
            SetupMocksForObjectType(_RELATIVITY_SOURCE_CASE_OBJECT_TYPE_ARTIFACT_ID, SourceWorkspaceObjectTypeGuid, _SOURCE_WORKSPACE_OBJECT_TYPE_NAME,
                p => p.Name == "Workspace", guidExists, objectTypeExists);
        }

        private void SetupForSourceJobObjectType(bool guidExists, bool objectTypeExists)
        {
            SetupMocksForObjectType(_SOURCE_JOB_OBJECT_TYPE_ARTIFACT_ID, SourceJobObjectTypeGuid, _SOURCE_JOB_OBJECT_TYPE_NAME,
                p => p.ArtifactID == _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_ARTIFACT_ID, guidExists, objectTypeExists);
        }

#pragma warning disable RG2011 // Method Argument Count Analyzer
        private void SetupMocksForObjectType(int objectTypeArtifactId,
            Guid objectTypeGuid,
            string objectTypeName,
            Predicate<ObjectTypeIdentifier> parentObjectTypePredicate,
            bool guidExists,
            bool objectTypeExists)
#pragma warning restore RG2011 // Method Argument Count Analyzer
        {
            string formattedParameters = $"Object type GUID: '{objectTypeGuid}'; Object type name: '{objectTypeName}'; Setup as GUID exists: '{guidExists}'; Setup as object type exists: '{objectTypeExists}'";

            SetupGuidExistenceCheckForArtifact(objectTypeGuid, guidExists, formattedParameters);

            if (guidExists)
            {
                SetupReadingObjectType(objectTypeArtifactId, objectTypeGuid, formattedParameters);
            }
            else
            {
                SetupQueryingObjectTypeByName(objectTypeArtifactId, objectTypeName, objectTypeExists);
                if (!objectTypeExists)
                {
                    SetupObjectTypeCreation(objectTypeArtifactId, objectTypeName, parentObjectTypePredicate);
                }
                SetupGuidAssociationWithArtifact(objectTypeArtifactId, objectTypeGuid, formattedParameters);
            }
        }

        private void SetupGuidExistenceCheckForArtifact(Guid guid, bool guidExists, string formattedParameters)
        {
            _artifactGuidManager.Setup(agm => agm.GuidExistsAsync(_WORKSPACE_ID, It.Is((Guid g) => g == guid))).ReturnsAsync(guidExists)
                .Verifiable($"Service '{nameof(IArtifactGuidManager)}.{nameof(IArtifactGuidManager.GuidExistsAsync)}' did not receive expected call for checking GUID. {formattedParameters}");
        }

        private void SetupReadingObjectType(int objectTypeArtifactId, Guid objectTypeGuid, string formattedParameters)
        {
            _artifactGuidManager
                .Setup(otm => otm.ReadSingleArtifactIdAsync(_WORKSPACE_ID, objectTypeGuid)).ReturnsAsync(objectTypeArtifactId)
                .Verifiable($"Service '{nameof(IObjectTypeManager)}.{nameof(IObjectTypeManager.ReadAsync)}' did not receive expected read call. {formattedParameters}");
        }

        private void SetupQueryingObjectTypeByName(int objectTypeArtifactId, string objectTypeName, bool objectTypeExists)
        {
            QueryResult result = objectTypeExists ? CreateQueryResult(new RelativityObject { ArtifactID = objectTypeArtifactId }) : CreateQueryResult();
            _objectManager.Setup(om => om.QueryAsync(_WORKSPACE_ID,
                    It.Is(BuildVerifyQueryRequestCondition(ArtifactType.ObjectType, $"'Name' == '{objectTypeName}'")), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(result).Verifiable();
        }

        private void SetupObjectTypeCreation(int objectTypeArtifactId, string objectTypeName, Predicate<ObjectTypeIdentifier> parentObjectTypePredicate)
        {
            _objectTypeManager.Setup(otm => otm.CreateAsync(_WORKSPACE_ID, It.Is<ObjectTypeRequest>(r => parentObjectTypePredicate(r.ParentObjectType.Value) && r.Name == objectTypeName)))
                .ReturnsAsync(objectTypeArtifactId).Verifiable();
        }

        private void SetupForCaseIdField(bool guidExists, bool fieldExists)
        {
            SetupMocksForField(_SOURCE_WORKSPACE_CASEID_FIELD_ARTIFACT_ID, CaseIdFieldNameGuid, _SOURCE_WORKSPACE_CASEID_FIELD_NAME, _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_ARTIFACT_ID,
                _SOURCE_WORKSPACE_OBJECT_TYPE_NAME, typeof(WholeNumberFieldRequest), guidExists, fieldExists);
        }

        private void SetupForCaseNameField(bool guidExists, bool fieldExists)
        {
            SetupMocksForField(_SOURCE_WORKSPACE_CASENAME_FIELD_ARTIFACT_ID, CaseNameFieldNameGuid, _SOURCE_WORKSPACE_CASENAME_FIELD_NAME, _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_ARTIFACT_ID,
                _SOURCE_WORKSPACE_OBJECT_TYPE_NAME, typeof(FixedLengthFieldRequest), guidExists, fieldExists);
        }

        private void SetupForInstanceNameField(bool guidExists, bool fieldExists)
        {
            SetupMocksForField(_SOURCE_WORKSPACE_INSTANCENAME_FIELD_ARTIFACT_ID, InstanceNameFieldGuid, _SOURCE_WORKSPACE_INSTANCENAME_FIELD_NAME, _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_ARTIFACT_ID,
                _SOURCE_WORKSPACE_OBJECT_TYPE_NAME, typeof(FixedLengthFieldRequest), guidExists, fieldExists);
        }

        private void SetupForSourceCaseDocumentField(bool guidExists, bool fieldExists)
        {
            SetupMocksForField(_SOURCE_WORKSPACE_FIELD_ON_DOCUMENT_ARTIFACT_ID, SourceWorkspaceFieldOnDocumentGuid, _SOURCE_WORKSPACE_FIELD_ON_DOCUMENT_NAME,
                (int)ArtifactType.Document, "Document", typeof(MultipleObjectFieldRequest), guidExists, fieldExists);
        }

        private void SetupForJobHistoryIdField(bool guidExists, bool objectTypeExists)
        {
            SetupMocksForField(_SOURCE_JOB_JOBHISTORYID_FIELD_ARTIFACT_ID, JobHistoryIdFieldGuid, _SOURCE_JOB_JOBHISTORYID_FIELD_NAME,
                _SOURCE_JOB_OBJECT_TYPE_ARTIFACT_ID, _SOURCE_JOB_OBJECT_TYPE_NAME, typeof(WholeNumberFieldRequest), guidExists, objectTypeExists);
        }

        private void SetupForJobHistoryNameField(bool guidExists, bool fieldExists)
        {
            SetupMocksForField(_SOURCE_JOB_JOBHISTORYNAME_FIELD_ARTIFACT_ID, JobHistoryNameFieldGuid, _SOURCE_JOB_JOBHISTORYNAME_FIELD_NAME,
                _SOURCE_JOB_OBJECT_TYPE_ARTIFACT_ID, _SOURCE_JOB_OBJECT_TYPE_NAME, typeof(FixedLengthFieldRequest), guidExists, fieldExists);
        }

        private void SetupForSourceJobDocumentField(bool guidExists, bool fieldExists)
        {
            SetupMocksForField(_SOURCE_JOB_FIELD_ON_DOCUMENT_ARTIFACT_ID, JobHistoryFieldOnDocumentGuid, _SOURCE_JOB_FIELD_ON_DOCUMENT_NAME,
                (int)ArtifactType.Document, "Document", typeof(MultipleObjectFieldRequest), guidExists, fieldExists);
        }

#pragma warning disable RG2011 // Method Argument Count Analyzer
        private void SetupMocksForField(int fieldArtifactId, Guid fieldGuid, string fieldName, int containingObjectTypeArtifactId, string containingObjectTypeName, Type creationRequestType,
            bool guidExists, bool fieldExists)
#pragma warning restore RG2011 // Method Argument Count Analyzer
        {
            string formattedParameters = $"Field GUID: '{fieldGuid}'; Field name: '{fieldName}'; " +
                                $"Of object type named: '{containingObjectTypeName}'; Object type artifact id: '{containingObjectTypeArtifactId}'; " +
                                $"Setup as GUID exists: '{guidExists}'; Setup as field exists: '{fieldExists}'";

            SetupGuidExistenceCheckForArtifact(fieldGuid, guidExists, formattedParameters);

            if (!guidExists)
            {
                SetupFieldQueryingByObjectManager(fieldArtifactId, fieldName, fieldExists, formattedParameters);

                if (!fieldExists)
                {
                    SetupFieldCreation(fieldArtifactId, fieldName, containingObjectTypeArtifactId, creationRequestType, formattedParameters);
                }

                SetupGuidAssociationWithArtifact(fieldArtifactId, fieldGuid, formattedParameters);
            }
        }

        private void SetupFieldQueryingByObjectManager(int fieldArtifactId, string fieldName, bool fieldExists, string parametersListForMessage)
        {
            QueryResult result = fieldExists ? CreateQueryResult(new RelativityObject { ArtifactID = fieldArtifactId }) : CreateQueryResult();
            _objectManager.Setup(om =>
                    om.QueryAsync(_WORKSPACE_ID, It.Is(BuildVerifyQueryRequestCondition(ArtifactType.Field, $"'Name' == '{fieldName}'")), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(result)
                    .Verifiable($"Service '{nameof(IObjectManager)}.{nameof(IObjectManager.QueryAsync)}' did not receive expected call for querying for field. {parametersListForMessage}");
        }

        private void SetupFieldCreation(int fieldArtifactId, string fieldName, int containingObjectTypeArtifactId, Type creationRequestType, string parametersListForMessage)
        {
            if (creationRequestType == typeof(WholeNumberFieldRequest))
            {
                SetupForWholeNumberField(fieldArtifactId, fieldName, containingObjectTypeArtifactId, parametersListForMessage);
            }
            else if (creationRequestType == typeof(FixedLengthFieldRequest))
            {
                SetupForFixedLengthTextField(fieldArtifactId, fieldName, containingObjectTypeArtifactId, parametersListForMessage);
            }
            else if (creationRequestType == typeof(MultipleObjectFieldRequest))
            {
                SetupForMultipleObjectField(fieldArtifactId, fieldName, containingObjectTypeArtifactId, parametersListForMessage);
            }
        }

        private void SetupGuidAssociationWithArtifact(int artifactId, Guid artifactGuid, string parametersListForMessage)
        {
            _artifactGuidManager
                .Setup(m => m.CreateSingleAsync(_WORKSPACE_ID, artifactId, It.Is<List<Guid>>(g => g.Contains(artifactGuid))))
                .Returns(Task.CompletedTask)
                .Verifiable($"Service '{nameof(IArtifactGuidManager)}.{nameof(IArtifactGuidManager.CreateSingleAsync)}' did not receive expected call to associate field with GUID. {parametersListForMessage}");
        }

        private void SetupForWholeNumberField(int fieldArtifactId, string fieldName, int containingObjectTypeArtifactId, string parametersListForMessage)
        {
            _fieldManager.Setup(fm =>
                    fm.CreateWholeNumberFieldAsync(_WORKSPACE_ID, It.Is<WholeNumberFieldRequest>(r => r.Name == fieldName && r.ObjectType.ArtifactID == containingObjectTypeArtifactId)))
                    .ReturnsAsync(fieldArtifactId)
                    .Verifiable($"Service '{nameof(IFieldManager)}.{nameof(IFieldManager.CreateWholeNumberFieldAsync)}' did not receive expected call for field creation. {parametersListForMessage}");
        }

        private void SetupForFixedLengthTextField(int fieldArtifactId, string fieldName, int containingObjectTypeArtifactId, string parametersListForMessage)
        {
            _fieldManager.Setup(fm =>
                    fm.CreateFixedLengthFieldAsync(_WORKSPACE_ID, It.Is<FixedLengthFieldRequest>(r => r.Name == fieldName && r.ObjectType.ArtifactID == containingObjectTypeArtifactId)))
                    .ReturnsAsync(fieldArtifactId)
                    .Verifiable($"Service '{nameof(IFieldManager)}.{nameof(IFieldManager.CreateWholeNumberFieldAsync)}' did not receive expected call for field creation. {parametersListForMessage}");
        }

        private void SetupForMultipleObjectField(int fieldArtifactId, string fieldName, int containingObjectTypeArtifactId, string parametersListForMessage)
        {
            _fieldManager.Setup(fm =>
                    fm.CreateMultipleObjectFieldAsync(_WORKSPACE_ID, It.Is<MultipleObjectFieldRequest>(r => r.Name == fieldName && r.ObjectType.ArtifactTypeID == containingObjectTypeArtifactId)))
                    .ReturnsAsync(fieldArtifactId)
                    .Verifiable($"Service '{nameof(IFieldManager)}.{nameof(IFieldManager.CreateWholeNumberFieldAsync)}' did not receive expected call for field creation. {parametersListForMessage}");
        }

        public static Expression<Func<QueryRequest, bool>> BuildVerifyQueryRequestCondition(ArtifactType artifactType, string condition)
        {
            return r => r.ObjectType.ArtifactTypeID == (int)artifactType && r.Condition == condition;
        }

        public static QueryResult CreateQueryResult(params RelativityObject[] results)
        {
            return new QueryResult { Objects = new List<RelativityObject>(results) };
        }

        private void VerifyMocks()
        {
            _objectManager.Verify();
            _artifactGuidManager.Verify();
            _fieldManager.Verify();
            _objectTypeManager.Verify();
        }
    }
}
