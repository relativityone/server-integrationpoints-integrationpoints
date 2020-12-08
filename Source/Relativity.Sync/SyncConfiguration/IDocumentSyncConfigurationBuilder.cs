using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	public interface IDocumentSyncConfigurationBuilder : ISyncConfigurationRootBuilder<IDocumentSyncConfigurationBuilder>
	{
		IDocumentSyncConfigurationBuilder DestinationFolderStructure(DestinationFolderStructureOptions options);
	}
}
