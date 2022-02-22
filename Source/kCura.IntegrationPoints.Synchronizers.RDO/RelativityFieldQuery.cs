using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RelativityFieldQuery : IRelativityFieldQuery
	{
		private readonly IRelativityObjectManager _relativityObjectManager;
		private readonly IAPILog _logger;

		public RelativityFieldQuery(IRelativityObjectManager relativityObjectManager, IHelper helper)
		{
			_relativityObjectManager = relativityObjectManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityFieldQuery>();
		}

		public virtual List<RelativityObject> GetFieldsForRdo(int rdoTypeId)
		{
			QueryRequest request = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = (int)ArtifactType.Field
				},
				Condition = $"'Object Type Artifact Type ID' == OBJECT {rdoTypeId}",
				IncludeNameInQueryResult = true,
				Fields = new[]
				{
					new FieldRef() {Name = "Name"},
					new FieldRef() {Name = "Choices"},
					new FieldRef() {Name = "Object Type Artifact Type ID"},
					new FieldRef() {Name = "Field Type"},
					new FieldRef() {Name = "Field Type ID"},
					new FieldRef() {Name = "Is Identifier"},
					new FieldRef() {Name = "Field Type Name"},
				},
				Sorts = new[]
				{
					new Sort()
					{
						Direction = SortEnum.Ascending,
						FieldIdentifier = new FieldRef()
						{
							Name = "Name"
						},
						Order = 1
					}
				}
			};

			try
			{
				List<RelativityObject> results = _relativityObjectManager.QueryAsync(request, ExecutionIdentity.System).GetAwaiter().GetResult();
				return results;
			}
			catch (Exception ex)
			{
				LogRetrievingAllFieldsError(ex, rdoTypeId);
				throw;
			}
		}

		#region Logging

		private void LogRetrievingAllFieldsError(Exception ex, int rdoTypeId)
		{
			_logger.LogError("Failed to retrieve all fields for RDO type {RdoTypeId}. Details: {Message}.", rdoTypeId, ex.Message);
		}

		#endregion
	}
}
