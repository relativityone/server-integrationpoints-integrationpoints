﻿using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	public interface IDataTransferLocationMigrationHelper
	{
		string GetUpdatedSourceConfiguration(Data.IntegrationPoint integrationPoint, IList<string> processingSourceLocations, string newDataTransferLocationRoot);
	}
}