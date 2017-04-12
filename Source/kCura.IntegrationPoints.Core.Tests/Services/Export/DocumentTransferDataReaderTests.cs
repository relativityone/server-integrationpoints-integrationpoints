using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.Core;

namespace kCura.IntegrationPoints.Core.Tests.Services.Export
{
	[TestFixture]
	public class DocumentTransferDataReaderTests : ExportTransferDataReaderTestsBase
	{
		protected override ExportTransferDataReaderBase CreatetDataReaderTestInstance()
		{
			return new DocumentTransferDataReader(
				_exportService,
				_templateFieldEntries,
				_context,
				_scratchRepositories, false);
		}

		protected override ExportTransferDataReaderBase CreatetDataReaderTestInstanceWithParameters(
			IExporterService relativityExportService,
			FieldMap[] fieldMappings,
			ICoreContext context,
			IScratchTableRepository[] scratchTableRepositories)
		{
			return new DocumentTransferDataReader(
				relativityExportService,
				fieldMappings,
				context,
				scratchTableRepositories, false);
		}
	}
}