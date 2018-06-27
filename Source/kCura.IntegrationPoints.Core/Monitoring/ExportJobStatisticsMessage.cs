﻿using System;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class ExportJobStatisticsMessage : JobApmMessageBase
	{
		private const string _FILE_BYTES_KEY_NAME = "FileBytes";
		private const string _METADATA_KEY_NAME = "MetadataBytes";

		public long FileBytes
		{
			get { return this.GetValueOrDefault<long>(_FILE_BYTES_KEY_NAME); }
			set { CustomData[_FILE_BYTES_KEY_NAME] = value; }
		}

		public long MetaBytes
		{
			get { return this.GetValueOrDefault<long>(_METADATA_KEY_NAME); }
			set { CustomData[_METADATA_KEY_NAME] = value; }
		}
	}
}