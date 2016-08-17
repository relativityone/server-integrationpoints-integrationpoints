Imports System
Imports System.Collections.Generic

Namespace GeneratorHelper
	Public Class FieldTypeHelper

		Public Shared Function CSType(ByVal fieldTypeId As Integer) As String
			Dim d = New Dictionary(Of FieldType, String)()
			d.Add(FieldType.Varchar, "string")
			d.Add(FieldType.Text, "string")
			d.Add(FieldType.LayoutText, "string")
			d.Add(FieldType.Code, "Choice")

			d.Add(FieldType.Integer, "int?")

			d.Add(FieldType.User, "User")
			d.Add(FieldType.File, "int?")
			d.Add(FieldType.Object, "int?")

			d.Add(FieldType.Date, "DateTime?")

			d.Add(FieldType.Boolean, "bool?")

			d.Add(FieldType.Decimal, "Decimal?")
			d.Add(FieldType.Currency, "Decimal?")

			d.Add(FieldType.Objects, "int[]")

			d.Add(FieldType.MultiCode, "Choice[]")

			If Not d.ContainsKey(DirectCast(fieldTypeId, FieldType)) Then Throw New Exception("Unkonwn Field Type Id.")
			Return d(DirectCast(fieldTypeId, FieldType))
		End Function

		Public Shared Function VBType(ByVal fieldTypeId As Integer) As String
			Dim d = New Dictionary(Of FieldType, String)()
			d.Add(FieldType.Varchar, "String")
			d.Add(FieldType.Text, "String")
			d.Add(FieldType.LayoutText, "String")
			d.Add(FieldType.Code, "Choice")

			d.Add(FieldType.Integer, "Integer?")

			d.Add(FieldType.User, "User")
			d.Add(FieldType.File, "Integer?")
			d.Add(FieldType.Object, "Integer?")

			d.Add(FieldType.Date, "DateTime?")

			d.Add(FieldType.Boolean, "Boolean?")

			d.Add(FieldType.Decimal, "Decimal?")
			d.Add(FieldType.Currency, "Decimal?")

			d.Add(FieldType.Objects, "Integer()")

			d.Add(FieldType.MultiCode, "Choice()")

			If Not d.ContainsKey(DirectCast(fieldTypeId, FieldType)) Then Throw New Exception("Unkonwn Field Type Id.")
			Return d(DirectCast(fieldTypeId, FieldType))
		End Function

		Public Enum FieldType
			Empty = -1
			Varchar = 0
			[Integer] = 1
			[Date] = 2
			[Boolean] = 3
			[Text] = 4
			Code = 5
			[Decimal] = 6
			Currency = 7
			MultiCode = 8
			File = 9
			[Object] = 10
			User = 11
			LayoutText = 12
			Objects = 13
		End Enum

		Public Enum RSFieldType
			'Empty = -1
			FixedLengthText = 0
			WholeNumber = 1
			[Date] = 2
			YesNo = 3
			LongText = 4
			SingleChoice = 5
			[Decimal] = 6
			Currency = 7
			MultipleChoice = 8
			File = 9
			SingleObject = 10
			User = 11
			'LayoutText = 12
			MultipleObject = 13
		End Enum

	End Class
End Namespace