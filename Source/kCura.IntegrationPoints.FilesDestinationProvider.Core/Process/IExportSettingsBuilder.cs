﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using ExportSettings = kCura.IntegrationPoints.Core.Models.ExportSettings;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IExportSettingsBuilder
	{
		ExportSettings Create(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId);
	}
}