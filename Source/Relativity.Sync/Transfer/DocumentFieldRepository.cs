using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class DocumentFieldRepository : IDocumentFieldRepository
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;
		private readonly ISourceServiceFactoryForUser _serviceFactory;

		public DocumentFieldRepository(ISourceServiceFactoryForUser serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task<Dictionary<string, RelativityDataType>> GetRelativityDataTypesForFieldsByFieldName(int sourceWorkspaceArtifactId, ICollection<string> fieldNames)
		{
			string concatenatedFieldNames = string.Join(", ", fieldNames.Select(f => $"'{f}'"));
			QueryResultSlim result;
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef {Name = "Field"},
					Condition = $"'Name' IN [{concatenatedFieldNames}] AND 'Object Type Artifact Type ID' == {_DOCUMENT_ARTIFACT_TYPE_ID}",
					Fields = new[] {new FieldRef {Name = "Field Type"}, new FieldRef {Name = "Name"}}
				};
				result = await objectManager.QuerySlimAsync(sourceWorkspaceArtifactId, request, 0, fieldNames.Count).ConfigureAwait(false);
			}

			Dictionary<string, RelativityDataType> parsedResult = result.Objects.ToDictionary(obj => (string) obj.Values[1], obj => ((string) obj.Values[0]).ToRelativityDataType());
			return parsedResult;
		}
	}
}
