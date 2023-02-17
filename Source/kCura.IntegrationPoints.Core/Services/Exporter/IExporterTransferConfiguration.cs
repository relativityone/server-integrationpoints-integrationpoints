using System;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public interface IExporterTransferConfiguration
    {
        IScratchTableRepository[] ScratchRepositories { get; }
        IJobHistoryService JobHistoryService { get; }
        Guid Identifier { get;  }
        ImportSettings ImportSettings { get; }
    }

    class ExporterTransferConfiguration:IExporterTransferConfiguration
    {
        public ExporterTransferConfiguration(IScratchTableRepository[] scratchRepositories, IJobHistoryService jobHistoryService, Guid identifier, ImportSettings importSettings)
        {
            ScratchRepositories = scratchRepositories;
            JobHistoryService = jobHistoryService;
            Identifier = identifier;
            ImportSettings = importSettings;
        }

        public IScratchTableRepository[] ScratchRepositories { get; }

        public IJobHistoryService JobHistoryService { get; }

        public Guid Identifier { get; set; }

        public ImportSettings ImportSettings { get; }
    }
}
