Imports Relativity

Namespace kCura.Relativity.Export.FileObjects
	<Serializable()> Public Class ImageLoadFile
		Implements System.Runtime.Serialization.ISerializable

		<NonSerialized()> Public CaseInfo As CaseInfo
		Public DestinationFolderID As Int32
		Public FileName As String
		Public Overwrite As String
		Public ControlKeyField As String
		Public ReplaceFullText As Boolean
		Public ForProduction As Boolean
		Public AutoNumberImages As Boolean
		Public ProductionTable As System.Data.DataTable
		Public ProductionArtifactID As Int32
		Public BeginBatesFieldArtifactID As Int32
		Public FullTextEncoding As System.Text.Encoding
		Public StartLineNumber As Int64
		Public IdentityFieldId As Int32 = -1
		Public SendEmailOnLoadCompletion As Boolean
		<NonSerialized()> Public SelectedCasePath As String = ""
		<NonSerialized()> Public CaseDefaultPath As String = ""
		<NonSerialized()> Public CopyFilesToDocumentRepository As Boolean = True
		<NonSerialized()> Public Credential As Net.NetworkCredential
		<NonSerialized()> Public CookieContainer As System.Net.CookieContainer
		'<NonSerialized()> Public Identity As Relativity.Core.EDDSIdentity

		Public Sub New()
			'Public Sub New(ByVal identity As Relativity.Core.EDDSIdentity)
			MyBase.New()
			Overwrite = "None"
			ProductionArtifactID = 0
			'Me.Identity = identity
		End Sub

		Public Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext) Implements System.Runtime.Serialization.ISerializable.GetObjectData
			'info.AddValue("CaseInfo", Me.CaseInfo, CaseInfo.GetType)
			info.AddValue("DestinationFolderID", Me.DestinationFolderID, GetType(Integer))
			info.AddValue("FileName", Me.FileName, GetType(String))
			info.AddValue("Overwrite", Me.Overwrite, GetType(String))
			info.AddValue("ControlKeyField", Me.ControlKeyField, GetType(String))
			info.AddValue("ReplaceFullText", Me.ReplaceFullText, GetType(Boolean))
			info.AddValue("ForProduction", Me.ForProduction, GetType(Boolean))
			info.AddValue("AutoNumberImages", Me.AutoNumberImages, GetType(Boolean))
			info.AddValue("ProductionTable", Me.ProductionTable, GetType(System.Data.DataTable))
			info.AddValue("ProductionArtifactID", Me.ProductionArtifactID, GetType(Integer))
			info.AddValue("BeginBatesFieldArtifactID", Me.BeginBatesFieldArtifactID, GetType(Integer))
			If Me.FullTextEncoding Is Nothing Then
				info.AddValue("FullTextEncoding", Nothing, GetType(System.Text.Encoding))
			Else
				info.AddValue("FullTextEncoding", Me.FullTextEncoding, GetType(System.Text.Encoding))
			End If
			info.AddValue("StartLineNumber", Me.StartLineNumber, GetType(Int64))
			info.AddValue("IdentityFieldId", Me.IdentityFieldId, GetType(Int32))
		End Sub

		Private Sub New(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal Context As System.Runtime.Serialization.StreamingContext)
			With info
				Me.DestinationFolderID = info.GetInt32("DestinationFolderID")
				Me.FileName = info.GetString("FileName")
				Me.Overwrite = info.GetString("Overwrite")
				Me.ControlKeyField = info.GetString("ControlKeyField")
				Me.ReplaceFullText = info.GetBoolean("ReplaceFullText")
				Me.ForProduction = info.GetBoolean("ForProduction")
				Me.AutoNumberImages = info.GetBoolean("AutoNumberImages")
				Me.ProductionTable = DirectCast(info.GetValue("ProductionTable", GetType(System.Data.DataTable)), System.Data.DataTable)
				Me.BeginBatesFieldArtifactID = info.GetInt32("BeginBatesFieldArtifactID")
				Me.StartLineNumber = info.GetInt64("StartLineNumber")
				Try
					Me.FullTextEncoding = DirectCast(info.GetValue("FullTextEncoding", GetType(System.Text.Encoding)), System.Text.Encoding)
				Catch ex As Exception
					Me.FullTextEncoding = Nothing
				End Try
				Try
					Me.IdentityFieldId = info.GetInt32("IdentityFieldId")
				Catch
					Me.IdentityFieldId = -1
				End Try
				Try
					Me.SendEmailOnLoadCompletion = info.GetBoolean("SendEmailOnLoadCompletion")
				Catch
					Me.SendEmailOnLoadCompletion = Settings.SendEmailOnLoadCompletion
				End Try
			End With
		End Sub

	End Class
End Namespace