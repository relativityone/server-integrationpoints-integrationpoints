Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Xml

Namespace GeneratorHelper
	Public Class SchemaHelper
		Private Shared _lastSchemaFile As String = ""
		Private Shared _objectTypes As List(Of ObjectTypeDTO)
		Public Shared Function GetObjectTypes(ByVal schemaFile As String) As List(Of ObjectTypeDTO)
			If _objectTypes IsNot Nothing AndAlso schemaFile = _lastSchemaFile Then Return _objectTypes
			_lastSchemaFile = schemaFile
			_objectTypes = New List(Of ObjectTypeDTO)()
			Dim d = New System.Xml.XmlDocument()
			d.Load(schemaFile)
			For Each objNode In d.SelectNodes("/Application/Objects/Object")
				_objectTypes.Add(ParseObject(objNode))
			Next
			ConvertNamesToUserFields()
			ResolveParentArtifactTypeNames()
			FindReflectedFields()
			Return _objectTypes
		End Function

		Private Shared Sub ConvertNamesToUserFields()
			For Each obj In _objectTypes

				Dim nameField = From f In obj.SystemFields Where f.FieldCategoryId = 2
				If (nameField.Any()) Then
					obj.NameField = nameField.First()
					obj.UserFields.Add(obj.NameField)
					obj.SystemFields.Remove(obj.NameField)
				End If
			Next
		End Sub

		Private Shared Sub ResolveParentArtifactTypeNames()
			Dim types = New Dictionary(Of Integer, String)()
			types.Add(2, "User")
			types.Add(3, "Group")
			types.Add(4, "View")
			types.Add(7, "Code")
			types.Add(8, "Workspace")
			types.Add(9, "Folder")
			types.Add(10, "Document")
			types.Add(13, "Email")
			types.Add(14, "Field")
			types.Add(15, "Search")
			types.Add(16, "Layout")
			types.Add(17, "Production")
			types.Add(19, "Report")
			types.Add(22, "MarkupSet")
			types.Add(23, "Tab")
			types.Add(24, "BatchSet")
			types.Add(25, "ObjectType")
			types.Add(26, "Search Folder")
			types.Add(27, "Batch")
			types.Add(28, "RelativityScript")
			types.Add(29, "SearchIndex")
			For Each obj In _objectTypes
				If Not types.ContainsKey(obj.DescriptorArtifactTypeId) Then types.Add(obj.DescriptorArtifactTypeId, obj.Name)
			Next
			For Each obj In _objectTypes
				If (types.ContainsKey(Integer.Parse(obj.ParentArtifactTypeId))) Then
					obj.ParentArtifactTypeName = types(Integer.Parse(obj.ParentArtifactTypeId))
				Else
					obj.ParentArtifactTypeName = "Workspace"
				End If
			Next
		End Sub

		Private Shared Sub FindReflectedFields()
			'' find fields of type object and multi-object
			'' find associatable fields on the objectType for those fields
			'' modify or extend FieldDTO
			'For Each objType In _objectTypes
			'	For Each ofield in objType.UserFields.Where(f => f.FieldTypeId == (int)FieldTypeHelper.FieldType.Object)
			'		For Each  oType in objTypes.Where(o => o.DescriptorArtifactTypeId == ofield.AssociativeArtifactTypeId)
			'			For Each field In oType.UserFields.Where(f >= f.IsAvailableToAssociativeObjects)

			'			Next
			'		Next
			'	Next
			'Next
		End Sub

		Private Shared Function ParseObject(ByVal objNode As XmlNode) As ObjectTypeDTO
			Dim objType = New ObjectTypeDTO()
			objType.Name = objNode.SelectSingleNode("Name").InnerText
			objType.Guid = objNode.SelectSingleNode("Guid").InnerText
			objType.ArtifactId = objNode.SelectSingleNode("ArtifactId").InnerText
			objType.DescriptorArtifactTypeId = Integer.Parse(objNode.SelectSingleNode("DescriptorArtifactTypeId").InnerText)
			objType.ParentArtifactTypeId = objNode.SelectSingleNode("ParentArtifactTypeId").InnerText

			Dim tab = objNode.SelectSingleNode("Tab")
			If (Not tab Is Nothing) Then objType.Tab = ParseTab(tab)

			For Each fieldNode In objNode.SelectNodes("Fields/Field")
				Dim field = ParseField(fieldNode)
				If Not (From f In objType.UserFields Select f.DisplayName).Contains(field.DisplayName) Then objType.UserFields.Add(field)
			Next
			For Each fieldNode In objNode.SelectNodes("SystemFields/SystemField")
				objType.SystemFields.Add(ParseField(fieldNode))
			Next

			For Each node In objNode.SelectNodes("Layouts/Layout")
				objType.UserLayouts.Add(ParseLayout(node))
			Next
			For Each node In objNode.SelectNodes("SystemLayouts/SystemLayout")
				objType.SystemLayouts.Add(ParseLayout(node))
			Next

			For Each node In objNode.SelectNodes("Views/View")
				objType.UserViews.Add(ParseView(node))
			Next
			For Each node In objNode.SelectNodes("SystemViews/SystemView")
				objType.SystemViews.Add(ParseView(node))
			Next

			Return objType
		End Function

		Private Shared Function ParseField(ByVal fieldNode As XmlNode) As FieldDTO
			Dim field = New FieldDTO()
			field.DisplayName = fieldNode.SelectSingleNode("DisplayName").InnerText
			field.FieldTypeId = Integer.Parse(fieldNode.SelectSingleNode("FieldTypeId").InnerText)
			field.FieldCategoryId = Integer.Parse(fieldNode.SelectSingleNode("FieldCategoryId").InnerText)
			field.FieldArtifactTypeId = Integer.Parse(fieldNode.SelectSingleNode("FieldArtifactTypeId").InnerText)
			field.Guid = fieldNode.SelectSingleNode("Guid").InnerText
			field.IsAvailableToAssociativeObjects = Boolean.Parse(fieldNode.SelectSingleNode("IsAvailableToAssociativeObjects").InnerText)
			If (fieldNode.SelectSingleNode("AssociativeArtifactTypeId").InnerText.Length > 0) Then
				field.AssociativeArtifactTypeId = Integer.Parse(fieldNode.SelectSingleNode("AssociativeArtifactTypeId").InnerText)
			End If
			field.IsReflected = (field.FieldCategoryId = 3 OrElse field.FieldCategoryId = 14)
			field.MaxLength = fieldNode.SelectSingleNode("MaxLength").InnerText
			For Each codeNode In fieldNode.SelectNodes("Codes/Code")
				field.Codes.Add(ParseCode(codeNode))
			Next
			Return field
		End Function

		Private Shared Function ParseCode(ByVal codeNode As XmlNode) As CodeDTO
			Dim code = New CodeDTO()
			code.Name = codeNode.SelectSingleNode("Name").InnerText
			code.Guid = Guid.Parse(codeNode.SelectSingleNode("Guid").InnerText)
			Return code
		End Function

		Private Shared Function ParseLayout(ByVal fieldNode As XmlNode) As LayoutDTO
			Dim layout = New LayoutDTO()
			layout.Guid = fieldNode.SelectSingleNode("Guid").InnerText
			layout.Keywords = fieldNode.SelectSingleNode("Keywords").InnerText
			layout.Name = fieldNode.SelectSingleNode("Name").InnerText
			layout.Notes = fieldNode.SelectSingleNode("Notes").InnerText
			layout.LayoutArtifactTypeId = Integer.Parse(fieldNode.SelectSingleNode("LayoutArtifactTypeId").InnerText)
			layout.Order = Integer.Parse(fieldNode.SelectSingleNode("Order").InnerText)
			Return layout
		End Function

		Private Shared Function ParseView(ByVal fieldNode As XmlNode) As ViewDTO
			Dim view = New ViewDTO()
			view.Guid = fieldNode.SelectSingleNode("Guid").InnerText
			view.Keywords = fieldNode.SelectSingleNode("Keywords").InnerText
			view.Name = fieldNode.SelectSingleNode("Name").InnerText
			view.Notes = fieldNode.SelectSingleNode("Notes").InnerText
			view.QueryHint = fieldNode.SelectSingleNode("QueryHint").InnerText
			view.Type = fieldNode.SelectSingleNode("Type").InnerText
			view.ArtifactId = Integer.Parse(fieldNode.SelectSingleNode("ArtifactId").InnerText)
			view.ArtifactTypeId = Integer.Parse(fieldNode.SelectSingleNode("ArtifactTypeId").InnerText)
			view.Order = Integer.Parse(fieldNode.SelectSingleNode("Order").InnerText)
			view.ViewByFamily = Integer.Parse(fieldNode.SelectSingleNode("ViewByFamily").InnerText)
			view.IsRelationalIndexView = Boolean.Parse(fieldNode.SelectSingleNode("IsRelationalIndexView").InnerText)
			view.IsReport = Boolean.Parse(fieldNode.SelectSingleNode("IsReport").InnerText)
			view.IsVisible = Boolean.Parse(fieldNode.SelectSingleNode("IsVisible").InnerText)
			view.RenderLinks = Boolean.Parse(fieldNode.SelectSingleNode("RenderLinks").InnerText)
			view.AvailableInObjectTab = Boolean.Parse(fieldNode.SelectSingleNode("AvailableInObjectTab").InnerText)
			Return view
		End Function

		Private Shared Function ParseTab(ByVal fieldNode As XmlNode) As TabDTO
			Dim tab = New TabDTO()
			tab.Guid = fieldNode.SelectSingleNode("Guid").InnerText
			tab.Name = fieldNode.SelectSingleNode("Name").InnerText
			tab.ArtifactId = Integer.Parse(fieldNode.SelectSingleNode("ArtifactId").InnerText)
			tab.DisplayOrder = Integer.Parse(fieldNode.SelectSingleNode("DisplayOrder").InnerText)
			tab.ObjectArtifactTypeId = Integer.Parse(fieldNode.SelectSingleNode("ObjectArtifactTypeId").InnerText)
			tab.ParentArtifactId = Integer.Parse(fieldNode.SelectSingleNode("ParentArtifactId").InnerText)
			tab.IsDefault = Boolean.Parse(fieldNode.SelectSingleNode("IsDefault").InnerText)
			Return tab
		End Function

	End Class
End Namespace