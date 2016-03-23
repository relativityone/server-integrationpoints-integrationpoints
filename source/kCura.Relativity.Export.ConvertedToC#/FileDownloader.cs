using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using Relativity;

namespace kCura.Relativity.Export
{

	public class FileDownloader
	{

		public enum FileAccessType
		{
			Web,
			Direct
		}

		private Service.FileIO _gateway;
		private System.Net.NetworkCredential _credentials;
		private FileAccessType _type;
		private string _destinationFolderPath;
		private string _downloadUrl;
		private System.Net.CookieContainer _cookieContainer;
		private string _authenticationToken;
		//Private _userManager As kCura.WinEDDS.Service.UserManager
		private bool _isBcpEnabled = true;

		private static System.Collections.Hashtable _locationAccessMatrix = new System.Collections.Hashtable();
		public void SetDesintationFolderName(string value)
		{
			_destinationFolderPath = value;
		}

		public FileDownloader(System.Net.NetworkCredential credentials, string destinationFolderPath, string downloadHandlerUrl, System.Net.CookieContainer cookieContainer, string authenticationToken)
		{
			_gateway = new Service.FileIO(credentials, cookieContainer);

			_cookieContainer = cookieContainer;
			_gateway.Credentials = credentials;
			_gateway.Timeout = Int32.MaxValue;
			_credentials = credentials;
			if (destinationFolderPath[destinationFolderPath.Length - 1] != '\\') {
				destinationFolderPath += "\\";
			}
			_destinationFolderPath = destinationFolderPath;
			_downloadUrl = kCura.Utility.URI.GetFullyQualifiedPath(downloadHandlerUrl, new System.Uri(Config.WebServiceURL));
			SetType(_destinationFolderPath);
			_authenticationToken = authenticationToken;
			//_userManager = New kCura.WinEDDS.Service.UserManager(credentials, cookieContainer)

			if (_locationAccessMatrix == null)
				_locationAccessMatrix = new System.Collections.Hashtable();
		}

		private void SetType(string destFolderPath)
		{
			try {
				string dummyText = System.Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 5);
				System.IO.File.Create(destFolderPath + dummyText).Close();
				System.IO.File.Delete(destFolderPath + dummyText);
				this.UploaderType = FileAccessType.Direct;
			} catch (System.Exception ex) {
				this.UploaderType = FileAccessType.Web;
			}
		}

		public string DestinationFolderPath {
			get { return _destinationFolderPath; }
			set { _destinationFolderPath = value; }
		}

		public FileAccessType UploaderType {
			get { return _type; }
			set {
				bool doevent = _type != value;
				_type = value;
				if (doevent)
					if (UploadModeChangeEvent != null) {
						UploadModeChangeEvent(value.ToString());
					}
			}
		}

		private Service.FileIO Gateway {
			get { return _gateway; }
		}

		internal class Settings
		{

			static internal Int32 ChunkSize {
				get { return 1024000; }
			}
		}

		//Public Function DownloadFile(ByVal filePath As String, ByVal fileGuid As String) As String
		//	Return UploadFile(filePath, contextArtifactID, System.Guid.NewGuid.ToString)
		//End Function

		public bool DownloadFullTextFile(string localFilePath, Int32 artifactID, string appID)
		{
			return WebDownloadFile(localFilePath, artifactID, "", appID, null, true, -1, -1, -1);
		}

		public bool DownloadLongTextFile(string localFilePath, Int32 artifactID, Types.ViewFieldInfo field, string appId)
		{
			return WebDownloadFile(localFilePath, artifactID, "", appId, null, false, field.FieldArtifactId, -1, -1);
		}

		private bool? DownloadFile(string localFilePath, string remoteFileGuid, string remoteLocation, Int32 artifactID, string appID, Int32 fileFieldArtifactID, Int32 fileID)
		{
			//If Me.UploaderType = Type.Web Then
			if (remoteLocation.Length > 7) {
				if (remoteLocation.Substring(0, 7).ToLower() == "file://") {
					remoteLocation = remoteLocation.Substring(7);
				}
			}
			string remoteLocationKey = remoteLocation.Substring(0, remoteLocation.LastIndexOf("\\")).TrimEnd('\\') + "\\";
			if (_locationAccessMatrix.Contains(remoteLocationKey)) {
				switch ((FileAccessType)_locationAccessMatrix[remoteLocationKey]) {
					case FileAccessType.Direct:
						this.UploaderType = FileAccessType.Direct;
						System.IO.File.Copy(remoteLocation, localFilePath, true);
						return true;
					case FileAccessType.Web:
						this.UploaderType = FileAccessType.Web;
						return WebDownloadFile(localFilePath, artifactID, remoteFileGuid, appID, null, false, -1, fileID, fileFieldArtifactID);
				}
			} else {
				try {
					System.IO.File.Copy(remoteLocation, localFilePath, true);
					_locationAccessMatrix.Add(remoteLocationKey, FileAccessType.Direct);
					return true;
				} catch (Exception ex) {
					return this.WebDownloadFile(localFilePath, artifactID, remoteFileGuid, appID, remoteLocationKey, false, -1, fileID, fileFieldArtifactID);
				}
			}
			return null;
		}

		public bool? DownloadFileForDocument(string localFilePath, string remoteFileGuid, string remoteLocation, Int32 artifactID, string appID)
		{
			return this.DownloadFile(localFilePath, remoteFileGuid, remoteLocation, artifactID, appID, -1, -1);
		}

		public bool? DownloadFileForDynamicObject(string localFilePath, string remoteLocation, Int32 artifactID, string appID, Int32 fileID, Int32 fileFieldArtifactID)
		{
			return this.DownloadFile(localFilePath, null, remoteLocation, artifactID, appID, fileFieldArtifactID, fileID);
		}

		public bool DownloadTempFile(string localFilePath, string remoteFileGuid, string appID)
		{
			this.UploaderType = FileAccessType.Web;
			return WebDownloadFile(localFilePath, -1, remoteFileGuid, appID, null, false, -1, -1, -1);
		}

		public bool MoveTempFileToLocal(string localFilePath, string remoteFileGuid, CaseInfo caseInfo)
		{
			return MoveTempFileToLocal(localFilePath, remoteFileGuid, caseInfo, true);
		}

		public bool MoveTempFileToLocal(string localFilePath, string remoteFileGuid, CaseInfo caseInfo, bool removeRemoteTempFile)
		{
			bool retval = this.DownloadTempFile(localFilePath, remoteFileGuid, caseInfo.ArtifactID.ToString());

			if (removeRemoteTempFile) {
				_gateway.RemoveTempFile(caseInfo.ArtifactID, remoteFileGuid);
			}

			return retval;
		}

		public void RemoveRemoteTempFile(string remoteFileGuid, CaseInfo caseInfo)
		{
			_gateway.RemoveTempFile(caseInfo.ArtifactID, remoteFileGuid);
		}


		public static long TotalWebTime = 0;
		private bool WebDownloadFile(string localFilePath, Int32 artifactID, string remoteFileGuid, string appID, string remotelocationkey, bool forFullText, Int32 longTextFieldArtifactID, Int32 fileID, Int32 fileFieldArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				try {
					return DoWebDownloadFile(localFilePath, artifactID, remoteFileGuid, appID, remotelocationkey, forFullText, longTextFieldArtifactID, fileID, fileFieldArtifactID);
				} catch (DistributedReLoginException ex) {
					tries += 1;
					if (UploadStatusEvent != null) {
						UploadStatusEvent(string.Format("Download Manager credentials failed.  Attempting to re-login ({0} of {1})", tries, Config.MaxReloginTries));
					}
					//_userManager.AttemptReLogin()
					_authenticationToken = kCura.Relativity.Export.Settings.AuthenticationToken;
				}
			}
			if (UploadStatusEvent != null) {
				UploadStatusEvent("Error Downloading File");
			}
			throw new ApplicationException("Error Downloading File: Unable to authenticate against Distributed server" + Microsoft.VisualBasic.Constants.vbNewLine, new DistributedReLoginException());
			return false;
		}

		private bool DoWebDownloadFile(string localFilePath, Int32 artifactID, string remoteFileGuid, string appID, string remotelocationkey, bool forFullText, Int32 longTextFieldArtifactID, Int32 fileID, Int32 fileFieldArtifactID)
		{
			long now = System.DateTime.Now.Ticks;
			Int32 tryNumber = 0;
			System.IO.Stream localStream = null;
			try {
				string remoteuri = null;
				string downloadUrl = _downloadUrl.TrimEnd('/') + "/";
				if (forFullText) {
					remoteuri = string.Format("{0}Download.aspx?ArtifactID={1}&AppID={2}&ExtractedText=True", downloadUrl, artifactID, appID);
				} else if (longTextFieldArtifactID > 0) {
					remoteuri = string.Format("{0}Download.aspx?ArtifactID={1}&AppID={2}&LongTextFieldArtifactID={3}", downloadUrl, artifactID, appID, longTextFieldArtifactID);
				} else if (fileFieldArtifactID > 0) {
					remoteuri = string.Format("{0}Download.aspx?ObjectArtifactID={1}&FileID={2}&AppID={3}&FileFieldArtifactID={4}", downloadUrl, artifactID, fileID, appID, fileFieldArtifactID);
				} else {
					remoteuri = string.Format("{0}Download.aspx?ArtifactID={1}&GUID={2}&AppID={3}", downloadUrl, artifactID, remoteFileGuid, appID);
				}
				if (kCura.Relativity.Export.Settings.AuthenticationToken != string.Empty) {
					remoteuri += string.Format("&AuthenticationToken={0}", kCura.Relativity.Export.Settings.AuthenticationToken);
				}
				System.Net.HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(remoteuri);
				httpWebRequest.Credentials = _credentials;
				httpWebRequest.CookieContainer = _cookieContainer;
				httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
				System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)httpWebRequest.GetResponse();
				Int64 length = 0;
				if ((webResponse != null)) {
					length = System.Math.Max(webResponse.ContentLength, 0);
					System.IO.Stream responseStream = webResponse.GetResponseStream();
					try {
						localStream = System.IO.File.Create(localFilePath);
					} catch (Exception ex) {
						localStream = System.IO.File.Create(localFilePath);
					}
					byte[] buffer = new byte[Config.WebBasedFileDownloadChunkSize];
					Int32 bytesRead = default(Int32);
					while (true) {
						bytesRead = responseStream.Read(buffer, 0, Config.WebBasedFileDownloadChunkSize);
						if (bytesRead <= 0) {
							break; // TODO: might not be correct. Was : Exit While
						}
						localStream.Write(buffer, 0, bytesRead);
					}
				}
				localStream.Close();
				Int64 actualLength = new System.IO.FileInfo(localFilePath).Length;
				if (length != actualLength && length > 0) {
					throw new kCura.Relativity.Export.Exceptions.WebDownloadCorruptException("Error retrieving data from distributed server; expecting " + length + " bytes and received " + actualLength);
				}
				if ((remotelocationkey != null))
					_locationAccessMatrix.Add(remotelocationkey, FileAccessType.Web);
				TotalWebTime += System.DateTime.Now.Ticks - now;
				return true;
			} catch (DistributedReLoginException ex) {
				this.CloseStream(localStream);
				throw;
			} catch (System.Net.WebException ex) {
				this.CloseStream(localStream);
				if (ex.Response is System.Net.HttpWebResponse) {
					System.Net.HttpWebResponse r = (System.Net.HttpWebResponse)ex.Response;
					if (r.StatusCode == System.Net.HttpStatusCode.Forbidden && r.StatusDescription.ToLower() == "kcuraaccessdeniedmarker") {
						throw new DistributedReLoginException();
					}
				}
				if (ex.Message.IndexOf("409") != -1) {
					if (UploadStatusEvent != null) {
						UploadStatusEvent("Error Downloading File");
					}
					//TODO: Change this to a separate error-type event'
					throw new ApplicationException("Error Downloading File: the file associated with the guid " + remoteFileGuid + " cannot be found" + Microsoft.VisualBasic.Constants.vbNewLine, ex);
				} else {
					if (UploadStatusEvent != null) {
						UploadStatusEvent("Error Downloading File");
					}
					//TODO: Change this to a separate error-type event'
					throw new ApplicationException("Error Downloading File:", ex);
				}
			} catch (System.Exception ex) {
				this.CloseStream(localStream);
				if (UploadStatusEvent != null) {
					UploadStatusEvent("Error Downloading File");
				}
				//TODO: Change this to a separate error-type event'
				throw new ApplicationException("Error Downloading File", ex);
			}
		}

		public class DistributedReLoginException : System.Exception
		{
		}

		private void CloseStream(System.IO.Stream stream)
		{
			if (stream == null)
				return;
			try {
				stream.Close();
			} catch {
			}
		}

		public event UploadStatusEventEventHandler UploadStatusEvent;
		public delegate void UploadStatusEventEventHandler(string message);
		public event UploadModeChangeEventEventHandler UploadModeChangeEvent;
		public delegate void UploadModeChangeEventEventHandler(string mode);

	}
}
