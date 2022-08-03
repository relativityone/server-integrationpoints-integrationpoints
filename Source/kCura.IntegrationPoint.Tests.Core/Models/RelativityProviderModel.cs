using System;
using System.Collections.Generic;
using System.ComponentModel;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class RelativityProviderModel : IntegrationPointGeneralModel
    {
        public RelativityProviderModel(string ripName) : base(ripName)
        {
            DestinationProvider = "Relativity";
        }

        #region "Source Details"

        [DefaultValue(SourceTypeEnum.SavedSearch)]
        public SourceTypeEnum? Source { get; set; }
        [DefaultValue("All documents")]
        public string SavedSearch { get; set; }
        public string ProductionSet { get; set; }

        #endregion

        #region "Destination Details"
        public string RelativityInstance { get; set; }
        public string DestinationWorkspace { get; set; }
        public LocationEnum? Location { get; set; }
        
        #endregion


        #region "Field Mappings"
        public List<Tuple<string, string>> FieldMapping { get; set; }
        #endregion


        #region "Relativity Provider Settings"
        
        
        [DefaultValue(OverwriteModeEnum.AppendOnly)]
        public OverwriteModeEnum? Overwrite { get; set; }
        [DefaultValue(false)]
        public bool CopyImages;
        public ImagePrecedence? ImagePrecedence { get; set; }
        public CopyNativeFilesEnum? CopyNativeFiles { get; set; }
        [DefaultValue(MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings)]
        public MultiSelectFieldOverlayBehaviorEnum? MultiSelectFieldOverlay;
        [DefaultValue(UseFolderPathInformationEnum.No)]
        public UseFolderPathInformationEnum? UseFolderPathInformation { get; set; }
        public string FolderPathInformation;
        public bool? MoveExistingDocuments;
        public bool? CopyFilesToRepository;
        public bool? IncludeOriginalImagesIfNotProduced;


        #endregion
        public bool? CreateSavedSearch { get; set; }
        public string SourceProductionName { get; set; }
        public string DestinationProductionName { get; set; }

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

        public enum MultiSelectFieldOverlayBehaviorEnum
        {
            MergeValues,
            ReplaceValues,
            UseFieldSettings
        }

        public enum SourceTypeEnum
        {
            SavedSearch,
            Production
        }

        public string SourceTypeTextUi()
        {
            return Source == RelativityProviderModel.SourceTypeEnum.SavedSearch
                ? "Saved Search"
                : "Production";
        }
    }
}
