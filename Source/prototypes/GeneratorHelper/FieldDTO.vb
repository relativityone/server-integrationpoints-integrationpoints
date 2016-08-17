Imports System.Collections.Generic

Namespace GeneratorHelper
	Public Class FieldDTO
		Public Property DisplayName As String
		Public Property FieldTypeId As Integer
		Public Property FieldCategoryId As Integer
		Public Property FieldArtifactTypeId As Integer
		Public Property Guid As String
		Public Property IsAvailableToAssociativeObjects As Boolean
		Public Property MaxLength As String
		Public Property Codes As List(Of CodeDTO) = New List(Of CodeDTO)
		Public Property AssociativeArtifactTypeId As Integer
		Public Property IsReflected As Boolean
	End Class
End Namespace