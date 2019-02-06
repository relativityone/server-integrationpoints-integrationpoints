using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Base;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter
{
	[TestFixture]
	public class DocumentTransferDataReaderTests : ExportTransferDataReaderTestsBase
	{
		protected override ExportTransferDataReaderBase CreateDataReaderTestInstance()
		{
			return new DocumentTransferDataReader(
				_exportService,
				_templateFieldEntries,
				_context,
				_scratchRepositories, 
				_relativityObjectManager,
				Substitute.For<IAPILog>(), 
				false);
		}

		protected override ExportTransferDataReaderBase CreateDataReaderTestInstanceWithParameters(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			BaseServiceContext context,
			IScratchTableRepository[] scratchTableRepositories)
		{
			return new DocumentTransferDataReader(
				relativityExportService,
				fieldMappings,
				context,
				scratchTableRepositories,
				_relativityObjectManager,
				Substitute.For<IAPILog>(), 
				false);
		}
	}
}