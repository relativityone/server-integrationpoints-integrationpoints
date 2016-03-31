
Imports Relativity

Namespace kCura.Relativity.Export.Service
	Public Class CaseManager
		Inherits kCura.EDDS.WebAPI.CaseManagerBase.CaseManager

		Public Sub New(ByVal credentials As Net.ICredentials, ByVal cookieContainer As System.Net.CookieContainer)
			MyBase.New()

			Me.Credentials = credentials
			Me.CookieContainer = cookieContainer
			Me.Url = String.Format("{0}CaseManager.asmx", Config.WebServiceURL)
			Me.Timeout = Settings.DefaultTimeOut
		End Sub

		Protected Overrides Function GetWebRequest(ByVal uri As System.Uri) As System.Net.WebRequest
			Dim wr As System.Net.HttpWebRequest = DirectCast(MyBase.GetWebRequest(uri), System.Net.HttpWebRequest)
			wr.UnsafeAuthenticatedConnectionSharing = True
			wr.Credentials = Me.Credentials
			Return wr
		End Function

		Public Shared Function ConvertToCaseInfo(ByVal toConvert As kCura.EDDS.WebAPI.CaseManagerBase.CaseInfo) As CaseInfo
			Dim c As New CaseInfo
			With toConvert
				c.ArtifactID = .ArtifactID
				c.MatterArtifactID = .MatterArtifactID
				c.Name = .Name
				c.RootArtifactID = .RootArtifactID
				c.RootFolderID = .RootFolderID
				c.StatusCodeArtifactID = .StatusCodeArtifactID
				c.EnableDataGrid = .EnableDataGrid
				c.DocumentPath = .DocumentPath
				c.DownloadHandlerURL = .DownloadHandlerURL
			End With
			Return c
		End Function

#Region " Shadow Functions "
		Public Shadows Function RetrieveAll() As System.Data.DataSet
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					If kCura.WinEDDS.Config.UsesWebAPI Then
						Return MyBase.RetrieveAllEnabled()
					Else
						'Return _caseManager.RetrieveAll(_identity).ToDataSet()
					End If
				Catch ex As System.Exception
					If TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("NeedToReLoginException") <> -1 AndAlso tries < Config.MaxReloginTries Then
						Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries, False)
					Else
						Throw
					End If
				End Try
			End While
			Return Nothing
		End Function

		Public Shadows Function Read(ByVal caseArtifactID As Int32) As Relativity.CaseInfo
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					Return ConvertToCaseInfo(MyBase.Read(caseArtifactID))
				Catch ex As System.Exception
					If TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("NeedToReLoginException") <> -1 Then
						If tries < Config.MaxReloginTries Then
							Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries, False)
						Else
							Throw ex
						End If
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