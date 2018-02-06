using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
	using System.ComponentModel;

	public class ExportToLoadFileProviderModel : IntegrationPointGeneralModel
	{
		public ExportToLoadFileSourceInformationModel SourceInformationModel { get; set; }
		public ExportToLoadFileDetailsModel ExportDetails { get; set; } = new ExportToLoadFileDetailsModel();
		public ExportToLoadFileVolumeAndSubdirectoryModel ToLoadFileVolumeAndSubdirectoryModel { get; set; } = new ExportToLoadFileVolumeAndSubdirectoryModel();
		public ExportToLoadFileOutputSettingsModel OutputSettings { get; set; } = new ExportToLoadFileOutputSettingsModel();

		public ExportToLoadFileProviderModel(string name, string savedSearch) : base(name)
		{
			DestinationProvider = INTEGRATION_POINT_PROVIDER_LOADFILE;

			SourceInformationModel = new ExportToLoadFileSourceInformationModel(savedSearch);
		}
		
		public enum FilePathTypeEnum
		{
			Relative,
			Absolute,
			UserPrefix
		}
	}
}
