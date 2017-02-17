﻿using System;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public interface IExporterTransferConfiguration
	{
		IScratchTableRepository[] ScratchRepositories { get; }
		IJobHistoryService JobHistoryService { get; }
		Guid Identifier { get;  }
	}
	

	class ExporterTransferConfiguration:IExporterTransferConfiguration
	{
		public ExporterTransferConfiguration(IScratchTableRepository[] scratchRepositories, IJobHistoryService jobHistoryService, Guid identifier)
		{
			ScratchRepositories = scratchRepositories;
			JobHistoryService = jobHistoryService;
			Identifier = identifier;
		}

		public IScratchTableRepository[] ScratchRepositories { get; }
		public IJobHistoryService JobHistoryService { get; }
		public Guid Identifier { get; set; }
	}
}