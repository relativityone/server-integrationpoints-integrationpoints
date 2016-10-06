using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Config;
using kCura.WinEDDS;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class PreviewJob
    {

        public PreviewJob(ICredentialProvider credentialProvider, string loadFile, int workspaceId)
        {
            IsComplete = false;

            //Set up Config object with WebAPI link
            var webApiConfig = new WebApiConfig();
            WinEDDS.Config.WebServiceURL = webApiConfig.GetWebApiUrl;

            var cookeiContainer = new System.Net.CookieContainer();

            var factory = new kCura.WinEDDS.NativeSettingsFactory(credentialProvider.Authenticate(cookeiContainer), workspaceId);
            var eddsLoadFile = factory.ToLoadFile();

            eddsLoadFile.RecordDelimiter = ',';
            eddsLoadFile.FilePath = loadFile;
            eddsLoadFile.LoadNativeFiles = false;
            eddsLoadFile.CreateFolderStructure = false;

            //Create obj
            var temp = new kCura.WinEDDS.LoadFileReader(eddsLoadFile, false);

            //set up field mapping to extract all fields with reader
            var cols = temp.GetColumnNames(eddsLoadFile);
            int colIdx = 0;
            foreach (var col in cols)
            {
                int fieldCat = -1;
                //setting the first column as the identifier
                if (colIdx == 0)
                {
                    fieldCat = 2;
                }
                //the fieldID here is purely and ID not an index of any kind
                //In production, we need to basically do this (grab cols from loadFileReader, but insert the column name of the 
                //MAPPED destination column display name rather than the column name in the load file it's self
                //What I need, is this method to get 2 parallel arrays of the source and dest column names that are mapped

                var newDocField = new kCura.WinEDDS.DocumentField(col, colIdx * 100, 4, fieldCat, -1, -1, -1, false,
                    kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

                //The column index we give here determines which column in the load file gets mapped to this Doc Field
                var newfieldMapItem = new kCura.WinEDDS.LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx);

                eddsLoadFile.FieldMap.Add(newfieldMapItem);

                colIdx++;
            }

            _loadFilePreviewer = new kCura.WinEDDS.LoadFilePreviewer(eddsLoadFile, 0, false, false);

        }

        public void StartRead()
        {
            _loadFilePreviewer.OnEvent += OnPreviewerProgress;

            ArrayList arrs = (ArrayList)_loadFilePreviewer.ReadFile("", 0);
            ImportPreviewTable preview = new ImportPreviewTable();
            preview.Header = (arrs[0] as kCura.WinEDDS.Api.ArtifactField[]).Select(i => i.DisplayName).ToList();
            foreach (kCura.WinEDDS.Api.ArtifactField[] item in arrs)
            {
                List<string> row = item.Select(i => i.Value.ToString()).ToList();
                preview.Data.Add(row);
            }
            //TODO: remove, this is for thread testing purposes
            System.Threading.Thread.Sleep(30000);
            IsComplete = true;
            PreviewTable = preview;
        }

        private void OnPreviewerProgress(kCura.WinEDDS.LoadFilePreviewer.EventArgs e)
        {
            if (e.Type == LoadFilePreviewer.EventType.Progress)
            {
                TotalBytes = e.TotalBytes;
            }
            else if (e.Type == LoadFilePreviewer.EventType.Complete)
            {
                TotalBytes = e.TotalBytes;
                IsComplete = true;
            }

        }

        public void DisposePreviewJob()
        {
            _loadFilePreviewer.OnEvent -= OnPreviewerProgress;
            
        }

        private LoadFilePreviewer _loadFilePreviewer;

        public ImportPreviewTable PreviewTable { get; set; }

        public bool IsComplete { get; set; }

        public long TotalBytes { get; set; }
    }
}
