using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract
{
	public abstract class BaseExportTestCase : IExportTestCase
	{
		protected ExportSettings _exportSettings;

		public virtual ExportSettings Prepare(ExportSettings settings)
		{
			_exportSettings = settings;

            _exportSettings.ExportFilesLocation += $"_{GetType().Name}";

            return _exportSettings;
		}

		public abstract void Verify(DirectoryInfo directory, DataTable documents, DataTable images);
	}
}
