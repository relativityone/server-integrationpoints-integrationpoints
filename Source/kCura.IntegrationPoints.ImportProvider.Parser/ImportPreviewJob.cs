using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class PreviewJob
    {
        private bool _errorsOnly;
        public PreviewJob(NetworkCredential authenticatedCredential, ImportPreviewSettings settings)
        {
            IsComplete = false;
            IsFailed = false;

            var factory = new kCura.WinEDDS.NativeSettingsFactory(authenticatedCredential, settings.WorkspaceId);
            var eddsLoadFile = factory.ToLoadFile();
            _errorsOnly = false;
            if(settings.PreviewType == "errors")
            {
                _errorsOnly = true;
            }
            eddsLoadFile.RecordDelimiter = ',';
            eddsLoadFile.FilePath = settings.FilePath;
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

            _loadFilePreviewer = new kCura.WinEDDS.LoadFilePreviewer(eddsLoadFile, 0, _errorsOnly, false);            
        }

        public void StartRead()
        {
            try
            {
                _loadFilePreviewer.OnEvent += OnPreviewerProgress;
                ArrayList arrs = (ArrayList)_loadFilePreviewer.ReadFile("", 0);
                ImportPreviewTable preview = new ImportPreviewTable();

                bool populatedHeaders = false;

                //create header and default to one field w/ empty string in case we only return error rows and don't get any headers
                preview.Header.Add(string.Empty);
                int columnNumbers = 1;
                int dataRowIndex = 1;//this will be used to populate the list of rows with an error
                foreach (var item in arrs)
                {
                    List<string> row = new List<string>();
                    //check the type to see if we got back an array or an exception
                    if (item.GetType() != typeof(kCura.WinEDDS.Api.ArtifactField[]))
                    {
                        //if the item is not an ArtifactField array, it means we have an error
                        row = new List<string>();
                        string errorString = ((Exception)item).Message;
                        for (int i = 0; i < columnNumbers; i++)
                        {
                            row.Add(errorString);
                        }

                        if (!_errorsOnly)
                        {
                            preview.ErrorRows.Add(dataRowIndex);
                        }
                    }
                    else
                    {
                        if (!populatedHeaders)
                        {
                            preview.Header = ((kCura.WinEDDS.Api.ArtifactField[])item).Select(i => i.DisplayName).ToList();
                            columnNumbers = preview.Header.Count();
                            populatedHeaders = true;
                        }
                        row = ((kCura.WinEDDS.Api.ArtifactField[])item).Select(i => i.Value.ToString()).ToList();
                        //check to see if any of the cells have an error so we can highlight red in UI
                        //we won't do this if the user has requested only errors to come back
                        if (!_errorsOnly)
                        {
                            foreach (string cell in row)
                            {
                                if (cell.StartsWith("Error: "))
                                {
                                    preview.ErrorRows.Add(dataRowIndex);
                                    break;
                                }
                            }
                        }
                    }
                    preview.Data.Add(row);
                    dataRowIndex++;
                }

                //update any error rows that were created before we hit a row that allowed us to populate the full header list
                if (populatedHeaders)
                {
                    foreach (var row in preview.Data)
                    {
                        while (row.Count < columnNumbers)
                        {
                            row.Add(row[0]);
                        }
                    }
                }

                IsComplete = true;
                PreviewTable = preview;
            }
            catch (Exception ex)
            {
                IsFailed = true;
                ErrorMessage = ex.Message;
            }
        }

        private void OnPreviewerProgress(kCura.WinEDDS.LoadFilePreviewer.EventArgs e)
        {
            if (e.Type == LoadFilePreviewer.EventType.Progress)
            {
                
            }
            else if (e.Type == LoadFilePreviewer.EventType.Complete)
            {
                IsComplete = true;
            }
            BytesRead = e.BytesRead;
            TotalBytes = e.TotalBytes;
            StepSize = e.StepSize;

        }

        public void DisposePreviewJob()
        {
            _loadFilePreviewer.OnEvent -= OnPreviewerProgress;
            
        }

        private LoadFilePreviewer _loadFilePreviewer;

        public ImportPreviewTable PreviewTable { get; private set; }

        public bool IsComplete { get; private set; }
        public bool IsFailed { get; private set; }
        public string ErrorMessage { get; private set;}
        public long TotalBytes { get; private set; }
        public long BytesRead { get; private set; }
        public long StepSize { get; private set; }
    }
}
