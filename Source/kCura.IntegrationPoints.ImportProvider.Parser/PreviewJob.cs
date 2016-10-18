using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Contracts;
using kCura.WinEDDS;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using System.Runtime.CompilerServices;
using REL = Relativity;

[assembly: InternalsVisibleTo("kCura.IntegrationPoints.ImportProvider.Parser.Tests")]
namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class PreviewJob : IPreviewJob
    {
        internal bool _errorsOnly;
        public PreviewJob()
        {
            IsComplete = false;
            IsFailed = false;
            _errorsOnly = false;       
        }

        public void Init(LoadFile loadFile, ImportPreviewSettings settings)
        {
            if (settings.PreviewType == "errors")
            {
                _errorsOnly = true;
            }

            //Create obj
            LoadFileReader temp = new kCura.WinEDDS.LoadFileReader(loadFile, false);

            //set up field mapping to extract all fields with reader
            string[] cols = temp.GetColumnNames(loadFile);
            int colIdx = 0;
            foreach (string colName in cols)
            {
                int fieldCat = -1;
                FieldMap currentField = settings.FieldMapping.Where(f => f.SourceField.DisplayName == colName).FirstOrDefault();
                if (currentField != null)
                {
                    //set as an identifier
                    if (currentField.SourceField.IsIdentifier)
                    {
                        fieldCat = (int)REL.FieldCategory.Identifier;
                    }

                    var newDocField = new kCura.WinEDDS.DocumentField(currentField.DestinationField.DisplayName, int.Parse(currentField.DestinationField.FieldIdentifier), 4, fieldCat, -1, -1, -1, false,
                        kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

                    //The column index we give here determines which column in the load file gets mapped to this Doc Field
                    var newfieldMapItem = new kCura.WinEDDS.LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx);

                    loadFile.FieldMap.Add(newfieldMapItem);
                }
                colIdx++;
            }

            _loadFilePreviewer = new kCura.WinEDDS.LoadFilePreviewer(loadFile, 0, _errorsOnly, false);
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
                //using var in this foreach since we don't know the type of each item in the arraylist (can be ArtifactField[] or Exception)
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
                    foreach (List<string> row in preview.Data)
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
            if (e.Type == LoadFilePreviewer.EventType.Complete)
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

        internal LoadFilePreviewer _loadFilePreviewer;

        public ImportPreviewTable PreviewTable { get; private set; }

        public bool IsComplete { get; internal set; }
        public bool IsFailed { get; internal set; }
        public string ErrorMessage { get; internal set;}
        public long TotalBytes { get; internal set; }
        public long BytesRead { get; internal set; }
        public long StepSize { get; internal set; }
    }
}
