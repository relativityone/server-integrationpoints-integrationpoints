Namespace kCura.Relativity.Export.Service
	Public Class ExportManager
		Inherits kCura.EDDS.WebAPI.ExportManagerBase.ExportManager

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
			Me.Url = String.Format("{0}ExportManager.asmx", Config.WebServiceURL)
			Me.Timeout = Settings.DefaultTimeOut
		End Sub

		Private Function MakeCallAttemptReLogin(Of T)(f As Func(Of T)) As T
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					Return f()
				Catch ex As System.Exception
					UnpackHandledException(ex)
					If TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("NeedToReLoginException") <> -1 AndAlso tries < Config.MaxReloginTries Then
						'Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					Else
						Throw
					End If
				End Try
			End While
			Return Nothing
		End Function

		Public Shadows Function InitializeFolderExport(ByVal appID As Int32, ByVal viewArtifactID As Int32, ByVal parentArtifactID As Int32, ByVal includeSubFolders As Boolean, ByVal avfIds As Int32(), ByVal startAtRecord As Int32, ByVal artifactTypeID As Int32) As kCura.EDDS.WebAPI.ExportManagerBase.InitializationResults
			Return MakeCallAttemptReLogin(Function() MyBase.InitializeFolderExport(appID, viewArtifactID, parentArtifactID, includeSubFolders, avfIds, startAtRecord, artifactTypeID))
		End Function

		Public Shadows Function InitializeProductionExport(ByVal appID As Int32, ByVal productionArtifactID As Int32, ByVal avfIds As Int32(), ByVal startAtRecord As Int32) As kCura.EDDS.WebAPI.ExportManagerBase.InitializationResults
			Return MakeCallAttemptReLogin(Function() MyBase.InitializeProductionExport(appID, productionArtifactID, avfIds, startAtRecord))
		End Function

		Public Shadows Function InitializeSearchExport(ByVal appID As Int32, ByVal searchArtifactID As Int32, ByVal avfIds As Int32(), ByVal startAtRecord As Int32) As kCura.EDDS.WebAPI.ExportManagerBase.InitializationResults
			Return MakeCallAttemptReLogin(Function() MyBase.InitializeSearchExport(appID, searchArtifactID, avfIds, startAtRecord))
		End Function

		Public Shadows Function RetrieveResultsBlock(ByVal appID As Int32, ByVal runId As Guid, ByVal artifactTypeID As Int32, ByVal avfIds As Int32(), ByVal chunkSize As Int32, ByVal displayMulticodesAsNested As Boolean, ByVal multiValueDelimiter As Char, ByVal nestedValueDelimiter As Char, ByVal textPrecedenceAvfIds As Int32()) As Object()
			Dim retval As Object() = MakeCallAttemptReLogin(Function() MyBase.RetrieveResultsBlock(appID, runId, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds))
			If Not retval Is Nothing Then
				For Each row As Object() In retval
					If row Is Nothing Then
						Throw New System.Exception("Invalid (null) row retrieved from server")
					End If
					For i As Int32 = 0 To row.Length - 1
						If TypeOf row(i) Is Byte() Then row(i) = System.Text.Encoding.Unicode.GetString(DirectCast(row(i), Byte()))
					Next
				Next
			End If
			Return retval
		End Function

		Public Shadows Function RetrieveResultsBlockForProduction(ByVal appID As Int32, ByVal runId As Guid, ByVal artifactTypeID As Int32, ByVal avfIds As Int32(), ByVal chunkSize As Int32, ByVal displayMulticodesAsNested As Boolean, ByVal multiValueDelimiter As Char, ByVal nestedValueDelimiter As Char, ByVal textPrecedenceAvfIds As Int32(), ByVal productionId As Int32) As Object()
			Dim retval As Object() = MakeCallAttemptReLogin(Function() MyBase.RetrieveResultsBlockForProduction(appID, runId, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds, productionId))
			If Not retval Is Nothing Then
				For Each row As Object() In retval
					If row Is Nothing Then
						Throw New System.Exception("Invalid (null) row retrieved from server")
					End If
					For i As Int32 = 0 To row.Length - 1
						If TypeOf row(i) Is Byte() Then row(i) = System.Text.Encoding.Unicode.GetString(DirectCast(row(i), Byte()))
					Next
				Next
			End If
			Return retval
		End Function

		Public Shadows Function HasExportPermissions(appID As Int32) As Boolean
			Return MakeCallAttemptReLogin(Function() MyBase.HasExportPermissions(appID))
		End Function

		Private Sub UnpackHandledException(ByVal ex As System.Exception)
			Dim soapEx As System.Web.Services.Protocols.SoapException = TryCast(ex, System.Web.Services.Protocols.SoapException)
			If soapEx Is Nothing Then Return
			Dim x As System.Exception = Nothing
			Try
				If soapEx.Detail.SelectNodes("ExceptionType").Item(0).InnerText = "Relativity.Core.Exception.InsufficientAccessControlListPermissions" Then
					x = New InsufficientPermissionsForExportException(soapEx.Detail.SelectNodes("ExceptionMessage")(0).InnerText, ex)
				End If
			Catch
			End Try
			If Not x Is Nothing Then Throw x
		End Sub

		Public Class InsufficientPermissionsForExportException
			Inherits System.Exception

			Public Sub New(message As String)
				MyBase.New(message)
			End Sub
			Public Sub New(message As String, ex As System.Exception)
				MyBase.New(message, ex)
			End Sub
		End Class

	End Class
End Namespace

