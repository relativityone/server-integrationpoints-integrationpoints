Namespace kCura.Relativity.Export.Service
	Public Class AuditManager
		Inherits kCura.EDDS.WebAPI.AuditManagerBase.AuditManager

#Region "Constructors"

		Public Sub New(ByVal credentials As Net.ICredentials, ByVal cookieContainer As System.Net.CookieContainer)
			MyBase.New()

			Me.Credentials = credentials
			Me.CookieContainer = cookieContainer
			Me.Url = String.Format("{0}AuditManager.asmx", Config.WebServiceURL)
			Me.Timeout = Settings.DefaultTimeOut
		End Sub

#End Region

#Region " Shadow Methods "
		Public Shadows Function CreateAuditRecord(ByVal caseContextArtifactID As Int32, ByVal artifactID As Int32, ByVal action As Int32, ByVal details As String, ByVal origination As String) As Boolean
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					Return MyBase.CreateAuditRecord(caseContextArtifactID, artifactID, action, details, origination)
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

		Public Shadows Function AuditImageImport(ByVal appID As Int32, ByVal runId As String, ByVal isFatalError As Boolean, ByVal importStats As kCura.EDDS.WebAPI.AuditManagerBase.ImageImportStatistics) As Boolean
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					Return MyBase.AuditImageImport(appID, runId, isFatalError, importStats)
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

		Public Shadows Function AuditObjectImport(ByVal appID As Int32, ByVal runId As String, ByVal isFatalError As Boolean, ByVal importStats As kCura.EDDS.WebAPI.AuditManagerBase.ObjectImportStatistics) As Boolean
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					Return MyBase.AuditObjectImport(appID, runId, isFatalError, importStats)
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

		Public Shadows Function AuditExport(ByVal appID As Int32, ByVal isFatalError As Boolean, ByVal exportStats As kCura.EDDS.WebAPI.AuditManagerBase.ExportStatistics) As Boolean
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					Return MyBase.AuditExport(appID, isFatalError, exportStats)
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
