using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Transfer
{
	internal sealed class SourceWorkspaceDataReaderConfiguration
	{
		public int SourceWorkspaceId { get; set; }
		public int SourceJobId { get; set; }
		public Guid RunId { get; set; }
		public MetadataMapping MetadataMapping { get; set; }
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }
	}
}
