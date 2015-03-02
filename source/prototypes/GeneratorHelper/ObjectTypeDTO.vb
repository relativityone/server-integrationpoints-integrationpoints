Imports System.Collections.Generic

Namespace GeneratorHelper
	Public Class ObjectTypeDTO
		Public Property Name As String
		Public Property Guid As String
		Public Property ArtifactId As String
		Public Property ParentArtifactTypeId As String
		Public Property DescriptorArtifactTypeId As Integer
		Public Property ParentArtifactTypeName As String
		Public Property NameField As FieldDTO
		Public Property Tab As TabDTO

		Public Property UserFields As List(Of FieldDTO) = New List(Of FieldDTO)
		Public Property SystemFields As List(Of FieldDTO) = New List(Of FieldDTO)
		Public Property ReflectedFields As List(Of FieldDTO) = New List(Of FieldDTO)

		Public Property UserViews As List(Of ViewDTO) = New List(Of ViewDTO)
		Public Property SystemViews As List(Of ViewDTO) = New List(Of ViewDTO)

		Public Property UserLayouts As List(Of LayoutDTO) = New List(Of LayoutDTO)
		Public Property SystemLayouts As List(Of LayoutDTO) = New List(Of LayoutDTO)

	End Class
End Namespace