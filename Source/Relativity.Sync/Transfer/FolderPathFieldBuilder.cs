using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class FolderPathFieldBuilder : ISpecialFieldBuilder
	{
		private const string _FOLDER_PATH_FIELD_NAME = "76B270CB-7CA9-4121-B9A1-BC0D655E5B2D";

		private readonly IFolderPathRetriever _folderPathRetriever;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IFieldConfiguration _fieldConfiguration;

		public FolderPathFieldBuilder(ISourceServiceFactoryForUser serviceFactory, IFolderPathRetriever folderPathRetriever,
			IFieldConfiguration fieldConfiguration)
		{
			_folderPathRetriever = folderPathRetriever;
			_serviceFactory = serviceFactory;
			_fieldConfiguration = fieldConfiguration;
		}

		public IEnumerable<FieldInfoDto> BuildColumns()
		{
			string folderFieldName;
			bool isDocumentField;
			if (_fieldConfiguration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				// GetAwaiter().GetResult() has to be used until we use field name instead of field id to get folder field
				folderFieldName = GetFolderFieldNameAsync().GetAwaiter().GetResult();
				isDocumentField = false;
			}
			else if (_fieldConfiguration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			{
				folderFieldName = _FOLDER_PATH_FIELD_NAME;
				isDocumentField = true;
			}
			else
			{
				yield break;
			}

			yield return new FieldInfoDto { SpecialFieldType = SpecialFieldType.FolderPath, DisplayName = folderFieldName, IsDocumentField = isDocumentField };
		}

		private async Task<string> GetFolderFieldNameAsync()
		{
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { Name = "Field" },
					Condition = $"'ArtifactId' == {_fieldConfiguration.FolderPathSourceFieldArtifactId}",
					Fields = new[] { new FieldRef { Name = "Name" } }
				};
				QueryResultSlim result = await objectManager.QuerySlimAsync(_fieldConfiguration.SourceWorkspaceArtifactId, request, 0, 1).ConfigureAwait(false);
				return result.Objects[0].Values[0].ToString();
			}
		}

		public async Task<ISpecialFieldRowValuesBuilder> GetRowValuesBuilderAsync(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			IDictionary<int, string> folderPathsMap = await BuildFolderPathsMap(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false);
			return new FolderPathRowValueBuilder(_fieldConfiguration.DestinationFolderStructureBehavior, folderPathsMap);
		}

		private async Task<IDictionary<int, string>> BuildFolderPathsMap(int sourceWorkspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			IDictionary<int, string> folderPathsMap = null;
			if (_fieldConfiguration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			{
				folderPathsMap = await _folderPathRetriever.GetFolderPathsAsync(sourceWorkspaceArtifactId, documentArtifactIds).ConfigureAwait(false);
			}
			return folderPathsMap;
		}
	}
}