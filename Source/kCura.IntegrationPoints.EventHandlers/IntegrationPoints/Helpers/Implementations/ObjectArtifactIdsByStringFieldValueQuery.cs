using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class ObjectArtifactIdsByStringFieldValueQuery : IObjectArtifactIdsByStringFieldValueQuery
	{
		private readonly Func<int, IRelativityObjectManager> _createRelativityObjectManager;

		public ObjectArtifactIdsByStringFieldValueQuery(Func<int, IRelativityObjectManager> createRelativityObjectManager)
		{
			_createRelativityObjectManager = createRelativityObjectManager;
		}

		public async Task<IEnumerable<int>> QueryForObjectArtifactIdsByStringFieldValueAsync<TSource>(int workspaceID,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			Guid fieldGuid = BaseRdo.GetFieldGuid(propertySelector);
			Condition searchCondition = new TextCondition(fieldGuid, TextConditionEnum.EqualTo, fieldValue);

			var queryRequest = new QueryRequest
			{
				Condition = searchCondition.ToQueryString()
			};
			List<TSource> relativityObjects = await _createRelativityObjectManager(workspaceID)
				.QueryAsync<TSource>(queryRequest, noFields: true)
				.ConfigureAwait(false);

			IEnumerable<int> objectsArtifactIDs = relativityObjects
				.Select(relativityObject => relativityObject.ArtifactId);
			return objectsArtifactIDs;
		}
	}
}
