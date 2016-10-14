﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Contracts;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
    public class PreviewJob
    {
        private bool _errorsOnly;
        public PreviewJob()
        {
            IsComplete = false;
            IsFailed = false;
            _errorsOnly = false;       
        }

        public void Init(NetworkCredential authenticatedCredential, ImportPreviewSettings settings)
        {
            NativeSettingsFactory factory = new kCura.WinEDDS.NativeSettingsFactory(authenticatedCredential, settings.WorkspaceId);
            LoadFile eddsLoadFile = factory.ToLoadFile();
            
            if (settings.PreviewType == "errors")
            {
                _errorsOnly = true;
            }
            //delimiter settings
            eddsLoadFile.RecordDelimiter = (char)settings.AsciiColumn;
            eddsLoadFile.QuoteDelimiter = (char)settings.AsciiQuote;
            eddsLoadFile.NewlineDelimiter = (char)settings.AsciiNewLine;
            eddsLoadFile.MultiRecordDelimiter = (char)settings.AsciiMultiLine;
            eddsLoadFile.HierarchicalValueDelimiter = (char)settings.AsciiNestedValue;
            eddsLoadFile.SourceFileEncoding = Encoding.GetEncoding(settings.EncodingType);

            eddsLoadFile.FilePath = settings.LoadFile;
            eddsLoadFile.LoadNativeFiles = false;
            eddsLoadFile.CreateFolderStructure = false;

            //Create obj
            LoadFileReader temp = new kCura.WinEDDS.LoadFileReader(eddsLoadFile, false);

            //set up field mapping to extract all fields with reader
            string[] cols = temp.GetColumnNames(eddsLoadFile);
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
                        fieldCat = 2;
                    }

                    var newDocField = new kCura.WinEDDS.DocumentField(currentField.DestinationField.DisplayName, int.Parse(currentField.DestinationField.FieldIdentifier), 4, fieldCat, -1, -1, -1, false,
                        kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

                    //The column index we give here determines which column in the load file gets mapped to this Doc Field
                    var newfieldMapItem = new kCura.WinEDDS.LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx);

                    eddsLoadFile.FieldMap.Add(newfieldMapItem);
                }
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
