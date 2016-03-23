using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace PropertyExtractor
{
	public class File
	{

		public System.Collections.Hashtable Extract(string fileName)
		{

			System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);

			System.Collections.Hashtable hashTable = new System.Collections.Hashtable();
			hashTable.Add("CreationTime".ToLower(), fileInfo.CreationTime.ToString());
			hashTable.Add("LastModifiedTime".ToLower(), fileInfo.LastWriteTime.ToString());
			hashTable.Add("DirectoryName".ToLower(), fileInfo.DirectoryName);
			hashTable.Add("Extention".ToLower(), fileInfo.Extension);
			hashTable.Add("FullName".ToLower(), fileInfo.FullName);
			hashTable.Add("Size".ToLower(), fileInfo.Length.ToString());
			hashTable.Add("Name".ToLower(), fileInfo.Name);

			return hashTable;

		}
	}
}
