using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Api;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Helpers
{
	public class PreviewHelper
	{
		private HybridDictionary _totalFolders = new HybridDictionary();
		public ImportPreviewTable BuildFoldersAndChoices(List<object> arrs, HybridDictionary previewCodeCount)
		{
			ImportPreviewTable preview = new ImportPreviewTable();
			List<string> folderInfo = new List<string>();
			List<List<string>> choiceInfo = new List<List<string>>();
			preview.Header.Add("Field Name");
			preview.Header.Add("Count");
			folderInfo.Add("Folder");

			List<int> codeFieldColumnIndexes = GetCodeFieldColumnIndexes(((ArtifactField[])arrs[0]).ToList());
			int folderCount = GetFolderCount(arrs);
			if (folderCount != -1)
			{
				folderInfo.Add(folderCount.ToString());
			}
			else
			{
				folderInfo.Add("0");
			}

			preview.Data.Add(folderInfo);
			//now add info for the choices
			if (codeFieldColumnIndexes.Count == 0)
			{
				choiceInfo.Add(new List<string>(new string[] { "Choice", "No choice fields have been mapped" }));
			}
			else
			{
				foreach (string key in previewCodeCount.Keys)
				{
					List<string> row = new List<string>();
					row.Add(key.Split(new char[] { '_' }, 2)[1]);//this adds the column with the choice field's name
					HybridDictionary currentFieldHashTable = (HybridDictionary)previewCodeCount[key];
					row.Add(currentFieldHashTable.Count.ToString());//this adds the column that has the count corresponding to the choice field
					choiceInfo.Add(row);

					currentFieldHashTable.Clear();
				}
			}
			preview.Data.AddRange(choiceInfo);

			return preview;
		}

		private int GetFolderCount(List<object> folderFieldList)
		{
			_totalFolders.Clear();
			string fieldValue = null;
			ArtifactField[] fields = null;
			Int32 folderColumnIndex = GetFolderColumnIndex(((ArtifactField[])folderFieldList[0]).ToList());

			if (folderColumnIndex == -1)
			{
				return -1;
			}

			foreach (object item in folderFieldList)
			{
				if ((item != null))
				{
					fields = (ArtifactField[])item;
					fieldValue = fields[folderColumnIndex].ValueAsString;
					AddFoldersToTotalFolders(fieldValue);
				}
			}
			return _totalFolders.Count;

		}

		private int GetFolderColumnIndex(List<ArtifactField> firstRow)
		{
			Int32 folderColumnIndex = -1;
			Int32 currentIndex = 0;

			foreach (ArtifactField field in firstRow)
			{
				if (field.DisplayName == "Parent Folder Identifier")
				{
					folderColumnIndex = currentIndex;
					break;
				}
				currentIndex += 1;
			}
			return folderColumnIndex;
		}

		private void AddFoldersToTotalFolders(string folderPath)
		{
			if (!String.IsNullOrEmpty(folderPath) && folderPath != "\\")
			{
				if (folderPath.LastIndexOf('\\') < 1)
				{
					if (!_totalFolders.Contains(folderPath))
						_totalFolders.Add(folderPath, string.Empty);
				}
				else
				{
					if (!_totalFolders.Contains(folderPath))
					{
						_totalFolders.Add(folderPath, string.Empty);
					}
					AddFoldersToTotalFolders(folderPath.Substring(0, folderPath.LastIndexOf('\\')));
				}
			}
		}

		private static List<int> GetCodeFieldColumnIndexes(List<ArtifactField> firstRow)
		{
			List<int> codeFieldColumnIndexes = new List<int>();
			Int32 currentIndex = 0;
			foreach (ArtifactField field in firstRow)
			{
				if (field.Type == global::Relativity.FieldTypeHelper.FieldType.Code || field.Type == global::Relativity.FieldTypeHelper.FieldType.MultiCode)
				{
					codeFieldColumnIndexes.Add(currentIndex);
				}
				currentIndex += 1;
			}
			return codeFieldColumnIndexes;
		}
	}
}
