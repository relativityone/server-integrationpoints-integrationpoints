using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Attributes;
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

		public async Task<List<int>> QueryForObjectArtifactIdsByStringFieldValueAsync<TSource>(int workspaceId, Expression<Func<TSource, string>> propertySelector, string fieldValue)
			where TSource : BaseRdo, new()
		{
			PropertyInfo propertyInfo = GetPropertyInfo(propertySelector);
			Guid fieldGuid = propertyInfo
				.GetCustomAttributes(typeof(DynamicFieldAttribute), true)
				.Cast<DynamicFieldAttribute>()
				.Single()
				.FieldGuid;

			Condition searchCondition = new TextCondition(fieldGuid, TextConditionEnum.EqualTo, fieldValue);

			var queryRequest = new QueryRequest
			{
				Condition = searchCondition.ToQueryString()
			};
			List<TSource> relativityObjects = await _createRelativityObjectManager(workspaceId)
				.QueryAsync<TSource>(queryRequest, true).ConfigureAwait(false);
			List<int> objectsArtifactIds = relativityObjects.Select(o => o.ArtifactId).ToList();
			return objectsArtifactIds;
		}

		private static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
		{
			Type type = typeof(TSource);

			if (!(propertyLambda.Body is MemberExpression member))
			{
				throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
			}

			PropertyInfo propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
			{
				throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
			}

			if (propInfo.ReflectedType != null && type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
			{
				throw new ArgumentException($"Expression '{propertyLambda}' refers to a property that is not from type {type}.");
			}

			return propInfo;
		}
	}
}