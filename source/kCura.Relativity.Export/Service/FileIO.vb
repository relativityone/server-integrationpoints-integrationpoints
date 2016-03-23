Imports System.Runtime.Caching
Imports Relativity

Namespace kCura.Relativity.Export.Service

	Public Class FileIO
		Inherits kCura.EDDS.WebAPI.FileIOBase.FileIO
		Private Shared BCPCache As New MemoryCache("BCPCache")
		Public Sub New(ByVal credentials As Net.ICredentials, ByVal cookieContainer As System.Net.CookieContainer)
			MyBase.New()

			Me.Credentials = credentials
			Me.CookieContainer = cookieContainer
			Me.Url = String.Format("{0}FileIO.asmx", Config.WebServiceURL)
			Me.Timeout = Settings.DefaultTimeOut
		End Sub

		Protected Overrides Function GetWebRequest(ByVal uri As System.Uri) As System.Net.WebRequest
			Dim wr As System.Net.HttpWebRequest = DirectCast(MyBase.GetWebRequest(uri), System.Net.HttpWebRequest)
			wr.UnsafeAuthenticatedConnectionSharing = True
			wr.Credentials = Me.Credentials
			Return wr
		End Function

#Region " ExecutionWrappers "

		Private Function IsRetryableException(ex As System.Exception) As Boolean
			Dim retval As Boolean = False

			If TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("NeedToReLoginException") <> -1 Then
				retval = True

			ElseIf ex.Message = "The server committed a protocol violation. Section=ResponseStatusLine" Then
				'HACK: this fixes a symptom, not the cause.  I haven't yet been able to prevent the error from being thrown, as a debug stop on the server side doesn't actually halt the protocol violation from being thrown.
				retval = True
			End If
			Return retval
		End Function

		'Private Function ExecuteWithRetry(Of T)(f As Func(Of T)) As T
		'	Dim tries As Int32 = 0
		'	While tries < Config.MaxReloginTries
		'		tries += 1
		'		Try
		'			Return f()
		'		Catch ex As System.Exception
		'			If IsRetryableException(ex) AndAlso tries < Config.MaxReloginTries Then
		'				Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
		'			Else
		'				Dim relativityException As System.Exception = ConvertExpectedSoapExceptionToRelativityException(ex)
		'				If relativityException IsNot Nothing Then
		'					Throw relativityException
		'				End If

		'				Throw
		'			End If
		'		End Try
		'	End While
		'	Return Nothing
		'End Function

		''' <summary>
		''' Convert a SoapException to a normal typed exception which we could specify in a catch clause or test by Type.
		''' </summary>
		''' <param name="ex"></param>
		''' <returns></returns>
		''' <remarks>For example, if the user does not have permissions, the Soap exception is converted to a Relativity.Core.Exception.InsufficientAccessControlListPermissions.
		''' This method is expected to test for exceptions which we intentionally throw from WebAPI FileIO due to error conditions.</remarks>
		'Public Function ConvertExpectedSoapExceptionToRelativityException(ex As System.Exception) As System.Exception
		'	Dim soapEx As System.Web.Services.Protocols.SoapException = TryCast(ex, System.Web.Services.Protocols.SoapException)
		'	If soapEx Is Nothing Then Return Nothing

		'	Dim relativityException As System.Exception = Nothing
		'	Try
		'		If soapEx.Detail.SelectNodes("ExceptionType").Item(0).InnerText = "Relativity.Core.Exception.InsufficientAccessControlListPermissions" Then
		'			relativityException = New kCura.WinEDDS.Service.BulkImportManager.InsufficientPermissionsForImportException(soapEx.Detail.SelectNodes("ExceptionMessage")(0).InnerText)
		'		ElseIf soapEx.Detail.SelectNodes("ExceptionType").Item(0).InnerText = "System.ArgumentException" Then
		'			relativityException = New System.ArgumentException(soapEx.Detail.SelectNodes("ExceptionMessage")(0).InnerText)
		'		End If
		'	Catch
		'	End Try

		'	Return relativityException
		'End Function

		'Private Sub ExecuteWithRetry(f As Action)
		'	ExecuteWithRetry(Function()
		'												f()
		'												Return True
		'											End Function)
		'End Sub

#End Region

#Region " Shadow Functions "
		'Public Shadows Function BeginFill(ByVal caseContextArtifactID As Int32, ByVal b() As Byte, ByVal documentDirectory As String, ByVal fileGuid As String) As kCura.EDDS.WebAPI.FileIOBase.IoResponse
		'	Return ExecuteWithRetry(Function() MyBase.BeginFill(caseContextArtifactID, b, documentDirectory, fileGuid))
		'End Function

		'Public Shadows Function FileFill(ByVal caseContextArtifactID As Int32, ByVal documentDirectory As String, ByVal fileName As String, ByVal b() As Byte, ByVal contextArtifactID As Int32) As kCura.EDDS.WebAPI.FileIOBase.IoResponse
		'	Return ExecuteWithRetry(Function() MyBase.FileFill(caseContextArtifactID, documentDirectory, fileName, b))
		'End Function

		'Public Shadows Sub RemoveFill(ByVal caseContextArtifactID As Int32, ByVal documentDirectory As String, ByVal fileName As String)
		'	ExecuteWithRetry(Sub() MyBase.RemoveFill(caseContextArtifactID, documentDirectory, fileName))
		'End Sub

		'Public Shadows Sub RemoveTempFile(ByVal caseContextArtifactID As Integer, ByVal fileName As String)
		'	ExecuteWithRetry(Sub() MyBase.RemoveTempFile(caseContextArtifactID, fileName))
		'End Sub

		'Public Shadows Function ValidateBcpShare(ByVal appID As Int32) As Boolean
		'	Return ExecuteWithRetry(Function() MyBase.ValidateBcpShare(appID))
		'End Function

		'Public Shadows Function GetBcpShareSpaceReport(ByVal appID As Int32) As String()()
		'	Return ExecuteWithRetry(Function() MyBase.GetBcpShareSpaceReport(appID))
		'End Function

		'Public Shadows Function GetDefaultRepositorySpaceReport(ByVal appID As Int32) As String()()
		'	Return ExecuteWithRetry(Function() MyBase.GetDefaultRepositorySpaceReport(appID))
		'End Function

		'Public Shadows Function RepositoryVolumeMax() As Int32
		'	Return ExecuteWithRetry(Function() MyBase.RepositoryVolumeMax())
		'End Function

		Public Shadows Function GetBcpSharePath(ByVal appID As Int32) As String
			Dim cacheVal As Object = BCPCache.Get(appID.ToString())
			If (cacheVal IsNot Nothing) Then
				Return cacheVal.ToString()
			End If
			Dim tries As Int32 = 0
			While tries < Config.MaxReloginTries
				tries += 1
				Try
					Dim retVal As String = MyBase.GetBcpSharePath(appID)
					If String.IsNullOrEmpty(retVal) Then
						'Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					Else
						BCPCache.Add(appID.ToString(), retVal, New CacheItemPolicy() With {.AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(60)})
						Return retVal
					End If
				Catch ex As System.Exception
					If TypeOf ex Is System.Web.Services.Protocols.SoapException AndAlso ex.ToString.IndexOf("NeedToReLoginException") <> -1 AndAlso tries < Config.MaxReloginTries Then
						'Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					Else
						If TypeOf ex Is System.Web.Services.Protocols.SoapException Then
							Throw ParseExceptionForMoreInfo(ex)
						Else
							Throw ex
						End If
					End If
				End Try
			End While
			Throw New System.Exception("Unable to retrieve BCP share path from the server")
		End Function

		Public Shared Function ParseExceptionForMoreInfo(ByVal ex As Exception) As System.Exception
			Dim resultException As System.Exception = ex
			Dim detailedException As SoapExceptionDetail = Nothing
			If TypeOf ex Is System.Web.Services.Protocols.SoapException Then
				Dim soapException As System.Web.Services.Protocols.SoapException = DirectCast(ex, System.Web.Services.Protocols.SoapException)
				Dim xs As New Xml.Serialization.XmlSerializer(GetType(SoapExceptionDetail))
				Dim doc As New System.Xml.XmlDocument
				doc.LoadXml(soapException.Detail.OuterXml)
				Dim xr As Xml.XmlReader = doc.CreateNavigator.ReadSubtree
				detailedException = TryCast(xs.Deserialize(xr), SoapExceptionDetail)
			End If

			If detailedException IsNot Nothing Then
				resultException = New CustomException(detailedException.ExceptionMessage, ex)
			End If

			Return resultException

		End Function


#End Region

		Public Class CustomException
			Inherits System.Exception

			Public Sub New(ByVal message As String, ByVal innerException As System.Exception)
				MyBase.New(message, innerException)
			End Sub

			Public Overrides Function ToString() As String
				Return MyBase.ToString + vbNewLine + MyBase.InnerException.ToString
			End Function
		End Class

	End Class
End Namespace