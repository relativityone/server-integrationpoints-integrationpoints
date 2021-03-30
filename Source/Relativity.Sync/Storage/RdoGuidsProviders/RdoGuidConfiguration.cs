using System;
using System.Linq.Expressions;
using System.Windows.Forms;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class RdoGuidConfiguration : IRdoGuidConfiguration
    {

        public RdoGuidConfiguration(IConfiguration cache)
        {
            JobHistory = new JobHistoryRdoGuidsProvider(cache);
            JobHistoryError = new JobHistoryErrorGuidsProvider(cache);
            DestinationWorkspace = new DestinationWorkspaceTagGuidProvider(cache);
        }

        public IJobHistoryRdoGuidsProvider JobHistory { get; }
        public IJobHistoryErrorGuidsProvider JobHistoryError { get; }
        public IDestinationWorkspaceTagGuidProvider DestinationWorkspace { get; }
    }
}