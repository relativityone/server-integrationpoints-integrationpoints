﻿using System;
using System.Data;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class ImportTransferDataContext : IDataTransferContext, IDisposable
	{
		public ImportTransferDataContext(IDataReaderFactory dataReaderFactory, string providerSettings, List<FieldMap> mappedFields)
		{
			DataReader = dataReaderFactory.GetDataReader(mappedFields.ToArray(), providerSettings);
		}

		public IDataReader DataReader { get; set; }

		public int? TotalItemsFound { get; set; }

		public void UpdateTransferStatus(){}

		public void Dispose()
		{
			DataReader?.Dispose();
		}
	}
}
