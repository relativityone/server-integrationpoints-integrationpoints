Namespace kCura.Relativity.Export.Service
	Public Class FieldManager
		Inherits EDDS.WebAPI.FieldManagerBase.FieldManager

		Private _query As FieldQuery
		Public ReadOnly Property Query() As FieldQuery
			Get
				Return _query
			End Get
		End Property

		Protected Overrides Function GetWebRequest(ByVal uri As System.Uri) As System.Net.WebRequest
			Dim wr As System.Net.HttpWebRequest = DirectCast(MyBase.GetWebRequest(uri), System.Net.HttpWebRequest)
			wr.UnsafeAuthenticatedConnectionSharing = True
			wr.Credentials = Me.Credentials
			Return wr
		End Function

		Public Sub New(ByVal credentials As Net.ICredentials, ByVal cookieContainer As System.Net.CookieContainer)
			MyBase.New()

			Me.Credentials = credentials
			Me.CookieContainer = cookieContainer
			_query = New FieldQuery(credentials, Me.CookieContainer)
			Me.Url = String.Format("{0}FieldManager.asmx", Config.WebServiceURL)
			Me.Timeout = Settings.DefaultTimeOut
		End Sub


#Region " Translations "
		'Public Shared Function DTOtoDocumentField(ByVal dto As kCura.EDDS.WebAPI.DocumentManagerBase.Field) As DocumentField
		'	Dim retval As New DocumentField(dto.DisplayName, dto.ArtifactID, dto.FieldTypeID, dto.FieldCategoryID, dto.CodeTypeID, dto.MaxLength, dto.AssociativeArtifactTypeID, dto.UseUnicodeEncoding, dto.ImportBehavior, dto.EnableDataGrid)
		'	If retval.FieldCategoryID = Relativity.FieldCategory.FullText Then
		'		retval.Value = System.Text.ASCIIEncoding.ASCII.GetString(DirectCast(dto.Value, Byte()))
		'	ElseIf retval.FieldTypeID = Relativity.FieldTypeHelper.FieldType.Code OrElse retval.FieldTypeID = Relativity.FieldTypeHelper.FieldType.MultiCode Then
		'		retval.Value = kCura.Utility.Array.ToCsv(DirectCast(dto.Value, Int32())).Replace(",", ";")
		'	Else
		'		retval.Value = dto.Value.ToString
		'	End If
		'	Return retval
		'End Function

		'Public Shared Function DTOsToDocumentField(ByVal dtos As kCura.EDDS.WebAPI.DocumentManagerBase.Field()) As DocumentField()
		'	Dim documentFields(dtos.Length - 1) As DocumentField
		'	Dim i As Int32
		'	For i = 0 To documentFields.Length - 1
		'		documentFields(i) = DTOtoDocumentField(dtos(i))
		'	Next
		'	Return documentFields
		'End Function

#End Region

#Region " Shadow Functions "
		Public Shadows Function Create(ByVal caseContextArtifactID As Int32, ByVal field As kCura.EDDS.WebAPI.FieldManagerBase.Field) As Int32
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					If Config.UsesWebAPI Then
						Return MyBase.Create(caseContextArtifactID, field)
					Else
						'Return _fieldManager.ExternalCreate(Me.WebAPIFieldtoDTO(field), _identity)
					End If
				Catch ex As System.Exception
					If TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("NeedToReLoginException") <> -1 AndAlso tries < Config.MaxReloginTries Then
						'Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					Else
						Throw
					End If
				End Try
			End While
			Return Nothing
		End Function

		Public Shadows Function Read(ByVal caseContextArtifactID As Int32, ByVal fieldArtifactID As Int32) As kCura.EDDS.WebAPI.FieldManagerBase.Field
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					If Config.UsesWebAPI Then
						Return MyBase.Read(caseContextArtifactID, fieldArtifactID)
					Else
						'Return Me.DTOtoFieldWebAPIField(_fieldManager.Read(fieldArtifactID, _identity))
					End If
				Catch ex As System.Exception
					If TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("NeedToReLoginException") <> -1 AndAlso tries < Config.MaxReloginTries Then
						'Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					Else
						Throw
					End If
				End Try
			End While
			Return Nothing
		End Function
#End Region

	End Class
End Namespace