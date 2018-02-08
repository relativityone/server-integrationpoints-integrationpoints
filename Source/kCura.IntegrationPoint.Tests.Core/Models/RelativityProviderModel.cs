using System;
using System.Collections.Generic;
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
		
		public OverwriteModeEnum Overwrite { get; set; }
		public bool? CopyImages;
		public CopyNativeFilesEnum? CopyNativeFiles;
		public string MultiSelectFieldOverlay;
		public UseFolderPathInformationEnum? UseFolderPathInformation;
		public string FolderPathInformation;
		public bool? MoveExistingDocuments;


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
	}
}
