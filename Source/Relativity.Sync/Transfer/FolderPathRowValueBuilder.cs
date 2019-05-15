using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	internal sealed class FolderPathRowValueBuilder : ISpecialFieldRowValuesBuilder
	{
		private readonly DestinationFolderStructureBehavior _destinationFolderStructureBehavior;
		private readonly IDictionary<int, string> _folderPathsMap;

		public FolderPathRowValueBuilder(DestinationFolderStructureBehavior destinationFolderStructureBehavior, IDictionary<int, string> folderPathsMap)
		{
			_destinationFolderStructureBehavior = destinationFolderStructureBehavior;
			_folderPathsMap = folderPathsMap;
		}

		public IEnumerable<SpecialFieldType> AllowedSpecialFieldTypes => new[] {SpecialFieldType.FolderPath};

		public IEnumerable<object> BuildRowValues(FieldInfo fieldInfo, RelativityObjectSlim document, object initialValue)
		{
			if (fieldInfo.SpecialFieldType == SpecialFieldType.FolderPath)
			{
				if (_destinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
				{
					yield return _folderPathsMap[document.ArtifactID];
				}
				else
				{
					yield return initialValue;
				}
			}
		}
	}
}