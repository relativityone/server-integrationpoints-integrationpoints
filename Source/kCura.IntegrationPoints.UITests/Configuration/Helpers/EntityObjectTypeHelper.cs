using kCura.IntegrationPoint.Tests.Core.Exceptions;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.UITests.Logging;
using Relativity.Services.Objects.DataContracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class EntityObjectTypeHelper
	{
		private int? _entityObjectTypeArtifactId;

		private readonly TestContext _testContext;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(EntityObjectTypeHelper));

		private int? EntityObjectTypeArtifactID =>
			_entityObjectTypeArtifactId
			?? (_entityObjectTypeArtifactId = GetEntityTypeArtifactID());

		public EntityObjectTypeHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		public void AddEntityObjectToWorkspace()
		{
			string workspaceName = _testContext.WorkspaceName;
			int? workspaceID = _testContext.WorkspaceId;

			Log.Information("Adding Entity object to '{WorkspaceName}' ({WorkspaceId}).", workspaceName, workspaceID);
			if (!EntityObjectTypeArtifactID.HasValue)
			{
				_testContext.ApplicationInstallationHelper.InstallLegalHold();

				if (!EntityObjectTypeArtifactID.HasValue)
				{
					throw new TestSetupException("Entity object type is missing after installing Legal Hold application");
				}
			}
			else
			{
				Log.Information("Entity object was already present in '{WorkspaceName}' ({WorkspaceId}).", workspaceName, workspaceID);
			}
		}

		public async Task CreateEntityView(string viewName)
		{
			int entityObjectTypeArtifactID = GetEntityObjectTypeArtifactIdOrThrowException();
			int workspaceID = _testContext.GetWorkspaceId();

			try
			{
				Guid[] viewFieldsGuids = { Guid.Parse(EntityFieldGuids.FullName) };
				await IntegrationPoint.Tests.Core.View
					.CreateViewAsync(
						workspaceID,
						viewName,
						entityObjectTypeArtifactID,
						viewFieldsGuids)
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new TestSetupException("Exception occured while creating entity view", ex);
			}
		}

		private int GetEntityObjectTypeArtifactIdOrThrowException()
		{
			if (EntityObjectTypeArtifactID.HasValue)
			{
				return EntityObjectTypeArtifactID.Value;
			}

			const string errorMessage = "Cannot add Entity view, because Entity object type is missing in a workspace";
			Log.Error(errorMessage + " '{WorkspaceName}' ({WorkspaceId}).", _testContext.WorkspaceName, _testContext.WorkspaceId);
			throw new TestSetupException(errorMessage);
		}

		private int? GetEntityTypeArtifactID()
		{
			const string artifactTypeIdFieldName = "Artifact Type ID";
			const string entityObjectName = "Entity";

			var objectTypeRef = new ObjectTypeRef
			{
				ArtifactTypeID = (int)Relativity.Client.ArtifactType.ObjectType
			};

			string condition = $"'Name' == '{entityObjectName}'";
			var artifactTypeIdField = new FieldRef
			{
				Name = artifactTypeIdFieldName
			};
			var queryRequest = new QueryRequest
			{
				ObjectType = objectTypeRef,
				Fields = new[] { artifactTypeIdField },
				Condition = condition
			};

			List<RelativityObject> result = _testContext.ObjectManager.Query(queryRequest);
			RelativityObject entityObjectType = result?.FirstOrDefault();

			return entityObjectType
				?.FieldValues
				?.Single(x => x.Field.Name == artifactTypeIdFieldName)
				.Value as int?;
		}
	}
}
