using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	public interface IObjectArtifactIdsByStringFieldValueQuery
	{
		Task<IEnumerable<int>> QueryForObjectArtifactIdsByStringFieldValueAsync<TSource>(int workspaceID, Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new();
	}
}