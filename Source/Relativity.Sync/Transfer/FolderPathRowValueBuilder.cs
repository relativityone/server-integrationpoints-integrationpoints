using System;
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

		public object BuildRowValue(FieldInfoDto fieldInfoDto, RelativityObjectSlim document, object initialValue)
		{
			if (fieldInfoDto.SpecialFieldType == SpecialFieldType.FolderPath)
			{
				if (_destinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
				{
					return _folderPathsMap[document.ArtifactID];
				}
				return initialValue;
			}
			throw new ArgumentException($"Cannot build value for {nameof(SpecialFieldType)}.{fieldInfoDto.SpecialFieldType.ToString()}.", nameof(fieldInfoDto));
		}
	}
}