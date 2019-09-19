using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	public interface IObjectArtifactIdsByStringFieldValueQuery
	{
		Task<List<int>> QueryForObjectArtifactIdsByStringFieldValueAsync<TSource>(int workspaceId, Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new();
	}
}