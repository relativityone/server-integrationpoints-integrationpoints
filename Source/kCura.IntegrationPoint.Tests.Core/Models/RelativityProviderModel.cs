﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class RelativityProviderModel : IntegrationPointGeneralModel
	{
        public RelativityProviderModel(string ripName) : base(ripName)
        {
            Scheduler = null;
	        DestinationProvider = "Relativity";
        }
		
        public SchedulerModel Scheduler { get; set; }

		#region "Source Details"

		public string Source { get; set; }
		public string SavedSearch { get; set; }
		public string ProductionSet { get; set; }

		#endregion

		#region "Destination Details"
		public string RelativityInstance { get; set; }
		public string DestinationWorkspace { get; set; }
		public LocationEnum? Location { get; set; }
		
		#endregion


		#region "Field Mappings"
		public List<string> SourceFields;
		public List<string> DestinationFields;
		#endregion


		#region "Relativity Provider Settings"
		
		
		[DefaultValue(OverwriteModeEnum.AppendOnly)]
		public OverwriteModeEnum? Overwrite { get; set; }
		[DefaultValue(false)]
		public bool? CopyImages;
		public ImagePrecedenceEnum? ImagePrecedence { get; set; }
		public CopyNativeFilesEnum? CopyNativeFiles { get; set; }
		public string MultiSelectFieldOverlay;
		[DefaultValue(UseFolderPathInformationEnum.No)]
		public UseFolderPathInformationEnum? UseFolderPathInformation { get; set; }
		public string FolderPathInformation;
		public bool? MoveExistingDocuments;
		public bool? CopyFilesToRepository;


		#endregion
		public bool? CreateSavedSearch { get; set; }

		public enum LocationEnum
		{
			Folder,
			ProductionSet
		}

		public enum CopyNativeFilesEnum
		{
			PhysicalFiles,
			LinksOnly,
			No
		}

		public enum UseFolderPathInformationEnum
		{
			No,
			ReadFromField,
			ReadFromFolderTree
		}

		public enum OverwriteModeEnum
		{
			AppendOnly,
			OverlayOnly,
			AppendOverlay
		}

		public enum ImagePrecedenceEnum
		{
			OriginalImages,
			ProducedImages
		}
	}
}
