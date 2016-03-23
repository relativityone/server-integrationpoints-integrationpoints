using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Service
{
	public class FieldManager : EDDS.WebAPI.FieldManagerBase.FieldManager
	{

		private FieldQuery _query;
		public FieldQuery Query {
			get { return _query; }
		}

		protected override System.Net.WebRequest GetWebRequest(System.Uri uri)
		{
			System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)base.GetWebRequest(uri);
			wr.UnsafeAuthenticatedConnectionSharing = true;
			wr.Credentials = this.Credentials;
			return wr;
		}

		public FieldManager(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			_query = new FieldQuery(credentials, this.CookieContainer);
			this.Url = string.Format("{0}FieldManager.asmx", Config.WebServiceURL);
			this.Timeout = Settings.DefaultTimeOut;
		}


		#region " Translations "
		//Public Shared Function DTOtoDocumentField(ByVal dto As kCura.EDDS.WebAPI.DocumentManagerBase.Field) As DocumentField
		//	Dim retval As New DocumentField(dto.DisplayName, dto.ArtifactID, dto.FieldTypeID, dto.FieldCategoryID, dto.CodeTypeID, dto.MaxLength, dto.AssociativeArtifactTypeID, dto.UseUnicodeEncoding, dto.ImportBehavior, dto.EnableDataGrid)
		//	If retval.FieldCategoryID = Relativity.FieldCategory.FullText Then
		//		retval.Value = System.Text.ASCIIEncoding.ASCII.GetString(DirectCast(dto.Value, Byte()))
		//	ElseIf retval.FieldTypeID = Relativity.FieldTypeHelper.FieldType.Code OrElse retval.FieldTypeID = Relativity.FieldTypeHelper.FieldType.MultiCode Then
		//		retval.Value = kCura.Utility.Array.ToCsv(DirectCast(dto.Value, Int32())).Replace(",", ";")
		//	Else
		//		retval.Value = dto.Value.ToString
		//	End If
		//	Return retval
		//End Function

		//Public Shared Function DTOsToDocumentField(ByVal dtos As kCura.EDDS.WebAPI.DocumentManagerBase.Field()) As DocumentField()
		//	Dim documentFields(dtos.Length - 1) As DocumentField
		//	Dim i As Int32
		//	For i = 0 To documentFields.Length - 1
		//		documentFields(i) = DTOtoDocumentField(dtos(i))
		//	Next
		//	Return documentFields
		//End Function

		#endregion

		#region " Shadow Functions "
		public new Int32? Create(Int32 caseContextArtifactID, kCura.EDDS.WebAPI.FieldManagerBase.Field field)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.Create(caseContextArtifactID, field);
					} else {
						//Return _fieldManager.ExternalCreate(Me.WebAPIFieldtoDTO(field), _identity)
					}
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
			return null;
		}

		public new kCura.EDDS.WebAPI.FieldManagerBase.Field Read(Int32 caseContextArtifactID, Int32 fieldArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.Read(caseContextArtifactID, fieldArtifactID);
					} else {
						//Return Me.DTOtoFieldWebAPIField(_fieldManager.Read(fieldArtifactID, _identity))
					}
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
			return null;
		}
		#endregion

	}
}
