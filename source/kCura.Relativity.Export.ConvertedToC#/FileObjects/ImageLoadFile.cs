using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using Relativity;

namespace kCura.Relativity.Export.FileObjects
{
	[Serializable()]
	public class ImageLoadFile : System.Runtime.Serialization.ISerializable
	{

		[NonSerialized()]
		public CaseInfo CaseInfo;
		public Int32 DestinationFolderID;
		public string FileName;
		public string Overwrite;
		public string ControlKeyField;
		public bool ReplaceFullText;
		public bool ForProduction;
		public bool AutoNumberImages;
		public System.Data.DataTable ProductionTable;
		public Int32 ProductionArtifactID;
		public Int32 BeginBatesFieldArtifactID;
		public System.Text.Encoding FullTextEncoding;
		public Int64 StartLineNumber;
		public Int32 IdentityFieldId = -1;
		public bool SendEmailOnLoadCompletion;
		[NonSerialized()]
		public string SelectedCasePath = "";
		[NonSerialized()]
		public string CaseDefaultPath = "";
		[NonSerialized()]
		public bool CopyFilesToDocumentRepository = true;
		[NonSerialized()]
		public System.Net.NetworkCredential Credential;
		[NonSerialized()]
		public System.Net.CookieContainer CookieContainer;
		//<NonSerialized()> Public Identity As Relativity.Core.EDDSIdentity

		public ImageLoadFile() : base()
		{
			//Public Sub New(ByVal identity As Relativity.Core.EDDSIdentity)
			Overwrite = "None";
			ProductionArtifactID = 0;
			//Me.Identity = identity
		}

		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			//info.AddValue("CaseInfo", Me.CaseInfo, CaseInfo.GetType)
			info.AddValue("DestinationFolderID", this.DestinationFolderID, typeof(int));
			info.AddValue("FileName", this.FileName, typeof(string));
			info.AddValue("Overwrite", this.Overwrite, typeof(string));
			info.AddValue("ControlKeyField", this.ControlKeyField, typeof(string));
			info.AddValue("ReplaceFullText", this.ReplaceFullText, typeof(bool));
			info.AddValue("ForProduction", this.ForProduction, typeof(bool));
			info.AddValue("AutoNumberImages", this.AutoNumberImages, typeof(bool));
			info.AddValue("ProductionTable", this.ProductionTable, typeof(System.Data.DataTable));
			info.AddValue("ProductionArtifactID", this.ProductionArtifactID, typeof(int));
			info.AddValue("BeginBatesFieldArtifactID", this.BeginBatesFieldArtifactID, typeof(int));
			if (this.FullTextEncoding == null) {
				info.AddValue("FullTextEncoding", null, typeof(System.Text.Encoding));
			} else {
				info.AddValue("FullTextEncoding", this.FullTextEncoding, typeof(System.Text.Encoding));
			}
			info.AddValue("StartLineNumber", this.StartLineNumber, typeof(Int64));
			info.AddValue("IdentityFieldId", this.IdentityFieldId, typeof(Int32));
		}

		private ImageLoadFile(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext Context)
		{
			var _with1 = info;
			this.DestinationFolderID = info.GetInt32("DestinationFolderID");
			this.FileName = info.GetString("FileName");
			this.Overwrite = info.GetString("Overwrite");
			this.ControlKeyField = info.GetString("ControlKeyField");
			this.ReplaceFullText = info.GetBoolean("ReplaceFullText");
			this.ForProduction = info.GetBoolean("ForProduction");
			this.AutoNumberImages = info.GetBoolean("AutoNumberImages");
			this.ProductionTable = (System.Data.DataTable)info.GetValue("ProductionTable", typeof(System.Data.DataTable));
			this.BeginBatesFieldArtifactID = info.GetInt32("BeginBatesFieldArtifactID");
			this.StartLineNumber = info.GetInt64("StartLineNumber");
			try {
				this.FullTextEncoding = (System.Text.Encoding)info.GetValue("FullTextEncoding", typeof(System.Text.Encoding));
			} catch (Exception ex) {
				this.FullTextEncoding = null;
			}
			try {
				this.IdentityFieldId = info.GetInt32("IdentityFieldId");
			} catch {
				this.IdentityFieldId = -1;
			}
			try {
				this.SendEmailOnLoadCompletion = info.GetBoolean("SendEmailOnLoadCompletion");
			} catch {
				this.SendEmailOnLoadCompletion = Settings.SendEmailOnLoadCompletion;
			}
		}

	}
}
