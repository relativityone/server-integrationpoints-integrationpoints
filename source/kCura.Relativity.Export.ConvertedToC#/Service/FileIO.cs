using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using Relativity;

namespace kCura.Relativity.Export.Service
{

	public class FileIO : kCura.EDDS.WebAPI.FileIOBase.FileIO
	{
		private static MemoryCache BCPCache = new MemoryCache("BCPCache");
		public FileIO(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			this.Url = string.Format("{0}FileIO.asmx", Config.WebServiceURL);
			this.Timeout = Settings.DefaultTimeOut;
		}

		protected override System.Net.WebRequest GetWebRequest(System.Uri uri)
		{
			System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)base.GetWebRequest(uri);
			wr.UnsafeAuthenticatedConnectionSharing = true;
			wr.Credentials = this.Credentials;
			return wr;
		}

		#region " ExecutionWrappers "

		private bool IsRetryableException(System.Exception ex)
		{
			bool retval = false;

			if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1) {
				retval = true;

			} else if (ex.Message == "The server committed a protocol violation. Section=ResponseStatusLine") {
				//HACK: this fixes a symptom, not the cause.  I haven't yet been able to prevent the error from being thrown, as a debug stop on the server side doesn't actually halt the protocol violation from being thrown.
				retval = true;
			}
			return retval;
		}

		//Private Function ExecuteWithRetry(Of T)(f As Func(Of T)) As T
		//	Dim tries As Int32 = 0
		//	While tries < Config.MaxReloginTries
		//		tries += 1
		//		Try
		//			Return f()
		//		Catch ex As System.Exception
		//			If IsRetryableException(ex) AndAlso tries < Config.MaxReloginTries Then
		//				Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
		//			Else
		//				Dim relativityException As System.Exception = ConvertExpectedSoapExceptionToRelativityException(ex)
		//				If relativityException IsNot Nothing Then
		//					Throw relativityException
		//				End If

		//				Throw
		//			End If
		//		End Try
		//	End While
		//	Return Nothing
		//End Function

		/// <summary>
		/// Convert a SoapException to a normal typed exception which we could specify in a catch clause or test by Type.
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		/// <remarks>For example, if the user does not have permissions, the Soap exception is converted to a Relativity.Core.Exception.InsufficientAccessControlListPermissions.
		/// This method is expected to test for exceptions which we intentionally throw from WebAPI FileIO due to error conditions.</remarks>
		//Public Function ConvertExpectedSoapExceptionToRelativityException(ex As System.Exception) As System.Exception
		//	Dim soapEx As System.Web.Services.Protocols.SoapException = TryCast(ex, System.Web.Services.Protocols.SoapException)
		//	If soapEx Is Nothing Then Return Nothing

		//	Dim relativityException As System.Exception = Nothing
		//	Try
		//		If soapEx.Detail.SelectNodes("ExceptionType").Item(0).InnerText = "Relativity.Core.Exception.InsufficientAccessControlListPermissions" Then
		//			relativityException = New kCura.WinEDDS.Service.BulkImportManager.InsufficientPermissionsForImportException(soapEx.Detail.SelectNodes("ExceptionMessage")(0).InnerText)
		//		ElseIf soapEx.Detail.SelectNodes("ExceptionType").Item(0).InnerText = "System.ArgumentException" Then
		//			relativityException = New System.ArgumentException(soapEx.Detail.SelectNodes("ExceptionMessage")(0).InnerText)
		//		End If
		//	Catch
		//	End Try

		//	Return relativityException
		//End Function

		//Private Sub ExecuteWithRetry(f As Action)
		//	ExecuteWithRetry(Function()
		//												f()
		//												Return True
		//											End Function)
		//End Sub

		#endregion

		#region " Shadow Functions "
		//Public Shadows Function BeginFill(ByVal caseContextArtifactID As Int32, ByVal b() As Byte, ByVal documentDirectory As String, ByVal fileGuid As String) As kCura.EDDS.WebAPI.FileIOBase.IoResponse
		//	Return ExecuteWithRetry(Function() MyBase.BeginFill(caseContextArtifactID, b, documentDirectory, fileGuid))
		//End Function

		//Public Shadows Function FileFill(ByVal caseContextArtifactID As Int32, ByVal documentDirectory As String, ByVal fileName As String, ByVal b() As Byte, ByVal contextArtifactID As Int32) As kCura.EDDS.WebAPI.FileIOBase.IoResponse
		//	Return ExecuteWithRetry(Function() MyBase.FileFill(caseContextArtifactID, documentDirectory, fileName, b))
		//End Function

		//Public Shadows Sub RemoveFill(ByVal caseContextArtifactID As Int32, ByVal documentDirectory As String, ByVal fileName As String)
		//	ExecuteWithRetry(Sub() MyBase.RemoveFill(caseContextArtifactID, documentDirectory, fileName))
		//End Sub

		//Public Shadows Sub RemoveTempFile(ByVal caseContextArtifactID As Integer, ByVal fileName As String)
		//	ExecuteWithRetry(Sub() MyBase.RemoveTempFile(caseContextArtifactID, fileName))
		//End Sub

		//Public Shadows Function ValidateBcpShare(ByVal appID As Int32) As Boolean
		//	Return ExecuteWithRetry(Function() MyBase.ValidateBcpShare(appID))
		//End Function

		//Public Shadows Function GetBcpShareSpaceReport(ByVal appID As Int32) As String()()
		//	Return ExecuteWithRetry(Function() MyBase.GetBcpShareSpaceReport(appID))
		//End Function

		//Public Shadows Function GetDefaultRepositorySpaceReport(ByVal appID As Int32) As String()()
		//	Return ExecuteWithRetry(Function() MyBase.GetDefaultRepositorySpaceReport(appID))
		//End Function

		//Public Shadows Function RepositoryVolumeMax() As Int32
		//	Return ExecuteWithRetry(Function() MyBase.RepositoryVolumeMax())
		//End Function

		public new string GetBcpSharePath(Int32 appID)
		{
			object cacheVal = BCPCache.Get(appID.ToString());
			if ((cacheVal != null)) {
				return cacheVal.ToString();
			}
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					string retVal = base.GetBcpSharePath(appID);
					if (string.IsNullOrEmpty(retVal)) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						BCPCache.Add(appID.ToString(), retVal, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(60) });
						return retVal;
					}
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						if (ex is System.Web.Services.Protocols.SoapException) {
							throw ParseExceptionForMoreInfo(ex);
						} else {
							throw ex;
						}
					}
				}
			}
			throw new System.Exception("Unable to retrieve BCP share path from the server");
		}

		public static System.Exception ParseExceptionForMoreInfo(Exception ex)
		{
			System.Exception resultException = ex;
			SoapExceptionDetail detailedException = null;
			if (ex is System.Web.Services.Protocols.SoapException) {
				System.Web.Services.Protocols.SoapException soapException = (System.Web.Services.Protocols.SoapException)ex;
				System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(SoapExceptionDetail));
				System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
				doc.LoadXml(soapException.Detail.OuterXml);
				System.Xml.XmlReader xr = doc.CreateNavigator().ReadSubtree();
				detailedException = xs.Deserialize(xr) as SoapExceptionDetail;
			}

			if (detailedException != null) {
				resultException = new CustomException(detailedException.ExceptionMessage, ex);
			}

			return resultException;

		}


		#endregion

		public class CustomException : System.Exception
		{

			public CustomException(string message, System.Exception innerException) : base(message, innerException)
			{
			}

			public override string ToString()
			{
				return base.ToString() + Microsoft.VisualBasic.Constants.vbNewLine + base.InnerException.ToString();
			}
		}

	}
}
