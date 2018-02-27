using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Core.Services.Exporter.Images;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.Core;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{

	[TestFixture]
	public class ImageTransferDataReaderTests : ExportTransferDataReaderTestsBase
	{
		protected override ExportTransferDataReaderBase CreateDataReaderTestInstance()
		{
			return new ImageTransferDataReader(
				_exportService,
				_templateFieldEntries,
				_context,
				_scratchRepositories);
		}

		protected override ExportTransferDataReaderBase CreateDataReaderTestInstanceWithParameters(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			BaseServiceContext context,
			IScratchTableRepository[] scratchTableRepositories)
		{
			return new ImageTransferDataReader(
				relativityExportService,
				fieldMappings,
				context,
				scratchTableRepositories);
		}
	}
}