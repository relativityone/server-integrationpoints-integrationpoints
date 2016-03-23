Imports System.Configuration
Imports System.Linq
Imports System.Collections.Generic

Namespace kCura.Relativity.Export
	Public Class Config

#Region " ConfigSettings "

		Private Shared _LoadLock As New System.Object

		Private Shared _configDictionary As System.Collections.IDictionary
		Public Shared ReadOnly Property ConfigSettings() As System.Collections.IDictionary
			Get
				' You may ask, why are there two identical if/then statements?
				' If the config dictionary is already set, we want to return it.  And 99% of the time, the if/then will handle this, not needing a synclock.  The synclock would just slow things down.

				' If the config dictionary is not set, we want to load it.  Previously it wasn't thread safe, so the synclock was added.

				' However, it is possible that one thread could start this and get into the synclock, and another thread comes in before _configDictionary is set, and tries to enter the load process.
				' This is why we want the second if/then.
				' Next, we don't set _configDictionary until the temporary dictionary is fully formed.  It was possible for multiple threads to throw an error because _configDictionary had been created, 
				' but hadn't had all its values added to it yet, thus code looking for a specific value threw a null reference exception.

				' So here you go, this is a good example of how to make loading a static collection thread safe, and still keep some performance.  - slm - 5/24/2012

				If _configDictionary Is Nothing Then
					SyncLock _LoadLock
						If _configDictionary Is Nothing Then
							Dim tempDict As System.Collections.IDictionary
							tempDict = DirectCast(System.Configuration.ConfigurationManager.GetSection("kCura.WinEDDS"), System.Collections.IDictionary)
							If tempDict Is Nothing Then tempDict = New System.Collections.Hashtable
							If Not tempDict.Contains("ImportBatchSize") Then tempDict.Add("ImportBatchSize", "1000")
							If Not tempDict.Contains("WebAPIOperationTimeout") Then tempDict.Add("WebAPIOperationTimeout", "600000")
							If Not tempDict.Contains("DynamicBatchResizingOn") Then tempDict.Add("DynamicBatchResizingOn", "True")
							If Not tempDict.Contains("MinimumBatchSize") Then tempDict.Add("MinimumBatchSize", "100")
							If Not tempDict.Contains("ImportBatchMaxVolume") Then tempDict.Add("ImportBatchMaxVolume", "10485760") '10(2^20) - don't know what 10MB standard is
							If Not tempDict.Contains("ExportBatchSize") Then tempDict.Add("ExportBatchSize", "1000")
							If Not tempDict.Contains("EnableSingleModeImport") Then tempDict.Add("EnableSingleModeImport", "False")
							If Not tempDict.Contains("CreateErrorForEmptyNativeFile") Then tempDict.Add("CreateErrorForEmptyNativeFile", "False")
							If Not tempDict.Contains("AuditLevel") Then tempDict.Add("AuditLevel", "FullAudit")
							If Not tempDict.Contains("CreateFoldersInWebAPI") Then tempDict.Add("CreateFoldersInWebAPI", "True")
							If Not tempDict.Contains("ForceWebUpload") Then tempDict.Add("ForceWebUpload", "False")
							_configDictionary = tempDict
						End If
					End SyncLock
				End If
				Return _configDictionary
			End Get
		End Property

#End Region

#Region " Unused or shouldn't be used "

		Public Shared ReadOnly Property UsesWebAPI() As Boolean
			Get
				Return True				'Return CType(ConfigSettings("UsesWebAPI"), Boolean)
			End Get
		End Property

#End Region

#Region " Constants "

		Public Shared ReadOnly Property MaxReloginTries() As Int32
			Get
				Return 4
			End Get
		End Property

		Public Shared ReadOnly Property WaitBeforeReconnect() As Int32		'Milliseconds
			Get
				Return 2000
			End Get
		End Property

		Private Const webServiceUrlKeyName As String = "WebServiceURL"

		Public Const PREVIEW_THRESHOLD As Int32 = 1000

		Public Shared ReadOnly Property FileTransferModeExplanationText(ByVal includeBulk As Boolean) As String
			Get
				Dim sb As New System.Text.StringBuilder
				sb.Append("FILE TRANSFER MODES:" & vbNewLine)
				sb.Append(" • Web • ")
				sb.Append(vbNewLine & "The document repository is accessed through the Relativity web service API.  This is the slower of the two methods, but is globally available.")
				sb.Append(vbNewLine & vbNewLine)
				sb.Append(" • Direct • ")
				sb.Append(vbNewLine)
				sb.Append("Direct mode is significantly faster than Web mode.  To use Direct mode, you must:")
				sb.Append(vbNewLine & vbNewLine)
				sb.Append(" - Have Windows rights to the document repository.")
				sb.Append(vbNewLine)
				sb.Append(" - Be logged into the document repository’s network.")
				sb.Append(vbNewLine & vbNewLine & "If you meet the above criteria, Relativity will automatically load in Direct mode.  If you are loading in Web mode and think you should have Direct mode, contact your Relativity Administrator to establish the correct rights.")
				sb.Append(vbNewLine & vbNewLine)
				If includeBulk Then
					sb.Append("SQL INSERT MODES:" & vbNewLine)
					sb.Append(" • Bulk • " & vbNewLine)
					sb.Append("The upload process has access to the SQL share on the appropriate case database.  This ensures the fastest transfer of information between the desktop client and the relativity servers.")
					sb.Append(vbNewLine & vbNewLine)
					sb.Append(" • Single •" & vbNewLine)
					sb.Append("The upload process has NO access to the SQL share on the appropriate case database.  This is a slower method of import. If the process is using single mode, contact your Relativity Database Administrator to see if a SQL share can be opened for the desired case.")
				End If
				Return sb.ToString
			End Get
		End Property

#End Region

#Region "Registry Helpers"

		Private Shared Function GetRegistryKeyValue(ByVal keyName As String) As String
			Dim regKey As Microsoft.Win32.RegistryKey = Config.GetRegistryKey(False)
			Dim value As String = CType(regKey.GetValue(keyName, ""), String)
			regKey.Close()
			Return value
		End Function

		Private Shared Function SetRegistryKeyValue(ByVal keyName As String, ByVal keyVal As String) As String
			Dim regKey As Microsoft.Win32.RegistryKey = Config.GetRegistryKey(True)
			regKey.SetValue(keyName, keyVal)
			regKey.Close()
			Return Nothing

		End Function

		Private Shared ReadOnly Property GetRegistryKey(ByVal write As Boolean) As Microsoft.Win32.RegistryKey
			Get
				Dim regKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("software\kCura\Relativity", write)
				If regKey Is Nothing Then
					Microsoft.Win32.Registry.CurrentUser.CreateSubKey("software\\kCura\\Relativity")
					regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("software\kCura\Relativity", write)
				End If
				Return regKey
			End Get
		End Property

		Private Shared Function ValidateURIFormat(ByVal returnValue As String) As String
			If Not String.IsNullOrEmpty(returnValue) AndAlso Not returnValue.Trim.EndsWith("/") Then
				returnValue = returnValue.Trim + "/"
			End If

			'NOTE: This is here for validation; an improper URI will cause this to throw an
			' exception. We set it then to 'Nothing' to avoid a warning-turned-error about
			' having an unused variable. -Phil S. 12/05/2011
			' fixed 1/24/2012 - slm - return an empty string if invalid uri format.  this will cause the 
			' rdc to pop up its dialog prompting the user to enter a valid address

			Try
				Dim uriObj As Uri = New Uri(returnValue)
				uriObj = Nothing
			Catch
				returnValue = String.Empty
			End Try

			Return returnValue
		End Function

#End Region

		Public Shared ReadOnly Property ImportBatchMaxVolume() As Int32		'Volume in bytes
			Get
				Try
					Return CType(ConfigSettings("ImportBatchMaxVolume"), Int32)
				Catch
					Return 1000000
				End Try
			End Get
		End Property

		Public Shared ReadOnly Property ImportBatchSize() As Int32		'Number of records
			Get
				Try
					Return CType(ConfigSettings("ImportBatchSize"), Int32)
				Catch ex As Exception
					Return 500
				End Try
			End Get
		End Property

		Public Shared ReadOnly Property WebAPIOperationTimeout() As Int32
			Get
				Try
					Return CType(ConfigSettings("WebAPIOperationTimeout"), Int32)
				Catch ex As Exception
					Return 600000
				End Try
			End Get
		End Property

		''' <summary>
		''' If True, Folders which are created in Append mode are created in the WebAPI.
		''' If False, Folders which are created in Append mode are created in RDC/ImportAPI.
		''' If the value is not set in the config file, True is returned.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>This property is part of the fix for Dominus# 1127879</remarks>
		Public Shared ReadOnly Property CreateFoldersInWebAPI() As Boolean
			Get
				Try
					Return CType(ConfigSettings("CreateFoldersInWebAPI"), Boolean)
				Catch ex As Exception
					Return True	' Default changed from False to True for Atlas' IAPI Folder changes Oct/Nov 2013
				End Try
			End Get
		End Property

		Public Shared ReadOnly Property DynamicBatchResizingOn() As Boolean		'Allow or not to automatically decrease import batch size while import is in progress
			Get
				Return CType(ConfigSettings("DynamicBatchResizingOn"), Boolean)
			End Get
		End Property

		Public Shared ReadOnly Property DefaultMaximumErrorCount() As Int32
			Get
				Return 1000
			End Get
		End Property

		Public Shared ReadOnly Property MinimumBatchSize() As Int32		'When AutoBatch is on. This is the lower ceiling up to which batch will decrease
			Get
				Return CType(ConfigSettings("MinimumBatchSize"), Int32)
			End Get
		End Property

		Public Shared ReadOnly Property ExportBatchSize() As Int32		'Number of records
			Get
				Return CType(ConfigSettings("ExportBatchSize"), Int32)
			End Get
		End Property

		Public Shared ReadOnly Property DisableImageTypeValidation() As Boolean
			Get
				Return CType(ConfigSettings("DisableImageTypeValidation"), Boolean)
			End Get
		End Property

		Public Shared ReadOnly Property DisableImageLocationValidation() As Boolean
			Get
				Return CType(ConfigSettings("DisableImageLocationValidation"), Boolean)
			End Get
		End Property

		Public Shared ReadOnly Property DisableNativeValidation() As Boolean
			Get
				Return CType(ConfigSettings("DisableNativeValidation"), Boolean)
			End Get
		End Property

		Public Shared ReadOnly Property DisableNativeLocationValidation() As Boolean
			Get
				Return CType(ConfigSettings("DisableNativeLocationValidation"), Boolean)
			End Get
		End Property

		Public Shared ReadOnly Property CreateErrorForEmptyNativeFile() As Boolean
			Get
				Return CType(ConfigSettings("CreateErrorForEmptyNativeFile"), Boolean)
			End Get
		End Property

		'Public Shared ReadOnly Property AuditLevel() As kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel
		'	Get
		'		Return DirectCast([Enum].Parse(GetType(kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel), CStr(ConfigSettings("AuditLevel"))), kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel)
		'	End Get
		'End Property

		'This hidden configuration setting makes it easier to test WebUpload.  Testing is important.
		Public Shared ReadOnly Property ForceWebUpload() As Boolean
			Get
				Return CType(ConfigSettings("ForceWebUpload"), Boolean)
			End Get
		End Property

		Public Shared Property ForceFolderPreview() As Boolean
			Get
				Dim registryValue As String = Config.GetRegistryKeyValue("ForceFolderPreview")
				If String.IsNullOrEmpty(registryValue) Then
					Config.SetRegistryKeyValue("ForceFolderPreview", "true")
					Return True
				End If
				Return registryValue.ToLower.Equals("true")
			End Get
			Set(ByVal value As Boolean)
				Dim registryValue As String = "false"
				If value Then registryValue = "true"
				Config.SetRegistryKeyValue("ForceFolderPreview", registryValue)
			End Set
		End Property

		Public Shared Property ObjectFieldIdListContainsArtifactId() As IList(Of Int32)
			Get
				Dim registryValue As String = Config.GetRegistryKeyValue("ObjectFieldIdListContainsArtifactId")
				If String.IsNullOrEmpty(registryValue) Then
					Config.SetRegistryKeyValue("ObjectFieldIdListContainsArtifactId", "")
					Return New List(Of Int32)
				End If
				Return registryValue.Split(New String() {","}, StringSplitOptions.RemoveEmptyEntries).Select(Function(s As String) Int32.Parse(s)).ToArray
			End Get
			Set(ByVal value As IList(Of Int32))
				Config.SetRegistryKeyValue("ObjectFieldIdListContainsArtifactId", String.Join(",", value.Select(Function(x) x.ToString()).ToArray))
			End Set
		End Property

		Public Shared Property WebServiceURL() As String
			Get
				Dim returnValue As String = Nothing

				'Programmatic ServiceURL
				If Not String.IsNullOrWhiteSpace(_programmaticServiceURL) Then
					returnValue = _programmaticServiceURL
				ElseIf ConfigSettings.Contains(webServiceUrlKeyName) Then
					'App.config ServiceURL
					Dim regUrl As String = CType(ConfigSettings(webServiceUrlKeyName), String)

					If Not String.IsNullOrWhiteSpace(regUrl) Then
						returnValue = regUrl
					End If
				End If

				'Registry ServiceURL
				If String.IsNullOrWhiteSpace(returnValue) Then
					returnValue = GetRegistryKeyValue(webServiceUrlKeyName)
				End If

				Return ValidateURIFormat(returnValue)
			End Get
			Set(ByVal value As String)
				Dim properURI As String = ValidateURIFormat(value)

				SetRegistryKeyValue(webServiceUrlKeyName, properURI)
			End Set
		End Property

		Private Shared _programmaticServiceURL As String = Nothing

		Public Shared Property ProgrammaticServiceURL() As String
			Get
				Return _programmaticServiceURL
			End Get
			Set(value As String)
				_programmaticServiceURL = value
			End Set
		End Property

		Public Shared ReadOnly Property EnableSingleModeImport() As Boolean
			Get
				Try
					Return CType(ConfigSettings("EnableSingleModeImport"), Boolean)
				Catch ex As Exception
					Return False
				End Try
			End Get
		End Property

		Public Shared ReadOnly Property WebBasedFileDownloadChunkSize() As Int32
			Get
				If Not ConfigSettings.Contains("WebBasedFileDownloadChunkSize") Then
					ConfigSettings.Add("WebBasedFileDownloadChunkSize", 1048576)
				End If
				Return System.Math.Max(CType(ConfigSettings("WebBasedFileDownloadChunkSize"), Int32), 1024)
			End Get
		End Property
	End Class
End Namespace
