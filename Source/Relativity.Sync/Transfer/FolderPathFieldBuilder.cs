﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class FolderPathFieldBuilder : ISpecialFieldBuilder
	{
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
			if (_fieldConfiguration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.ReadFromField)
			{
				string folderPathFieldName = _fieldConfiguration.FolderPathSourceFieldName;
				yield return FieldInfoDto.FolderPathFieldFromDocumentField(folderPathFieldName);
			}
			else if (_fieldConfiguration.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			{
				yield return FieldInfoDto.FolderPathFieldFromSourceWorkspaceStructure();
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