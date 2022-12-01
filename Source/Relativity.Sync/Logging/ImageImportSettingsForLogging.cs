using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Logging
{
    internal class ImageImportSettingsForLogging : ImportSettingsForLoggingBase
    {
        private ImageImportSettingsForLogging(ImageSettings settings) : base(settings)
        {
            ArtifactTypeId = settings.ArtifactTypeId;
            AutoNumberImages = settings.AutoNumberImages;
            BeginBatesFieldArtifactID = settings.BeginBatesFieldArtifactID;
            DisableImageLocationValidation = settings.DisableImageLocationValidation;
            DisableImageTypeValidation = settings.DisableImageTypeValidation;
            ForProduction = settings.ForProduction;
            ProductionArtifactID = settings.ProductionArtifactID;

            BatesNumberField = RemoveSensitiveDataIfNotEmpty(settings.BatesNumberField);
            DocumentIdentifierField = RemoveSensitiveDataIfNotEmpty(settings.DocumentIdentifierField);
            FileLocationField = RemoveSensitiveDataIfNotEmpty(settings.FileLocationField);
            FileNameField = RemoveSensitiveDataIfNotEmpty(settings.FileNameField);
            FolderPathSourceFieldName = RemoveSensitiveDataIfNotEmpty(settings.FolderPathSourceFieldName);
            ImageFilePathSourceFieldName = RemoveSensitiveDataIfNotEmpty(settings.ImageFilePathSourceFieldName);
            SelectedCasePath = RemoveSensitiveDataIfNotEmpty(settings.SelectedCasePath);
        }

        public static ImageImportSettingsForLogging CreateWithoutSensitiveData(ImageSettings settings)
        {
            return new ImageImportSettingsForLogging(settings);
        }

        #region Properties

        public int ArtifactTypeId { get; set; }

        public bool AutoNumberImages { get; set; }

        public string BatesNumberField { get; set; }

        public int BeginBatesFieldArtifactID { get; set; }

        public bool? DisableImageLocationValidation { get; set; }

        public bool? DisableImageTypeValidation { get; set; }

        public string DocumentIdentifierField { get; set; }

        public string FileLocationField { get; set; }

        public string FileNameField { get; set; }

        public string FolderPathSourceFieldName { get; set; }

        public bool ForProduction { get; set; }

        public string ImageFilePathSourceFieldName { get; set; }

        public int ProductionArtifactID { get; set; }

        public string SelectedCasePath { get; set; }

        #endregion
    }
}
