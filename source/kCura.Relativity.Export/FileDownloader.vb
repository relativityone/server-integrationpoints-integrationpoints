Imports Relativity

Namespace kCura.Relativity.Export

	Public Class FileDownloader

		Public Enum FileAccessType
			Web
			Direct
		End Enum

		Private _gateway As Service.FileIO
		Private _credentials As Net.NetworkCredential
		Private _type As FileAccessType
		Private _destinationFolderPath As String
		Private _downloadUrl As String
		Private _cookieContainer As System.Net.CookieContainer
		Private _authenticationToken As String
		'Private _userManager As kCura.WinEDDS.Service.UserManager
		Private _isBcpEnabled As Boolean = True
		Private Shared _locationAccessMatrix As New System.Collections.Hashtable

		Public Sub SetDesintationFolderName(ByVal value As String)
			_destinationFolderPath = value
		End Sub

		Public Sub New(ByVal credentials As Net.NetworkCredential, ByVal destinationFolderPath As String, ByVal downloadHandlerUrl As String, ByVal cookieContainer As System.Net.CookieContainer, ByVal authenticationToken As String)
			_gateway = New Service.FileIO(credentials, cookieContainer)

			_cookieContainer = cookieContainer
			_gateway.Credentials = credentials
			_gateway.Timeout = Int32.MaxValue
			_credentials = credentials
			If destinationFolderPath.Chars(destinationFolderPath.Length - 1) <> "\"c Then
				destinationFolderPath &= "\"
			End If
			_destinationFolderPath = destinationFolderPath
			_downloadUrl = kCura.Utility.URI.GetFullyQualifiedPath(downloadHandlerUrl, New System.Uri(Config.WebServiceURL))
			SetType(_destinationFolderPath)
			_authenticationToken = authenticationToken
			'_userManager = New kCura.WinEDDS.Service.UserManager(credentials, cookieContainer)

			If _locationAccessMatrix Is Nothing Then _locationAccessMatrix = New System.Collections.Hashtable
		End Sub

		Private Sub SetType(ByVal destFolderPath As String)
			Try
				Dim dummyText As String = System.Guid.NewGuid().ToString().Replace("-", String.Empty).Substring(0, 5)
				System.IO.File.Create(destFolderPath & dummyText).Close()
				System.IO.File.Delete(destFolderPath & dummyText)
				Me.UploaderType = FileAccessType.Direct
			Catch ex As System.Exception
				Me.UploaderType = FileAccessType.Web
			End Try
		End Sub

		Public Property DestinationFolderPath() As String
			Get
				Return _destinationFolderPath
			End Get
			Set(ByVal value As String)
				_destinationFolderPath = value
			End Set
		End Property

		Public Property UploaderType() As FileAccessType
			Get
				Return _type
			End Get
			Set(ByVal value As FileAccessType)
				Dim doevent As Boolean = _type <> value
				_type = value
				If doevent Then RaiseEvent UploadModeChangeEvent(value.ToString)
			End Set
		End Property

		Private ReadOnly Property Gateway() As Service.FileIO
			Get
				Return _gateway
			End Get
		End Property

		Friend Class Settings

			Friend Shared ReadOnly Property ChunkSize() As Int32
				Get
					Return 1024000
				End Get
			End Property
		End Class

		'Public Function DownloadFile(ByVal filePath As String, ByVal fileGuid As String) As String
		'	Return UploadFile(filePath, contextArtifactID, System.Guid.NewGuid.ToString)
		'End Function

		Public Function DownloadFullTextFile(ByVal localFilePath As String, ByVal artifactID As Int32, ByVal appID As String) As Boolean
			Return WebDownloadFile(localFilePath, artifactID, "", appID, Nothing, True, -1, -1, -1)
		End Function

		Public Function DownloadLongTextFile(ByVal localFilePath As String, ByVal artifactID As Int32, ByVal field As Types.ViewFieldInfo, ByVal appId As String) As Boolean
			Return WebDownloadFile(localFilePath, artifactID, "", appId, Nothing, False, field.FieldArtifactId, -1, -1)
		End Function

		Private Function DownloadFile(ByVal localFilePath As String, ByVal remoteFileGuid As String, ByVal remoteLocation As String, ByVal artifactID As Int32, ByVal appID As String, ByVal fileFieldArtifactID As Int32, ByVal fileID As Int32) As Boolean
			'If Me.UploaderType = Type.Web Then
			If remoteLocation.Length > 7 Then
				If remoteLocation.Substring(0, 7).ToLower = "file://" Then
					remoteLocation = remoteLocation.Substring(7)
				End If
			End If
			Dim remoteLocationKey As String = remoteLocation.Substring(0, remoteLocation.LastIndexOf("\")).TrimEnd("\"c) & "\"
			If _locationAccessMatrix.Contains(remoteLocationKey) Then
				Select Case CType(_locationAccessMatrix(remoteLocationKey), FileAccessType)
					Case FileAccessType.Direct
						Me.UploaderType = FileAccessType.Direct
						System.IO.File.Copy(remoteLocation, localFilePath, True)
						Return True
					Case FileAccessType.Web
						Me.UploaderType = FileAccessType.Web
						Return WebDownloadFile(localFilePath, artifactID, remoteFileGuid, appID, Nothing, False, -1, fileID, fileFieldArtifactID)
				End Select
			Else
				Try
					System.IO.File.Copy(remoteLocation, localFilePath, True)
					_locationAccessMatrix.Add(remoteLocationKey, FileAccessType.Direct)
					Return True
				Catch ex As Exception
					Return Me.WebDownloadFile(localFilePath, artifactID, remoteFileGuid, appID, remoteLocationKey, False, -1, fileID, fileFieldArtifactID)
				End Try
			End If
			Return Nothing
		End Function

		Public Function DownloadFileForDocument(ByVal localFilePath As String, ByVal remoteFileGuid As String, ByVal remoteLocation As String, ByVal artifactID As Int32, ByVal appID As String) As Boolean
			Return Me.DownloadFile(localFilePath, remoteFileGuid, remoteLocation, artifactID, appID, -1, -1)
		End Function

		Public Function DownloadFileForDynamicObject(ByVal localFilePath As String, ByVal remoteLocation As String, ByVal artifactID As Int32, ByVal appID As String, ByVal fileID As Int32, ByVal fileFieldArtifactID As Int32) As Boolean
			Return Me.DownloadFile(localFilePath, Nothing, remoteLocation, artifactID, appID, fileFieldArtifactID, fileID)
		End Function

		Public Function DownloadTempFile(ByVal localFilePath As String, ByVal remoteFileGuid As String, ByVal appID As String) As Boolean
			Me.UploaderType = FileAccessType.Web
			Return WebDownloadFile(localFilePath, -1, remoteFileGuid, appID, Nothing, False, -1, -1, -1)
		End Function

		Public Function MoveTempFileToLocal(ByVal localFilePath As String, ByVal remoteFileGuid As String, ByVal caseInfo As CaseInfo) As Boolean
			Return MoveTempFileToLocal(localFilePath, remoteFileGuid, caseInfo, True)
		End Function

		Public Function MoveTempFileToLocal(ByVal localFilePath As String, ByVal remoteFileGuid As String, ByVal caseInfo As CaseInfo, ByVal removeRemoteTempFile As Boolean) As Boolean
			Dim retval As Boolean = Me.DownloadTempFile(localFilePath, remoteFileGuid, caseInfo.ArtifactID.ToString)

			If removeRemoteTempFile Then
				_gateway.RemoveTempFile(caseInfo.ArtifactID, remoteFileGuid)
			End If

			Return retval
		End Function

		Public Sub RemoveRemoteTempFile(ByVal remoteFileGuid As String, ByVal caseInfo As CaseInfo)
			_gateway.RemoveTempFile(caseInfo.ArtifactID, remoteFileGuid)
		End Sub

		Public Shared TotalWebTime As Long = 0

		Private Function WebDownloadFile(ByVal localFilePath As String, ByVal artifactID As Int32, ByVal remoteFileGuid As String, ByVal appID As String, ByVal remotelocationkey As String, ByVal forFullText As Boolean, ByVal longTextFieldArtifactID As Int32, ByVal fileID As Int32, ByVal fileFieldArtifactID As Int32) As Boolean
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				Try
					Return DoWebDownloadFile(localFilePath, artifactID, remoteFileGuid, appID, remotelocationkey, forFullText, longTextFieldArtifactID, fileID, fileFieldArtifactID)
				Catch ex As DistributedReLoginException
					tries += 1
					RaiseEvent UploadStatusEvent(String.Format("Download Manager credentials failed.  Attempting to re-login ({0} of {1})", tries, Config.MaxReloginTries))
					'_userManager.AttemptReLogin()
					_authenticationToken = Export.Settings.AuthenticationToken
				End Try
			End While
			RaiseEvent UploadStatusEvent("Error Downloading File")
			Throw New ApplicationException("Error Downloading File: Unable to authenticate against Distributed server" & vbNewLine, New DistributedReLoginException)
			Return False
		End Function

		Private Function DoWebDownloadFile(ByVal localFilePath As String, ByVal artifactID As Int32, ByVal remoteFileGuid As String, ByVal appID As String, ByVal remotelocationkey As String, ByVal forFullText As Boolean, ByVal longTextFieldArtifactID As Int32, ByVal fileID As Int32, ByVal fileFieldArtifactID As Int32) As Boolean
			Dim now As Long = System.DateTime.Now.Ticks
			Dim tryNumber As Int32 = 0
			Dim localStream As System.IO.Stream = Nothing
			Try
				Dim remoteuri As String
				Dim downloadUrl As String = _downloadUrl.TrimEnd("/"c) & "/"
				If forFullText Then
					remoteuri = String.Format("{0}Download.aspx?ArtifactID={1}&AppID={2}&ExtractedText=True", downloadUrl, artifactID, appID)
				ElseIf longTextFieldArtifactID > 0 Then
					remoteuri = String.Format("{0}Download.aspx?ArtifactID={1}&AppID={2}&LongTextFieldArtifactID={3}", downloadUrl, artifactID, appID, longTextFieldArtifactID)
				ElseIf fileFieldArtifactID > 0 Then
					remoteuri = String.Format("{0}Download.aspx?ObjectArtifactID={1}&FileID={2}&AppID={3}&FileFieldArtifactID={4}", downloadUrl, artifactID, fileID, appID, fileFieldArtifactID)
				Else
					remoteuri = String.Format("{0}Download.aspx?ArtifactID={1}&GUID={2}&AppID={3}", downloadUrl, artifactID, remoteFileGuid, appID)
				End If
				If Export.Settings.AuthenticationToken <> String.Empty Then
					remoteuri &= String.Format("&AuthenticationToken={0}", Export.Settings.AuthenticationToken)
				End If
				Dim httpWebRequest As System.Net.HttpWebRequest = CType(System.Net.HttpWebRequest.Create(remoteuri), System.Net.HttpWebRequest)
				httpWebRequest.Credentials = _credentials
				httpWebRequest.CookieContainer = _cookieContainer
				httpWebRequest.UnsafeAuthenticatedConnectionSharing = True
				Dim webResponse As System.Net.HttpWebResponse = DirectCast(httpWebRequest.GetResponse(), System.Net.HttpWebResponse)
				Dim length As Int64 = 0
				If Not webResponse Is Nothing Then
					length = System.Math.Max(webResponse.ContentLength, 0)
					Dim responseStream As System.IO.Stream = webResponse.GetResponseStream()
					Try
						localStream = System.IO.File.Create(localFilePath)
					Catch ex As Exception
						localStream = System.IO.File.Create(localFilePath)
					End Try
					Dim buffer(Config.WebBasedFileDownloadChunkSize - 1) As Byte
					Dim bytesRead As Int32
					While True
						bytesRead = responseStream.Read(buffer, 0, Config.WebBasedFileDownloadChunkSize)
						If bytesRead <= 0 Then
							Exit While
						End If
						localStream.Write(buffer, 0, bytesRead)
					End While
				End If
				localStream.Close()
				Dim actualLength As Int64 = New System.IO.FileInfo(localFilePath).Length
				If length <> actualLength AndAlso length > 0 Then
					Throw New kCura.Relativity.Export.Exceptions.WebDownloadCorruptException("Error retrieving data from distributed server; expecting " & length & " bytes and received " & actualLength)
				End If
				If Not remotelocationkey Is Nothing Then _locationAccessMatrix.Add(remotelocationkey, FileAccessType.Web)
				TotalWebTime += System.DateTime.Now.Ticks - Now
				Return True
			Catch ex As DistributedReLoginException
				Me.CloseStream(localStream)
				Throw
			Catch ex As System.Net.WebException
				Me.CloseStream(localStream)
				If TypeOf ex.Response Is System.Net.HttpWebResponse Then
					Dim r As System.Net.HttpWebResponse = DirectCast(ex.Response, System.Net.HttpWebResponse)
					If r.StatusCode = Net.HttpStatusCode.Forbidden AndAlso r.StatusDescription.ToLower = "kcuraaccessdeniedmarker" Then
						Throw New DistributedReLoginException
					End If
				End If
				If ex.Message.IndexOf("409") <> -1 Then
					RaiseEvent UploadStatusEvent("Error Downloading File")					'TODO: Change this to a separate error-type event'
					Throw New ApplicationException("Error Downloading File: the file associated with the guid " & remoteFileGuid & " cannot be found" & vbNewLine, ex)
				Else
					RaiseEvent UploadStatusEvent("Error Downloading File")					'TODO: Change this to a separate error-type event'
					Throw New ApplicationException("Error Downloading File:", ex)
				End If
			Catch ex As System.Exception
				Me.CloseStream(localStream)
				RaiseEvent UploadStatusEvent("Error Downloading File")				 'TODO: Change this to a separate error-type event'
				Throw New ApplicationException("Error Downloading File", ex)
			End Try
		End Function

		Public Class DistributedReLoginException
			Inherits System.Exception
		End Class

		Private Sub CloseStream(ByVal stream As System.IO.Stream)
			If stream Is Nothing Then Exit Sub
			Try
				stream.Close()
			Catch
			End Try
		End Sub

		Public Event UploadStatusEvent(ByVal message As String)
		Public Event UploadModeChangeEvent(ByVal mode As String)

	End Class
End Namespace
