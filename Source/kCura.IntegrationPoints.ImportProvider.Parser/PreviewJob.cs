using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Contracts;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using System.Runtime.CompilerServices;
using kCura.IntegrationPoints.ImportProvider.Parser.Helpers;


[assembly: InternalsVisibleTo("kCura.IntegrationPoints.ImportProvider.Parser.Tests")]
namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class PreviewJob : IPreviewJob
	{
		internal bool _errorsOnly;
		internal bool _foldersAndChoices;
		internal LoadFile _loadFile;

		public PreviewJob()
		{
			IsComplete = false;
			IsFailed = false;
			_errorsOnly = false;
			_foldersAndChoices = false;	   
		}

		public void Init(LoadFile loadFile, ImportPreviewSettings settings)
		{
			_loadFile = loadFile;
			//check if the user selected ExtractedText and set the current directory so that the extracted text files can be found via relative paths
			if (!String.IsNullOrEmpty(settings.ExtractedTextColumn))
			{
				_loadFile.LongTextColumnThatContainsPathToFullText = settings.ExtractedTextColumn;
				_loadFile.ExtractedTextFileEncoding = Encoding.GetEncoding(settings.ExtractedTextFileEncoding);
				System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(_loadFile.FilePath));
			}

			if (settings.PreviewType == (int)PreviewType.PreviewTypeEnum.Errors)
			{
				_errorsOnly = true;
			}
			else if(settings.PreviewType == (int)PreviewType.PreviewTypeEnum.Folders)
			{				
				_foldersAndChoices = true;
			}

			//Create obj
			LoadFileReader temp = new kCura.WinEDDS.LoadFileReader(_loadFile, false);

			//set up field mapping to extract all fields with reader
			string[] cols = temp.GetColumnNames(_loadFile);
			int colIdx = 0;
			foreach (string colName in cols)
			{
				FieldMap currentField = settings.FieldMapping.Where(f => f.SourceField.DisplayName == colName).FirstOrDefault();
				//Make sure that the current column exists in the fieldMapping object and has a destination mapped or is a folderInfo mapping
				if (ShouldFieldBeIncludedInPreview(currentField))
				{
					string docFieldName;
					int docFieldIdentifier;
					int fieldCategory = GetFieldCategory(currentField);
					int fieldTypeId = GetFieldTypeId(currentField, settings.ChoiceFields);

					docFieldName = (currentField.DestinationField.DisplayName != null) ? currentField.DestinationField.DisplayName : currentField.SourceField.DisplayName;
					docFieldIdentifier = (!String.IsNullOrEmpty(currentField.DestinationField.FieldIdentifier)) ? int.Parse(currentField.DestinationField.FieldIdentifier): int.Parse(currentField.SourceField.FieldIdentifier);

					DocumentField newDocField = new DocumentField(docFieldName, docFieldIdentifier, fieldTypeId, fieldCategory, -1, -1, -1, false,
						kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

					//The column index we give here determines which column in the load file gets mapped to this Doc Field
					LoadFileFieldMap.LoadFileFieldMapItem newfieldMapItem = new LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx);

					_loadFile.FieldMap.Add(newfieldMapItem);
				}
				colIdx++;
			}

			_loadFilePreviewer = new LoadFilePreviewerWrapper(_loadFile, 0, _errorsOnly, false);
		}

		public void StartRead()
		{
			try
			{
				_loadFilePreviewer.OnEventAdd(OnPreviewerProgress);
				List<object> arrs = _loadFilePreviewer.ReadFile(_foldersAndChoices);
				ImportPreviewTable preview = new ImportPreviewTable();

				if (_foldersAndChoices)
				{
					PreviewHelper previewHelper = new PreviewHelper();
					preview = previewHelper.BuildFoldersAndChoices(arrs, _loadFile.PreviewCodeCount);
				}
				else
				{
					preview = BuildPreviewTable(arrs);
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
			_loadFilePreviewer.OnEventRemove(OnPreviewerProgress);			
		}
		
		private ImportPreviewTable BuildPreviewTable(List<object> arrs)
		{
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
						preview.Header.Clear();
						preview.Header.AddRange(((kCura.WinEDDS.Api.ArtifactField[])item).Select(i => i.DisplayName).ToList());
						columnNumbers = preview.Header.Count();
						populatedHeaders = true;
					}
					row = ((kCura.WinEDDS.Api.ArtifactField[])item).Select(i => i.Value.ToString()).ToList();
					//check to see if any of the cells have an error so we can highlight red in UI
					//we won't do this if the user has requested only errors to come back
					if (!_errorsOnly)
					{
						if (!string.IsNullOrEmpty(row.FirstOrDefault(r => r.StartsWith("Error: "))))
						{
							preview.ErrorRows.Add(dataRowIndex);
						}
					}
				}

				preview.Data.Add(row);
				dataRowIndex++;
			}

			//update any error rows that were created before we hit a row that allowed us to populate the full header list
			if (populatedHeaders)
			{
				foreach (List<string> dataRow in preview.Data)
				{
					while (dataRow.Count < columnNumbers)
					{
						dataRow.Add(dataRow[0]);
					}
				}
			}

			return preview;
		}

		private int GetFieldCategory(FieldMap currentField)
		{
			int fieldCat = -1;
			//set as an identifier
			if (currentField.SourceField.IsIdentifier)
			{
				fieldCat = (int)global::Relativity.FieldCategory.Identifier;
			}
			else if (currentField.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
			{
				if (_foldersAndChoices)
				{
					fieldCat = (int)global::Relativity.FieldCategory.ParentArtifact;
					_loadFile.CreateFolderStructure = true;
				}
			}

			return fieldCat;
		}

		private int GetFieldTypeId(FieldMap currentField, List<string> choiceFields)
		{
			int fieldTypeId = (int)global::Relativity.FieldTypeHelper.FieldType.Text;

			if (choiceFields.Contains(currentField.DestinationField.DisplayName))
			{
				fieldTypeId = (int)global::Relativity.FieldTypeHelper.FieldType.Code;
			}

			return fieldTypeId;
		}

		private bool ShouldFieldBeIncludedInPreview(FieldMap currentField)
		{
			return (currentField != null && ((!String.IsNullOrEmpty(currentField.DestinationField.DisplayName) ||
					(currentField.FieldMapType == FieldMapTypeEnum.FolderPathInformation && _foldersAndChoices))));
		}

		internal ILoadFilePreviewer _loadFilePreviewer;
		public ImportPreviewTable PreviewTable { get; private set; }
		
		public bool IsComplete { get; internal set; }
		public bool IsFailed { get; internal set; }
		public string ErrorMessage { get; internal set;}
		public long TotalBytes { get; internal set; }
		public long BytesRead { get; internal set; }
		public long StepSize { get; internal set; }
	}
}
