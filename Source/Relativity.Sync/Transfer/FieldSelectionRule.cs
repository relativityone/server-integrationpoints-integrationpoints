using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	/// <summary>
	/// Incomplete, but ideally this should be something that takes in metadata and returns a list consisting of
	/// field maps + derived fields (e.g. special file fields). This might rely on or replace <see cref="MetadataMapping"/>
	/// </summary>
	internal sealed class FieldSelectionRule
	{
		public static async Task<IDictionary<string, object>> BuildFieldMapAsync(MetadataMapping metadata,
			IFolderPathRetriever folderPathRetriever,
			RelativityObjectSlim artifact)
		{
			Dictionary<string, object> map = new Dictionary<string, object>();

			IEnumerable<FieldRef> fieldRefs = metadata.GetFieldRefs();
			List<object> fieldValues = artifact.Values;

			map.Extend(fieldRefs.Select(x => x.Name), fieldValues);

			string folderPath;
			if (metadata.DestinationFolderStructureBehavior == DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)
			{
				folderPath = await folderPathRetriever.GetFolderPathAsync(artifact.ArtifactID).ConfigureAwait(false);
			}
			
			//map.Add

			return map;
		}
	}
}
