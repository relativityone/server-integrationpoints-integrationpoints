
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using kCura.Relativity.Export.Exports;
using kCura.Relativity.Export.Types;
using Relativity;

namespace kCura.Relativity.Export.FileObjects
{


    public class ExportFileDefBuilder
    {

        public static ExportFile CreateDefSetup(int exportedObjArtifactId, int workspaceId, string password, string userName, string exportFilesLocation, List<Types.ViewFieldInfo> selViewFieldInfos, int artifactTypeId = 10)
        {

            ExportFile expFile = new ExportFile(artifactTypeId);



            expFile.AppendOriginalFileName = false;
            expFile.ArtifactID = exportedObjArtifactId;

            expFile.CaseInfo = new CaseInfo();
            expFile.CaseInfo.ArtifactID = workspaceId;



            expFile.CookieContainer = new System.Net.CookieContainer();

            expFile.Credential = new NetworkCredential();
            expFile.Credential.Password = password;
            expFile.Credential.UserName = userName;

            expFile.ExportFullText = false;

            expFile.ExportImages = true;

            expFile.ExportFullTextAsFile = false;
            expFile.ExportNative = true;

            expFile.ExportNativesToFileNamedFrom = ExportNativeWithFilenameFrom.Identifier;



            expFile.FilePrefix = "";
            expFile.FolderPath = exportFilesLocation;


            expFile.IdentifierColumnName = "Control Number";


            List<Pair> imagePrecs = new List<Pair>();
            imagePrecs.Add(new Pair("Original", "-1"));

            expFile.ImagePrecedence = imagePrecs.ToArray();
            expFile.LoadFileEncoding = System.Text.Encoding.Default;
            expFile.LoadFileExtension = "dat";

            expFile.LoadFileIsHtml = false;

            expFile.LoadFilesPrefix = "Extracted Text Only";
            expFile.LogFileFormat = LoadFileType.FileFormat.Opticon;

            expFile.ObjectTypeName = "Document";
            expFile.Overwrite = true;


            expFile.RenameFilesToIdentifier = true;



            expFile.SelectedViewFields = selViewFieldInfos.ToArray();


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
