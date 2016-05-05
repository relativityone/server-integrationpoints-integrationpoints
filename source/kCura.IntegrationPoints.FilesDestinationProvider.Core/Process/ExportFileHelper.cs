using System.Collections.Generic;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	class ExportFileHelper
	{
		internal static ExportFile CreateDefaultSetup(ExportSettings exportSettings)
		{
			ExportFile expFile = new ExportFile(exportSettings.ArtifactTypeId);
			//ExportFile expFile = new ExportFile(exportSettings.ArtifactTypeId);​
			expFile.AppendOriginalFileName = false;
			expFile.ArtifactID = exportSettings.ExportedObjArtifactId;
			expFile.CaseInfo = new CaseInfo();
			expFile.CaseInfo.ArtifactID = exportSettings.WorkspaceId;

			
			expFile.ExportFullText = false;
			expFile.ExportImages = true;
			expFile.ExportFullTextAsFile = false;
			expFile.ExportNative = true;
			expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;
			expFile.FilePrefix = "";
			expFile.FolderPath = exportSettings.ExportFilesLocation;
			expFile.IdentifierColumnName = "Control Number";
			List<Pair> imagePrecs = new List<Pair>();
			imagePrecs.Add(new Pair("-1", "Original"));
			expFile.ImagePrecedence = imagePrecs.ToArray();
			expFile.LoadFileEncoding = System.Text.Encoding.Default;
			expFile.LoadFileExtension = "dat";
			expFile.LoadFileIsHtml = false;
			expFile.LoadFilesPrefix = "Extracted Text Only";
			expFile.LogFileFormat = LoadFileType.FileFormat.Opticon;
			expFile.ObjectTypeName = "Document";
			expFile.Overwrite = true;
			expFile.RenameFilesToIdentifier = true;
			expFile.StartAtDocumentNumber = 0;
			expFile.SubdirectoryDigitPadding = 3;
			expFile.TextFileEncoding = null;
			expFile.TypeOfExport = ExportFile.ExportType.ArtifactSearch;
			expFile.TypeOfExportedFilePath = ExportFile.ExportedFilePathType.Relative;
			expFile.TypeOfImage = ExportFile.ImageType.SinglePage;
			expFile.ViewID = 0;
			expFile.VolumeDigitPadding = 2;
			expFile.VolumeInfo = new VolumeInfo();
			expFile.VolumeInfo.VolumePrefix = "VOL";
			expFile.VolumeInfo.VolumeStartNumber = 1;
			expFile.VolumeInfo.VolumeMaxSize = 650;
			expFile.VolumeInfo.SubdirectoryStartNumber = 1;
			expFile.VolumeInfo.SubdirectoryMaxSize = 500;
			expFile.VolumeInfo.CopyFilesFromRepository = true;
			return expFile;
		}
	}
}
