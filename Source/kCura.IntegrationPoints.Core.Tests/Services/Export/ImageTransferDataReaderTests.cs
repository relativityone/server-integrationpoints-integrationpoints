using System.Data;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using Relativity.Core;

namespace kCura.IntegrationPoints.Core.Tests.Services.Export
{

	[TestFixture]
	public class ImageTransferDataReaderTests : ExportTransferDataReaderTestsBase
	{
		protected override ExportTransferDataReaderBase CreatetDataReaderTestInstance()
		{
			return new ImageTransferDataReader(
				_exportService,
				_templateFieldEntries,
				_context,
				_scratchRepositories);
		}

		protected override ExportTransferDataReaderBase CreatetDataReaderTestInstanceWithParameters(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			ICoreContext context,
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