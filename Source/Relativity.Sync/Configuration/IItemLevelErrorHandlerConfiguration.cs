using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
    internal interface IItemLevelErrorHandlerConfiguration : IConfiguration
    {
        public int SourceWorkspaceArtifactId { get; }

        public int JobHistoryArtifactId { get; }
    }
}
