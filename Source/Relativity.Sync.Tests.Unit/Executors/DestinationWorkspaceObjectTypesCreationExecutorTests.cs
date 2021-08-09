using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class DestinationWorkspaceObjectTypesCreationExecutorTests
	{
		private Mock<ISyncFieldManager> _fieldManager;
		private Mock<ISyncObjectTypeManager> _objectTypeManager;
		private DestinationWorkspaceObjectTypesCreationExecutor _instance;

		private const string _WORKSPACE_OBJECT_TYPE_NAME = "Workspace";
		private const string _SOURCE_WORKSPACE_OBJECT_TYPE_NAME = "Relativity Source Case";
		private const string _SOURCE_JOB_OBJECT_TYPE_NAME = "Relativity Source Job";

		private static readonly Guid SourceWorkspaceObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid SourceJobObjectTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

		private static readonly Guid CaseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
		private static readonly Guid CaseNameFieldNameGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
		private static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
		private static readonly Guid SourceWorkspaceFieldOnDocumentGuid = new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f");

		private static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
		private static readonly Guid JobHistoryNameFieldGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
		private static readonly Guid JobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");

		[SetUp]
		public void SetUp()
		{
			_objectTypeManager = new Mock<ISyncObjectTypeManager>();
			QueryResult queryResultForWorkspaceObjectType = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject()
					{
						ArtifactID = 1
					}
				}
			};
			_objectTypeManager.Setup(x => x.QueryObjectTypeByNameAsync(It.IsAny<int>(), It.Is<string>(s => s == _WORKSPACE_OBJECT_TYPE_NAME)))
				.ReturnsAsync(queryResultForWorkspaceObjectType);
			_fieldManager =new Mock<ISyncFieldManager>();
			_instance = new DestinationWorkspaceObjectTypesCreationExecutor(_objectTypeManager.Object, _fieldManager.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldCreateObjectTypes()
		{
			const int sourceCaseObjectTypeArtifactId = 2;
			_objectTypeManager.Setup(x => x.EnsureObjectTypeExistsAsync(It.IsAny<int>(), SourceWorkspaceObjectTypeGuid, It.Is<ObjectTypeRequest>(request =>
				request.Name == _SOURCE_WORKSPACE_OBJECT_TYPE_NAME))).ReturnsAsync(sourceCaseObjectTypeArtifactId);
			
			// act
			ExecutionResult result = await _instance.ExecuteAsync(Mock.Of<IDestinationWorkspaceObjectTypesCreationConfiguration>(), CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Completed);
			_objectTypeManager.Verify(x => x.EnsureObjectTypeExistsAsync(It.IsAny<int>(), SourceWorkspaceObjectTypeGuid, It.Is<ObjectTypeRequest>(request =>
				request.Name == _SOURCE_WORKSPACE_OBJECT_TYPE_NAME)));
			_objectTypeManager.Verify(x => x.EnsureObjectTypeExistsAsync(It.IsAny<int>(), SourceJobObjectTypeGuid, It.Is<ObjectTypeRequest>(request =>
				request.Name == _SOURCE_JOB_OBJECT_TYPE_NAME && request.ParentObjectType.Value.ArtifactID == sourceCaseObjectTypeArtifactId)));
		}

		[Test]
		public async Task ItShouldCreateFields()
		{           
			// act
			ExecutionResult result = await _instance.ExecuteAsync(Mock.Of<IDestinationWorkspaceObjectTypesCreationConfiguration>(), CompositeCancellationToken.None)
				.ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Completed);
			_fieldManager.Verify(x => x.EnsureFieldsExistAsync(It.IsAny<int>(), It.Is<IDictionary<Guid, BaseFieldRequest>>(fieldsToCreate => VerifySourceCaseObjectTypeFields(fieldsToCreate))));
			_fieldManager.Verify(x => x.EnsureFieldsExistAsync(It.IsAny<int>(), It.Is<IDictionary<Guid, BaseFieldRequest>>(fieldsToCreate => VerifySourceCaseDocumentFields(fieldsToCreate))));
			_fieldManager.Verify(x => x.EnsureFieldsExistAsync(It.IsAny<int>(), It.Is<IDictionary<Guid, BaseFieldRequest>>(fieldsToCreate => VerifySourceJobObjectTypeFields(fieldsToCreate))));
			_fieldManager.Verify(x => x.EnsureFieldsExistAsync(It.IsAny<int>(), It.Is<IDictionary<Guid, BaseFieldRequest>>(fieldsToCreate => VerifySourceJobDocumentFields(fieldsToCreate))));
		}

		private bool VerifySourceCaseObjectTypeFields(IDictionary<Guid, BaseFieldRequest> fieldsToCreate)
		{
			return fieldsToCreate.ContainsKey(CaseIdFieldNameGuid) &&
				fieldsToCreate.ContainsKey(CaseNameFieldNameGuid) &&
				fieldsToCreate.ContainsKey(InstanceNameFieldGuid);
		}

		private bool VerifySourceCaseDocumentFields(IDictionary<Guid, BaseFieldRequest> fieldsToCreate)
		{
			return fieldsToCreate.ContainsKey(SourceWorkspaceFieldOnDocumentGuid);
		}

		private bool VerifySourceJobObjectTypeFields(IDictionary<Guid, BaseFieldRequest> fieldsToCreate)
		{
			return fieldsToCreate.ContainsKey(JobHistoryIdFieldGuid) &&
				fieldsToCreate.ContainsKey(JobHistoryNameFieldGuid);
		}

		private bool VerifySourceJobDocumentFields(IDictionary<Guid, BaseFieldRequest> fieldsToCreate)
		{
			return fieldsToCreate.ContainsKey(JobHistoryFieldOnDocumentGuid);
		}
	}
}