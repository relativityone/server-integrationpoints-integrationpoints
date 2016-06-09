using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	public interface IExportSettingsBuilder
	{
		ExportSettings Create(ExportUsingSavedSearchSettings sourceSettings, IEnumerable<FieldMap> fieldMap, int artifactTypeId);
	}
}
