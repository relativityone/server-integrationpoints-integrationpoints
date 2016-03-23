Imports System.Runtime.Serialization
Imports System.Collections.Generic
Imports Relativity

Namespace kCura.Relativity.Export.Types
	<Serializable()> Public Class DocumentField
		Implements ISerializable

#Region "Members"
		Private _fieldName As String
		Private _fieldID As Int32
		Private _fieldTypeID As Int32
		Private _value As String
		Private _fieldCategoryID As Int32
		Private _codeTypeID As Nullable(Of Int32)
		Private _fileColumnIndex As Int32
		Private _fieldLength As Nullable(Of Int32)
#End Region


#Region "Properties"

		<NonSerialized()> Private _associatedObjectTypeID As Nullable(Of Int32)
		Public Property AssociatedObjectTypeID() As Nullable(Of Int32)
			Get
				Return _associatedObjectTypeID
			End Get
			Set(ByVal Value As Nullable(Of Int32))
				_associatedObjectTypeID = Value
			End Set
		End Property

		<NonSerialized()> Private _useUnicode As Boolean
		Public Property UseUnicode() As Boolean
			Get
				Return _useUnicode
			End Get
			Set(ByVal value As Boolean)
				_useUnicode = value
			End Set
		End Property

		<NonSerialized()> Private _enableDataGrid As Boolean
		Public Property EnableDataGrid() As Boolean
			Get
				Return _enableDataGrid
			End Get
			Set(ByVal value As Boolean)
				_enableDataGrid = value
			End Set
		End Property

		Public Property FieldName() As String
			Get
				Return _fieldName
			End Get
			Set(ByVal value As String)
				_fieldName = value
			End Set
		End Property

		Public Property FieldID() As Int32
			Get
				Return _fieldID
			End Get
			Set(ByVal value As Int32)
				_fieldID = value
			End Set
		End Property

		Public Property FieldTypeID() As Int32
			Get
				Return _fieldTypeID
			End Get
			Set(ByVal value As Int32)
				_fieldTypeID = value
			End Set
		End Property

		Public Property FieldCategoryID() As Int32
			Get
				Return _fieldCategoryID
			End Get
			Set(ByVal value As Int32)
				_fieldCategoryID = value
			End Set
		End Property

		Public Property FieldCategory() As FieldCategory
			Get
				Return CType(_fieldCategoryID, FieldCategory)
			End Get
			Set(ByVal value As FieldCategory)
				_fieldCategoryID = value
			End Set
		End Property

		Public Property Value() As String
			Get
				Return _value
			End Get
			Set(ByVal value As String)
				_value = value
			End Set
		End Property

		Public Property CodeTypeID() As Nullable(Of Int32)
			Get
				Return _codeTypeID
			End Get
			Set(ByVal value As Nullable(Of Int32))
				_codeTypeID = value
			End Set
		End Property

		Public Property FileColumnIndex() As Int32
			Get
				Return _fileColumnIndex
			End Get
			Set(ByVal value As Int32)
				_fileColumnIndex = value
			End Set
		End Property

		Public Property FieldLength() As Nullable(Of Int32)
			Get
				Return _fieldLength
			End Get
			Set(ByVal value As Nullable(Of Int32))
				_fieldLength = value
			End Set
		End Property

		Public Property Guids() As List(Of Guid)

#End Region

#Region "Constructors"

		Private Sub New(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal Context As System.Runtime.Serialization.StreamingContext)
			Me.FieldName = info.GetString("_fieldName")
			Me.FieldID = info.GetInt32("_fieldID")
			Me.FieldTypeID = info.GetInt32("_fieldTypeID")
			Me.Value = info.GetString("_value")
			Me.FieldCategoryID = info.GetInt32("_fieldCategoryID")
			Me.FileColumnIndex = info.GetInt32("_fileColumnIndex")
			Me.Guids = New List(Of Guid)
		End Sub

		Public Sub New(ByVal fieldName As String, ByVal fieldID As Int32, ByVal fieldTypeID As Int32, ByVal fieldCategoryID As Int32, ByVal codeTypeID As Nullable(Of Int32), ByVal fieldLength As Nullable(Of Int32), ByVal associatedObjectTypeID As Nullable(Of Int32), ByVal useUnicode As Boolean, ByVal enableDataGrid As Boolean)
			Me.New(fieldName, fieldID, fieldTypeID, fieldCategoryID, codeTypeID, fieldLength, associatedObjectTypeID, useUnicode, New List(Of Guid), enableDataGrid)
		End Sub

		Public Sub New(ByVal fieldName As String, ByVal fieldID As Int32, ByVal fieldTypeID As Int32, ByVal fieldCategoryID As Int32, ByVal codeTypeID As Nullable(Of Int32), ByVal fieldLength As Nullable(Of Int32), ByVal associatedObjectTypeID As Nullable(Of Int32), ByVal useUnicode As Boolean, guids As IEnumerable(Of Guid), ByVal enableDataGrid As Boolean)
			MyBase.New()
			_fieldName = fieldName
			_fieldID = fieldID
			_fieldTypeID = fieldTypeID
			_fieldCategoryID = fieldCategoryID
			_codeTypeID = codeTypeID
			_fieldLength = fieldLength
			_associatedObjectTypeID = associatedObjectTypeID
			_useUnicode = useUnicode
			_enableDataGrid = enableDataGrid
			If (Not guids Is Nothing) Then
				Me.Guids = guids.ToList()
			Else
				Me.Guids = New List(Of Guid)
			End If
		End Sub

		Public Sub New(ByVal docField As DocumentField)
			Me.New(docField.FieldName, docField.FieldID, docField.FieldTypeID, docField.FieldCategoryID, docField.CodeTypeID, docField.FieldLength, docField.AssociatedObjectTypeID, docField.UseUnicode, docField.EnableDataGrid)
		End Sub

#End Region

		Public Function ToDisplayString() As String
			Return String.Format("DocumentField[{0},{1},{2},{3},'{4}']", FieldCategoryID, FieldID, FieldName, FieldTypeID, kCura.Utility.NullableTypesHelper.ToEmptyStringOrValue(CodeTypeID))
		End Function

		Public Overrides Function ToString() As String
			Return FieldName
		End Function

		Public Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData
			info.AddValue("_fieldName", Me.FieldName, GetType(String))
			info.AddValue("_fieldID", Me.FieldID, GetType(Int32))
			info.AddValue("_fieldTypeID", Me.FieldTypeID, GetType(Int32))
			info.AddValue("_value", Me.Value, GetType(String))
			info.AddValue("_fieldCategoryID", Me.FieldCategoryID, GetType(Int32))
			info.AddValue("_fileColumnIndex", Me.FileColumnIndex, GetType(Int32))
		End Sub

		Public Shared Function op_Equality(ByVal df1 As DocumentField, ByVal df2 As DocumentField) As Boolean
			Dim areEqual As Boolean
			If df1.CodeTypeID Is Nothing Then
				If df2.CodeTypeID Is Nothing Then
					areEqual = True
				Else
					areEqual = False
				End If
			Else
				If df2.CodeTypeID Is Nothing Then
					areEqual = True
				Else
					areEqual = df1.CodeTypeID.Value = df2.CodeTypeID.Value
				End If
			End If
			areEqual = areEqual And df1.FieldCategoryID = df2.FieldCategoryID
			areEqual = areEqual And df1.FieldName = df2.FieldName
			areEqual = areEqual And df1.FieldTypeID = df2.FieldTypeID
			areEqual = areEqual And df1.Value = df2.Value
			Return areEqual
		End Function

	End Class

End Namespace
