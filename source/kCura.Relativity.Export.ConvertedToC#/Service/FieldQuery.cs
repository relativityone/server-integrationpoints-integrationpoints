using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace kCura.Relativity.Export.Service
{
	public class FieldQuery : kCura.EDDS.WebAPI.FieldQueryBase.FieldQuery
	{

		public FieldQuery(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			this.Url = string.Format("{0}FieldQuery.asmx", kCura.Relativity.Export.Config.WebServiceURL);
			this.Timeout = Settings.DefaultTimeOut;
		}

		protected override System.Net.WebRequest GetWebRequest(System.Uri uri)
		{
			System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)base.GetWebRequest(uri);
			wr.UnsafeAuthenticatedConnectionSharing = true;
			wr.Credentials = this.Credentials;
			return wr;
		}

		//Public Function RetrieveAllAsArray(ByVal caseContextArtifactID As Int32, ByVal artifactTypeID As Int32, Optional ByVal includeUnmappable As Boolean = False) As kCura.EDDS.WebAPI.DocumentManagerBase.Field()
		//	Dim dv As New kCura.Data.DataView(RetrieveAllMappable(caseContextArtifactID, artifactTypeID))
		//	Dim fields As New System.Collections.ArrayList
		//	Dim field As kCura.EDDS.WebAPI.DocumentManagerBase.Field
		//	Dim unmappableFields As New System.Collections.Specialized.StringCollection
		//	Dim unmappableFieldCategories As New System.Collections.ArrayList
		//	If Not includeUnmappable Then
		//		unmappableFieldCategories.AddRange(New Relativity.FieldCategory() {Relativity.FieldCategory.Reflected, Relativity.FieldCategory.Batch, Relativity.FieldCategory.MultiReflected, Relativity.FieldCategory.FileInfo, Relativity.FieldCategory.AutoCreate, Relativity.FieldCategory.FileSize, Relativity.FieldCategory.ProductionMarker, Relativity.FieldCategory.MarkupSetMarker})
		//	End If
		//	Dim i As Int32
		//	For i = 0 To dv.Count - 1
		//		field = New kCura.EDDS.WebAPI.DocumentManagerBase.Field
		//		If Not ( _
		//		 unmappableFieldCategories.Contains(CType(dv(i)("FieldCategoryID"), Relativity.FieldCategory)) _
		//		 OrElse _
		//		 unmappableFields.Contains(dv(i)("DisplayName").ToString) _
		//		) Then
		//			If Not (CType(dv(i)("FieldCategoryID"), Relativity.FieldCategory) = Relativity.FieldCategory.FullText AndAlso artifactTypeID <> Relativity.ArtifactType.Document) Then
		//				Dim guidsString As String = dv(i)("ArtifactGuids").ToString()
		//				Dim guids As New List(Of Guid)
		//				If (Not String.IsNullOrEmpty(guidsString)) Then
		//					Dim guidStringArray As String() = guidsString.Split(CChar(";"))
		//					For Each guidString As String In guidStringArray
		//						guids.Add(New Guid(guidString.Trim()))
		//					Next
		//				End If
		//				With field
		//					.ArtifactID = CType(dv(i)("ArtifactID"), Int32)
		//					.ArtifactViewFieldID = CType(dv(i)("ArtifactViewFieldID"), Int32)
		//					.CodeTypeID = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable(Of Int32)(dv(i)("CodeTypeID"))
		//					.DisplayName = CType(dv(i)("DisplayName"), String)
		//					.FieldCategoryID = CType(dv(i)("FieldCategoryID"), Int32)
		//					.FieldCategory = CType(dv(i)("FieldCategoryID"), kCura.EDDS.WebAPI.DocumentManagerBase.FieldCategory)
		//					.FieldType = CType(System.Enum.Parse(GetType(kCura.EDDS.WebAPI.DocumentManagerBase.FieldType), CType(dv(i)("FieldTypeID"), Relativity.FieldTypeHelper.FieldType).ToString), kCura.EDDS.WebAPI.DocumentManagerBase.FieldType)
		//					.FieldTypeID = CInt(dv(i)("FieldTypeID"))
		//					.IsEditable = CType(dv(i)("IsEditable"), Boolean)
		//					.IsRequired = CType(dv(i)("IsRequired"), Boolean)
		//					.MaxLength = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable(Of Int32)(dv(i)("FieldLength"))
		//					.IsRemovable = CType(dv(i)("IsRemovable"), Boolean)
		//					.IsVisible = CType(dv(i)("IsVisible"), Boolean)
		//					.UseUnicodeEncoding = CType(dv(i)("UseUnicodeEncoding"), Boolean)
		//					.AllowHtml = CType(dv(i)("AllowHTML"), Boolean)
		//					.AssociativeArtifactTypeID = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable(Of Int32)(dv(i)("AssociativeArtifactTypeID"))
		//					.ImportBehavior = Me.ConvertImportBehaviorEnum(kCura.Utility.NullableTypesHelper.DBNullConvertToNullable(Of Int32)(dv(i)("ImportBehavior")))
		//					.EnableDataGrid = CBool(dv(i)("EnableDataGrid"))
		//					.Guids = guids.ToArray()
		//				End With
		//				If field.FieldType = EDDS.WebAPI.DocumentManagerBase.FieldType.Object OrElse field.FieldType = EDDS.WebAPI.DocumentManagerBase.FieldType.Objects OrElse field.FieldCategory = EDDS.WebAPI.DocumentManagerBase.FieldCategory.MultiReflected OrElse field.FieldCategory = EDDS.WebAPI.DocumentManagerBase.FieldCategory.Reflected Then
		//					If field.AssociativeArtifactTypeID.HasValue AndAlso Relativity.ArtifactTypeHelper.IsDynamic(field.AssociativeArtifactTypeID.Value) Then fields.Add(field)
		//				Else
		//					fields.Add(field)
		//				End If
		//			End If
		//		End If

		//	Next
		//	Return DirectCast(fields.ToArray(GetType(kCura.EDDS.WebAPI.DocumentManagerBase.Field)), kCura.EDDS.WebAPI.DocumentManagerBase.Field())
		//End Function

		//Private Function ConvertImportBehaviorEnum(ByVal input As Int32?) As kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice?
		//	If Not input.HasValue Then Return Nothing
		//	Dim ibc As Relativity.FieldInfo.ImportBehaviorChoice = CType(input, Relativity.FieldInfo.ImportBehaviorChoice)
		//	Return CType(System.Enum.Parse(GetType(kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice), ibc.ToString), kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice)
		//End Function

		//Public Function RetrieveAllAsDocumentFieldCollection(ByVal caseContextArtifactID As Int32, ByVal artifactTypeID As Int32) As DocumentFieldCollection
		//	Dim retval As New DocumentFieldCollection
		//	For Each fieldDTO As kCura.EDDS.WebAPI.DocumentManagerBase.Field In Me.RetrieveAllAsArray(caseContextArtifactID, artifactTypeID)
		//		With (fieldDTO)
		//			retval.Add(New DocumentField(.DisplayName, .ArtifactID, .FieldTypeID, .FieldCategoryID, .CodeTypeID, .MaxLength, .AssociativeArtifactTypeID, .UseUnicodeEncoding, .ImportBehavior, .Guids, .EnableDataGrid))
		//		End With
		//	Next
		//	Return retval
		//End Function

		#region " Shadow Functions "
		public new string RetrieveDisplayFieldNameByFieldCategoryID(Int32 caseContextArtifactID, Int32 fieldCategoryID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.RetrieveDisplayFieldNameByFieldCategoryID(caseContextArtifactID, fieldCategoryID);
					} else {
						//Return CType(_fieldQuery.RetrieveByFieldCategoryID(_identity, fieldCategoryID, contextArtifactID)("DisplayName"), String)
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

		public new System.Data.DataSet RetrieveAllMappable(Int32 caseContextArtifactID, Int32 artifactTypeID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveAllMappable(caseContextArtifactID, artifactTypeID);
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

		public new System.Data.DataSet RetrieveAll(Int32 caseContextArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.RetrieveAll(caseContextArtifactID);
					} else {
						//Return _fieldQuery.RetrieveAllWithSecurity(_identity, caseID).ToDataSet
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

		public new System.Data.DataSet RetrievePotentialBeginBatesFields(Int32 caseContextArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.RetrievePotentialBeginBatesFields(caseContextArtifactID);
					} else {
						//Return _fieldQuery.RetrieveAllWithSecurity(_identity, caseID).ToDataSet
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

		public new bool? IsFieldIndexed(Int32 caseContextArtifactID, Int32 fieldArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.IsFieldIndexed(caseContextArtifactID, fieldArtifactID);
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
