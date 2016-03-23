Namespace PropertyExtractor
	Public Class File

		Public Function Extract(ByVal fileName As String) As System.Collections.Hashtable

			Dim fileInfo As New System.IO.FileInfo(fileName)

			Dim hashTable As New System.Collections.Hashtable
			hashTable.Add("CreationTime".ToLower, fileInfo.CreationTime.ToString)
			hashTable.Add("LastModifiedTime".ToLower, fileInfo.LastWriteTime.ToString)
			hashTable.Add("DirectoryName".ToLower, fileInfo.DirectoryName)
			hashTable.Add("Extention".ToLower, fileInfo.Extension)
			hashTable.Add("FullName".ToLower, fileInfo.FullName)
			hashTable.Add("Size".ToLower, fileInfo.Length.ToString)
			hashTable.Add("Name".ToLower, fileInfo.Name)

			Return hashTable

		End Function
	End Class
End Namespace