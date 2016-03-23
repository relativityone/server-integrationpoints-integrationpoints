Namespace kCura.Relativity.Export.Process

	Public Class ExportStatistics
		Inherits Statistics
		Public Overrides Function ToDictionary() As IDictionary
			Dim retval As New System.Collections.Specialized.HybridDictionary
			If Not Me.FileTime = 0 Then retval.Add("Average file transfer rate", ToFileSizeSpecification(Me.FileBytes / (Me.FileTime / 10000000)) & "/sec")
			If Not Me.MetadataTime = 0 AndAlso Not Me.MetadataBytes = 0 Then retval.Add("Average metadata transfer rate (includes SQL processing)", ToFileSizeSpecification(Me.MetadataBytes / (Me.MetadataTime / 10000000)) & "/sec")
			Return retval
		End Function
	End Class
End Namespace
