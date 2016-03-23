using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.FileObjects;
using kCura.Relativity.Export.Process;
using kCura.Relativity.Export.Types;
using Relativity;
using Constants = Relativity.Export.Constants;
using RelativityConstants = Relativity.Constants;


namespace kCura.Relativity.Export.Exports
{
	public class VolumeManager
	{

		#region "Members"

		private ExportFile _settings;
		private System.IO.StreamWriter _imageFileWriter;
		private System.IO.StreamWriter _nativeFileWriter;

		private System.IO.StreamWriter _errorWriter;
		private Int64 _nativeFileWriterPosition = 0;
		private Int64 _imageFileWriterPosition = 0;

		private Int64 _errorWriterPosition = 0;
		private Int32 _currentVolumeNumber;

		private Int32 _currentSubdirectoryNumber;
		private Int64 _currentVolumeSize;
		private Int64 _currentImageSubdirectorySize;
		private Int64 _currentNativeSubdirectorySize;

		private Int64 _currentTextSubdirectorySize;
		private Int32 _volumeLabelPaddingWidth;
		private Int32 _subdirectoryLabelPaddingWidth;

		private FileDownloader _downloadManager;
		private Exporter _parent;
		private string _columnHeaderString;
		private bool _hasWrittenColumnHeaderString = false;
		private System.Text.Encoding _encoding;
		private string _errorFileLocation = "";
		private kCura.Utility.Timekeeper _timekeeper;
		private Statistics _statistics;
		private Int64 _totalExtractedTextFileLength = 0;
		private bool _halt = false;
		private System.Collections.Generic.Dictionary<string, Int32> _ordinalLookup = new System.Collections.Generic.Dictionary<string, Int32>();
			#endregion
		private ILoadFileCellFormatter _loadFileFormatter;

		private enum ExportFileType
		{
			Image,
			Native
		}

		#region "Accessors"
		public string ErrorLogFileName {
			get {
				try {
					return _errorFileLocation;
				} catch (System.Exception ex) {
					return "";
				}
			}
		}

		public bool Halt {
			get { return _halt; }
			set { _halt = value; }
		}

		public ExportFile Settings {
			get { return _settings; }
		}

		private string CurrentVolumeLabel {
			get { return _settings.VolumeInfo.VolumePrefix + _currentVolumeNumber.ToString().PadLeft(_volumeLabelPaddingWidth, '0'); }
		}

		private string CurrentImageSubdirectoryLabel {
			get { return _settings.VolumeInfo.SubdirectoryImagePrefix + _currentSubdirectoryNumber.ToString().PadLeft(_subdirectoryLabelPaddingWidth, '0'); }
		}

		private string CurrentNativeSubdirectoryLabel {
			get { return _settings.VolumeInfo.SubdirectoryNativePrefix + _currentSubdirectoryNumber.ToString().PadLeft(_subdirectoryLabelPaddingWidth, '0'); }
		}

		private string CurrentFullTextSubdirectoryLabel {
			get { return _settings.VolumeInfo.SubdirectoryFullTextPrefix + _currentSubdirectoryNumber.ToString().PadLeft(_subdirectoryLabelPaddingWidth, '0'); }
		}

		public Int64 SubDirectoryMaxSize {
			get { return this.Settings.VolumeInfo.SubdirectoryMaxSize; }
		}

		public long VolumeMaxSize {
			get { return this.Settings.VolumeInfo.VolumeMaxSize * 1024 * 1024; }
		}

		public string ColumnHeaderString {
			get { return _columnHeaderString; }
			set { _columnHeaderString = value; }
		}

		public System.Collections.Generic.Dictionary<string, Int32> OrdinalLookup {
			get { return _ordinalLookup; }
		}

		protected virtual Int32 NumberOfRetries {
			get { return kCura.Utility.Config.IOErrorNumberOfRetries; }
		}

		protected virtual Int32 WaitTimeBetweenRetryAttempts {
			get { return kCura.Utility.Config.IOErrorWaitTimeInSeconds; }
		}

		#endregion

		#region "Constructors"

		public VolumeManager(ExportFile settings, string rootDirectory, bool overWriteFiles, Int64 totalFiles, Exporter parent, FileDownloader downloadHandler, kCura.Utility.Timekeeper t, string[] columnNamesInOrder, ExportStatistics statistics)
		{
			_settings = settings;
			_statistics = statistics;

			if (this.Settings.ExportImages) {
			}
			_timekeeper = t;
			_currentVolumeNumber = _settings.VolumeInfo.VolumeStartNumber;
			_currentSubdirectoryNumber = _settings.VolumeInfo.SubdirectoryStartNumber;
			Int32 volumeNumberPaddingWidth = (Int32)System.Math.Floor(System.Math.Log10(Convert.ToDouble(_currentVolumeNumber + 1)) + 1);
			Int32 subdirectoryNumberPaddingWidth = (Int32)System.Math.Floor(System.Math.Log10(Convert.ToDouble(_currentSubdirectoryNumber + 1)) + 1);
			Int32 totalFilesNumberPaddingWidth = (Int32)System.Math.Floor(System.Math.Log10(Convert.ToDouble(totalFiles + _currentVolumeNumber + 1)) + 1);
			_volumeLabelPaddingWidth = System.Math.Max(totalFilesNumberPaddingWidth, volumeNumberPaddingWidth);
			totalFilesNumberPaddingWidth = (Int32)System.Math.Floor(System.Math.Log10(Convert.ToDouble(totalFiles + _currentSubdirectoryNumber)) + 1);
			_subdirectoryLabelPaddingWidth = System.Math.Max(totalFilesNumberPaddingWidth, subdirectoryNumberPaddingWidth);

			//TODO in WEB
			//If Not (_volumeLabelPaddingWidth <= settings.VolumeDigitPadding AndAlso _subdirectoryLabelPaddingWidth <= settings.SubdirectoryDigitPadding) Then
			//	Dim message As New System.Text.StringBuilder
			//	If _volumeLabelPaddingWidth > settings.VolumeDigitPadding Then message.AppendFormat("The selected volume padding of {0} is less than the recommended volume padding {1} for this export" & vbNewLine, settings.VolumeDigitPadding, _volumeLabelPaddingWidth)
			//	If _subdirectoryLabelPaddingWidth > settings.SubdirectoryDigitPadding Then message.AppendFormat("The selected subdirectory padding of {0} is less than the recommended subdirectory padding {1} for this export" & vbNewLine, settings.SubdirectoryDigitPadding, _subdirectoryLabelPaddingWidth)
			//	message.Append("Continue with this selection?")
			//	Select Case MsgBox(message.ToString, MsgBoxStyle.OkCancel, "Relativity Desktop Client")
			//		Case MsgBoxResult.Cancel
			//			parent.Shutdown()
			//			Exit Sub
			//		Case Else
			//			If Not parent.ExportManager.HasExportPermissions(_settings.CaseArtifactID) Then Throw New Service.ExportManager.InsufficientPermissionsForExportException("Export permissions revoked!  Please contact your system administrator to re-instate export permissions.")
			//	End Select
			//End If

			_subdirectoryLabelPaddingWidth = settings.SubdirectoryDigitPadding;
			_volumeLabelPaddingWidth = settings.VolumeDigitPadding;
			_currentVolumeSize = 0;
			_currentImageSubdirectorySize = 0;
			_currentTextSubdirectorySize = 0;
			_currentNativeSubdirectorySize = 0;
			_downloadManager = downloadHandler;
			_parent = parent;
			string loadFilePath = this.LoadFileDestinationPath;
			//TODO in Web
			//If Not Me.Settings.Overwrite AndAlso System.IO.File.Exists(loadFilePath) Then
			//	MsgBox(String.Format("Overwrite not selected and file '{0}' exists.", loadFilePath))
			//	_parent.Shutdown()
			//	Exit Sub
			//End If
			//If Me.Settings.ExportImages AndAlso Not Me.Settings.Overwrite AndAlso System.IO.File.Exists(Me.ImageFileDestinationPath) Then
			//	MsgBox(String.Format("Overwrite not selected and file '{0}' exists.", Me.ImageFileDestinationPath))
			//	_parent.Shutdown()
			//	Exit Sub
			//End If
			_encoding = this.Settings.LoadFileEncoding;

			if (settings.ExportNative || settings.SelectedViewFields.Length > 0)
				_nativeFileWriter = new System.IO.StreamWriter(loadFilePath, false, _encoding);
			string imageFilePath = this.ImageFileDestinationPath;
			if (this.Settings.ExportImages) {
				if (!this.Settings.Overwrite && System.IO.File.Exists(imageFilePath)) {
					throw new System.Exception(string.Format("Overwrite not selected and file '{0}' exists.", imageFilePath));
				}
				_imageFileWriter = new System.IO.StreamWriter(imageFilePath, false, this.GetImageFileEncoding());
			}
			if (this.Settings.LoadFileIsHtml) {
				_loadFileFormatter = new HtmlCellFormatter(this.Settings);
			} else {
				_loadFileFormatter = new DelimitedCellFormatter(this.Settings);
			}
			for (Int32 i = 0; i <= columnNamesInOrder.Length - 1; i++) {
				_ordinalLookup.Add(columnNamesInOrder[i], i);
			}
			if ((this.Settings.SelectedTextFields != null) && this.Settings.SelectedTextFields.Count() > 0) {
				Int32 newindex = _ordinalLookup.Count;
				_ordinalLookup.Add(global::Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_ORIGINALSOURCE_AVF_COLUMN_NAME, newindex);
				_ordinalLookup.Add(global::Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME, newindex + 1);
			}

		}

		private System.Text.Encoding GetImageFileEncoding()
		{
			System.Text.Encoding retval = null;
			if (this.Settings.ExportImages) {
				retval = System.Text.Encoding.Default;
				if (this.Settings.LogFileFormat != LoadFileType.FileFormat.Opticon) {
					retval = System.Text.Encoding.UTF8;
				}
			} else {
				retval = _encoding;
			}
			return retval;
		}


		public string LoadFileDestinationPath {
			get { return this.Settings.FolderPath.TrimEnd('\\') + "\\" + this.Settings.LoadFilesPrefix + "_export." + this.Settings.LoadFileExtension; }
		}

		public string ImageFileDestinationPath {
			get {
				string logFileExension = "";
				switch (this.Settings.LogFileFormat) {
					case LoadFileType.FileFormat.Opticon:
						logFileExension = ".opt";
						break;
					case LoadFileType.FileFormat.IPRO:
						logFileExension = ".lfp";
						break;
					case LoadFileType.FileFormat.IPRO_FullText:
						logFileExension = "_FULLTEXT_.lfp";
						break;
					default:
						break;
				}
				return this.Settings.FolderPath.TrimEnd('\\') + "\\" + this.Settings.LoadFilesPrefix + "_export" + logFileExension;
			}
		}

		public string ErrorDestinationPath {
			get {
				if (string.IsNullOrEmpty(_errorFileLocation))
					_errorFileLocation = System.IO.Path.GetTempFileName();
				return _errorFileLocation;
			}
		}

		private void LogFileExportError(ExportFileType type, string recordIdentifier, string fileLocation, string errorText)
		{
			try {
				if (_errorWriter == null) {
					_errorWriter = new System.IO.StreamWriter(this.ErrorDestinationPath, false, _encoding);
					_errorWriter.WriteLine("\"File Type\",\"Document Identifier\",\"File Guid\",\"Error Description\"");
				}
				_errorWriter.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\"", type.ToString(), recordIdentifier, fileLocation, kCura.Utility.Strings.ToCsvCellContents(errorText)));
			} catch (System.IO.IOException ex) {
				throw new Exceptions.FileWriteException(kCura.Relativity.Export.Exceptions.FileWriteException.DestinationFile.Errors, ex);
			}
			_parent.WriteError(string.Format("{0} - Document [{1}] - File [{2}] - Error: {3}{4}", type.ToString(), recordIdentifier, fileLocation, System.Environment.NewLine, errorText));
		}

		public void Finish()
		{
			if ((_nativeFileWriter != null)) {
				_nativeFileWriter.Flush();
				_nativeFileWriter.Close();
			}
			if ((_imageFileWriter != null)) {
				_imageFileWriter.Flush();
				_imageFileWriter.Close();
			}
			if ((_errorWriter != null)) {
				_errorWriter.Flush();
				_errorWriter.Close();
			}
		}
		#endregion

		public Int64? ExportArtifact(ObjectExportInfo artifact)
		{
			Int32 tries = 0;
			Int32 maxTries = NumberOfRetries + 1;
			while (tries < maxTries & !this.Halt) {
				tries += 1;
				try {
					return this.ExportArtifact(artifact, tries > 1);
					break; // TODO: might not be correct. Was : Exit While
				} catch (Exceptions.ExportBaseException ex) {
					if (tries == maxTries)
						throw;
					_parent.WriteWarning(string.Format("Error writing data file(s) for document {0}", artifact.IdentifierValue));
					_parent.WriteWarning(string.Format("Actual error: {0}", ex.ToString()));
					if (tries > 1) {
						_parent.WriteWarning(string.Format("Waiting {0} seconds to retry", WaitTimeBetweenRetryAttempts));
						System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
					} else {
						_parent.WriteWarning("Retrying now");
					}
				}
			}
			return null;
		}

		private void ReInitializeAllStreams()
		{
			if ((_nativeFileWriter != null))
				_nativeFileWriter = this.ReInitializeStream(_nativeFileWriter, _nativeFileWriterPosition, this.LoadFileDestinationPath, _encoding);
			if ((_imageFileWriter != null))
				_imageFileWriter = this.ReInitializeStream(_imageFileWriter, _imageFileWriterPosition, this.ImageFileDestinationPath, this.GetImageFileEncoding());
		}

		private System.IO.StreamWriter ReInitializeStream(System.IO.StreamWriter brokenStream, Int64 position, string filepath, System.Text.Encoding encoding)
		{
			if (brokenStream == null)
				return null;
			try {
				brokenStream.Close();
			} catch (Exception ex) {
			}
			try {
				brokenStream = null;
			} catch (Exception ex) {
			}
			try {
				System.IO.StreamWriter retval = new System.IO.StreamWriter(filepath, true, encoding);
				retval.BaseStream.Position = position;
				return retval;
			} catch (System.IO.IOException ex) {
				throw new Exceptions.FileWriteException(kCura.Relativity.Export.Exceptions.FileWriteException.DestinationFile.Generic, ex);
			}
		}

		#region " Rollup Images "

		private void RollupImages(ref long imageCount, ref bool successfulRollup, ObjectExportInfo artifact, ImageExportInfo image)
		{
			string[] imageList = new string[artifact.Images.Count];
			for (Int32 i = 0; i <= imageList.Length - 1; i++) {
				imageList[i] = ((ImageExportInfo)artifact.Images[i]).TempLocation;
			}
			string tempLocation = this.Settings.FolderPath.TrimEnd('\\') + "\\" + System.Guid.NewGuid().ToString() + ".tmp";
			kCura.Utility.Image converter = new kCura.Utility.Image();
			try {
				switch (this.Settings.TypeOfImage) {
					case ExportFile.ImageType.MultiPageTiff:
						converter.ConvertTIFFsToMultiPage(imageList, tempLocation);
						break;
					case ExportFile.ImageType.Pdf:
						if ((tempLocation != null) && !string.IsNullOrEmpty(tempLocation))
							converter.ConvertImagesToMultiPagePdf(imageList, tempLocation);
						break;
				}
				imageCount = 1;
				foreach (string imageLocation in imageList) {
					kCura.Utility.File.Instance.Delete(imageLocation);
				}
				string ext = "";
				switch (this.Settings.TypeOfImage) {
					case ExportFile.ImageType.Pdf:
						ext = ".pdf";
						break;
					case ExportFile.ImageType.MultiPageTiff:
						ext = ".tif";
						break;
				}
				string currentTempLocation = this.GetImageExportLocation(image);
				if (currentTempLocation.IndexOf('.') != -1)
					currentTempLocation = currentTempLocation.Substring(0, currentTempLocation.LastIndexOf("."));
				currentTempLocation += ext;
				((ImageExportInfo)artifact.Images[0]).TempLocation = currentTempLocation;
				currentTempLocation = ((ImageExportInfo)artifact.Images[0]).FileName;
				if (currentTempLocation.IndexOf('.') != -1)
					currentTempLocation = currentTempLocation.Substring(0, currentTempLocation.LastIndexOf("."));
				currentTempLocation += ext;
				((ImageExportInfo)artifact.Images[0]).FileName = currentTempLocation;
				string location = ((ImageExportInfo)artifact.Images[0]).TempLocation;
				if (System.IO.File.Exists(((ImageExportInfo)artifact.Images[0]).TempLocation)) {
					if (this.Settings.Overwrite) {
						kCura.Utility.File.Instance.Delete(((ImageExportInfo)artifact.Images[0]).TempLocation);
						kCura.Utility.File.Instance.Move(tempLocation, ((ImageExportInfo)artifact.Images[0]).TempLocation);
					} else {
						_parent.WriteWarning("File exists - file copy skipped: " + ((ImageExportInfo)artifact.Images[0]).TempLocation);
					}
				} else {
					kCura.Utility.File.Instance.Move(tempLocation, ((ImageExportInfo)artifact.Images[0]).TempLocation);
				}
			} catch (kCura.Utility.Image.ImageRollupException ex) {
				successfulRollup = false;
				try {
					if ((tempLocation != null) && !string.IsNullOrEmpty(tempLocation))
						kCura.Utility.File.Instance.Delete(tempLocation);
					_parent.WriteImgProgressError(artifact, ex.ImageIndex, ex, "Document exported in single-page image mode.");
				} catch (System.IO.IOException ioex) {
					throw new Exceptions.FileWriteException(kCura.Relativity.Export.Exceptions.FileWriteException.DestinationFile.Errors, ioex);
				}
			}
		}

		#endregion

		private string CopySelectedLongTextToFile(ObjectExportInfo artifact, ref Int64 len)
		{
			Types.ViewFieldInfo field = this.GetFieldForLongTextPrecedenceDownload(null, artifact);
			if (!this.OrdinalLookup.ContainsKey(global::Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME)) {
				return string.Empty;
			}
			object text = artifact.Metadata[this.OrdinalLookup[global::Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME]];
			if (text == null)
				text = string.Empty;
			string longText = text.ToString();
			if (longText == RelativityConstants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN) {
				string filePath = this.DownloadTextFieldAsFile(artifact, field);
				len += new System.IO.FileInfo(filePath).Length;
				return filePath;
			} else {
				len += longText.Length;
				return string.Empty;
			}
		}

		private bool TextPrecedenceIsSet()
		{
			if (this.Settings.SelectedTextFields == null)
				return false;
			if (this.Settings.SelectedTextFields.Count() == 0)
				return false;
			return this.Settings.SelectedTextFields.Any(f =>  f != null);
		}

		private Int64 ExportArtifact(ObjectExportInfo artifact, bool isRetryAttempt)
		{
			if (isRetryAttempt)
				this.ReInitializeAllStreams();
			Int64 totalFileSize = 0;
			Int64 loadFileBytes = 0;
			Int64 extractedTextFileSizeForVolume = 0;
			ImageExportInfo image = null;
			bool imageSuccess = true;
			bool nativeSuccess = true;
			bool updateVolumeAfterExport = false;
			bool updateSubDirectoryAfterExport = false;
			if (this.Settings.ExportImages) {
				foreach (ImageExportInfo image_loopVariable in artifact.Images) {
					image = image_loopVariable;
					_timekeeper.MarkStart("VolumeManager_DownloadImage");
					try {
						if (this.Settings.VolumeInfo.CopyFilesFromRepository) {
							totalFileSize += this.DownloadImage(image);
						}
						image.HasBeenCounted = true;
					} catch (System.Exception ex) {
						image.TempLocation = "";
						this.LogFileExportError(ExportFileType.Image, artifact.IdentifierValue, image.FileGuid, ex.ToString());
						imageSuccess = false;
					}
					_timekeeper.MarkEnd("VolumeManager_DownloadImage");
				}
			}
			long imageCount = artifact.Images.Count;
			bool successfulRollup = true;
			if (artifact.Images.Count > 0 && (this.Settings.TypeOfImage == ExportFile.ImageType.MultiPageTiff || this.Settings.TypeOfImage == ExportFile.ImageType.Pdf)) {
				this.RollupImages(ref imageCount, ref successfulRollup, artifact, image);
			}

			if (this.Settings.ExportNative) {
				_timekeeper.MarkStart("VolumeManager_DownloadNative");
				try {
					if (this.Settings.VolumeInfo.CopyFilesFromRepository) {
						Int64 downloadSize = this.DownloadNative(artifact);
						if (!artifact.HasCountedNative)
							totalFileSize += downloadSize;
					}
					artifact.HasCountedNative = true;
				} catch (System.Exception ex) {
					this.LogFileExportError(ExportFileType.Native, artifact.IdentifierValue, artifact.NativeFileGuid, ex.ToString());
				}
				_timekeeper.MarkEnd("VolumeManager_DownloadNative");
			}
			string tempLocalFullTextFilePath = "";
			string tempLocalIproFullTextFilePath = "";

			long extractedTextFileLength = 0;
			if (this.Settings.ExportFullText && this.Settings.ExportFullTextAsFile) {
				Int64 len = 0;
				tempLocalFullTextFilePath = this.CopySelectedLongTextToFile(artifact, ref len);
				if (this.Settings.ExportFullTextAsFile) {
					if (!artifact.HasCountedTextFile) {
						totalFileSize += len;
						extractedTextFileSizeForVolume += len;
					}
					artifact.HasCountedTextFile = true;
				}
				artifact.HasFullText = true;
			}

			if (this.Settings.LogFileFormat == LoadFileType.FileFormat.IPRO_FullText && this.Settings.ExportImages) {
				if (!this.TextPrecedenceIsSet()) {
					tempLocalIproFullTextFilePath = System.IO.Path.GetTempFileName();
					Int32 tries = 0;
					Int32 maxTries = NumberOfRetries + 1;
					Int64 start = System.DateTime.Now.Ticks;
					//BigData_ET_1037768
					string val = artifact.Metadata[this.OrdinalLookup["ExtractedText"]].ToString();
					if (val != RelativityConstants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN) {
						System.IO.StreamWriter sw = new System.IO.StreamWriter(tempLocalIproFullTextFilePath, false, System.Text.Encoding.Unicode);
						sw.Write(val);
						sw.Close();
					} else {
						while (tries < maxTries && !this.Halt) {
							tries += 1;
							try {
								_downloadManager.DownloadFullTextFile(tempLocalIproFullTextFilePath, artifact.ArtifactID, _settings.CaseInfo.ArtifactID.ToString());
								break; // TODO: might not be correct. Was : Exit While
							} catch (System.Exception ex) {
								if (tries == 1) {
									_parent.WriteStatusLine(kCura.Windows.Process.EventType.Warning, "Second attempt to download full text for document " + artifact.IdentifierValue, true);
								} else if (tries < maxTries) {
									Int32 waitTime = WaitTimeBetweenRetryAttempts;
									_parent.WriteStatusLine(kCura.Windows.Process.EventType.Warning, "Additional attempt to download full text for document " + artifact.IdentifierValue + " failed - retrying in " + waitTime.ToString() + " seconds", true);
									System.Threading.Thread.CurrentThread.Join(waitTime * 1000);
								} else {
									throw;
								}
							}
						}
					}
					_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1);
				} else {
					if (tempLocalFullTextFilePath != string.Empty) {
						tempLocalIproFullTextFilePath = string.Copy(tempLocalFullTextFilePath);
					} else {
						tempLocalIproFullTextFilePath = System.IO.Path.GetTempFileName();
						System.IO.StreamWriter sw = new System.IO.StreamWriter(tempLocalIproFullTextFilePath, false, System.Text.Encoding.Unicode);
						string val = artifact.Metadata[this.OrdinalLookup[global::Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME]].ToString();
						sw.Write(val);
						sw.Close();
					}
				}
			}

			Int32 textCount = 0;
			if (this.Settings.ExportFullTextAsFile && artifact.HasFullText)
				textCount += 1;
			if (totalFileSize + _currentVolumeSize > this.VolumeMaxSize) {
				if (_currentVolumeSize == 0) {
					updateVolumeAfterExport = true;
				} else {
					this.UpdateVolume();
				}
			} else if (imageCount + _currentImageSubdirectorySize > this.SubDirectoryMaxSize) {
				this.UpdateSubdirectory();
			} else if (artifact.NativeCount + _currentNativeSubdirectorySize > this.SubDirectoryMaxSize) {
				this.UpdateSubdirectory();
			} else if (textCount + _currentTextSubdirectorySize > this.SubDirectoryMaxSize) {
				this.UpdateSubdirectory();
			}
			if (this.Settings.ExportImages) {
				_timekeeper.MarkStart("VolumeManager_ExportImages");
				this.ExportImages(artifact.Images, tempLocalIproFullTextFilePath, successfulRollup);
				_timekeeper.MarkEnd("VolumeManager_ExportImages");
			}
			Int32 nativeCount = 0;
			string nativeLocation = "";
			if (this.Settings.ExportNative && this.Settings.VolumeInfo.CopyFilesFromRepository) {
				string nativeFileName = this.GetNativeFileName(artifact);
				string localFilePath = this.GetLocalNativeFilePath(artifact, nativeFileName);
				_timekeeper.MarkStart("VolumeManager_ExportNative");
				this.ExportNative(localFilePath, artifact.NativeFileGuid, artifact.ArtifactID, nativeFileName, artifact.NativeTempLocation);
				_timekeeper.MarkEnd("VolumeManager_ExportNative");
				if (string.IsNullOrEmpty(artifact.NativeTempLocation)) {
					nativeLocation = "";
				} else {
					nativeCount = 1;
					switch (this.Settings.TypeOfExportedFilePath) {
						case ExportFile.ExportedFilePathType.Absolute:
							nativeLocation = localFilePath;
							break;
						case ExportFile.ExportedFilePathType.Relative:
							nativeLocation = ".\\" + this.CurrentVolumeLabel + "\\" + this.CurrentNativeSubdirectoryLabel + "\\" + nativeFileName;
							break;
						case ExportFile.ExportedFilePathType.Prefix:
							nativeLocation = this.Settings.FilePrefix.TrimEnd('\\') + "\\" + this.CurrentVolumeLabel + "\\" + this.CurrentNativeSubdirectoryLabel + "\\" + nativeFileName;
							break;
					}
				}
			}
			try {
				if (!_hasWrittenColumnHeaderString && (_nativeFileWriter != null)) {
					_nativeFileWriter.Write(_columnHeaderString);
					_hasWrittenColumnHeaderString = true;
				}
				this.UpdateLoadFile(artifact.Metadata, artifact.HasFullText, artifact.ArtifactID, nativeLocation, ref tempLocalFullTextFilePath, artifact, ref extractedTextFileLength);
			} catch (System.IO.IOException ex) {
				throw new Exceptions.FileWriteException(kCura.Relativity.Export.Exceptions.FileWriteException.DestinationFile.Load, ex);
			}

			_parent.DocumentsExported += artifact.DocCount;
			_currentVolumeSize += totalFileSize;
			if (this.Settings.VolumeInfo.CopyFilesFromRepository) {
				_currentNativeSubdirectorySize += artifact.NativeCount;
				if (this.Settings.ExportFullTextAsFile && artifact.HasFullText)
					_currentTextSubdirectorySize += 1;
				_currentImageSubdirectorySize += imageCount;
			}
			if (updateSubDirectoryAfterExport)
				this.UpdateSubdirectory();
			if (updateVolumeAfterExport)
				this.UpdateVolume();
			_parent.WriteUpdate("Document " + artifact.IdentifierValue + " exported.", false);

			try {
				if ((_nativeFileWriter != null))
					_nativeFileWriter.Flush();
			} catch (Exception ex) {
				throw new Exceptions.FileWriteException(kCura.Relativity.Export.Exceptions.FileWriteException.DestinationFile.Load, ex);
			}
			try {
				if ((_imageFileWriter != null))
					_imageFileWriter.Flush();
			} catch (Exception ex) {
				throw new Exceptions.FileWriteException(kCura.Relativity.Export.Exceptions.FileWriteException.DestinationFile.Image, ex);
			}
			try {
				if ((_errorWriter != null))
					_errorWriter.Flush();
			} catch (Exception ex) {
				throw new Exceptions.FileWriteException(kCura.Relativity.Export.Exceptions.FileWriteException.DestinationFile.Errors, ex);
			}
			if ((_nativeFileWriter != null)) {
				_nativeFileWriterPosition = _nativeFileWriter.BaseStream.Position;
				loadFileBytes += kCura.Utility.File.Instance.GetFileSize(((System.IO.FileStream)_nativeFileWriter.BaseStream).Name);
			}
			if ((_imageFileWriter != null)) {
				_imageFileWriterPosition = _imageFileWriter.BaseStream.Position;
				loadFileBytes += kCura.Utility.File.Instance.GetFileSize(((System.IO.FileStream)_imageFileWriter.BaseStream).Name);
			}
			_totalExtractedTextFileLength += extractedTextFileLength;
			_statistics.MetadataBytes = loadFileBytes + _totalExtractedTextFileLength;
			_statistics.FileBytes += totalFileSize - extractedTextFileSizeForVolume;
			if ((_errorWriter != null))
				_errorWriterPosition = _errorWriter.BaseStream.Position;
			TempTextFileDeletor deletor = new TempTextFileDeletor(new string[] {
				tempLocalIproFullTextFilePath,
				tempLocalFullTextFilePath
			});
			System.Threading.Thread t = new System.Threading.Thread(deletor.DeleteFiles);
			t.Start();
			if (!this.Settings.VolumeInfo.CopyFilesFromRepository) {
				return 0;
			} else {
				return imageCount + nativeCount;
			}
		}

		private Types.ViewFieldInfo GetFieldForLongTextPrecedenceDownload(Types.ViewFieldInfo input, ObjectExportInfo artifact)
		{
			Types.ViewFieldInfo retval = input;
			if (input == null || input.AvfColumnName == global::Relativity.Export.Constants.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME) {
				if (this.Settings.SelectedTextFields != null) {
					retval =  this.Settings.SelectedTextFields.First(f => f.FieldArtifactId == Convert.ToInt32(artifact.Metadata[_ordinalLookup[Constants.TEXT_PRECEDENCE_AWARE_ORIGINALSOURCE_AVF_COLUMN_NAME]]));
				}
			}
			return retval;
		}
		private string DownloadTextFieldAsFile(ObjectExportInfo artifact, Types.ViewFieldInfo field)
		{
			string tempLocalFullTextFilePath = System.IO.Path.GetTempFileName();
			Int32 tries = 0;
			Int32 maxTries = NumberOfRetries + 1;
			Int64 start = System.DateTime.Now.Ticks;
			while (tries < maxTries && !this.Halt) {
				tries += 1;
				try {
					if (this.Settings.ArtifactTypeID == (int)ArtifactType.Document && field.Category == FieldCategory.FullText && !(field is CoalescedTextViewField)) {
						_downloadManager.DownloadFullTextFile(tempLocalFullTextFilePath, artifact.ArtifactID, _settings.CaseInfo.ArtifactID.ToString());
					} else {
						Types.ViewFieldInfo fieldToActuallyExportFrom = this.GetFieldForLongTextPrecedenceDownload(field, artifact);
						_downloadManager.DownloadLongTextFile(tempLocalFullTextFilePath, artifact.ArtifactID, fieldToActuallyExportFrom, _settings.CaseInfo.ArtifactID.ToString());
					}
					break; // TODO: might not be correct. Was : Exit While
				} catch (System.Exception ex) {
					if (tries == 1) {
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Warning, "Second attempt to download full text for document " + artifact.IdentifierValue, true);
					} else if (tries < maxTries) {
						Int32 waitTime = WaitTimeBetweenRetryAttempts;
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Warning, "Additional attempt to download full text for document " + artifact.IdentifierValue + " failed - retrying in " + waitTime.ToString() + " seconds", true);
						System.Threading.Thread.CurrentThread.Join(waitTime * 1000);
					} else {
						throw;
					}
				}
			}
			_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1);
			return tempLocalFullTextFilePath;
		}

		private string GetLocalNativeFilePath(ObjectExportInfo doc, string nativeFileName)
		{
			string localFilePath = this.Settings.FolderPath;
			if (localFilePath[localFilePath.Length - 1] != '\\')
				localFilePath += "\\";
			localFilePath += this.CurrentVolumeLabel + "\\" + this.CurrentNativeSubdirectoryLabel + "\\";
			if (!System.IO.Directory.Exists(localFilePath))
				System.IO.Directory.CreateDirectory(localFilePath);
			return localFilePath + nativeFileName;
		}

		private string GetLocalTextFilePath(ObjectExportInfo doc)
		{
			string localFilePath = this.Settings.FolderPath;
			if (localFilePath[localFilePath.Length - 1] != '\\')
				localFilePath += "\\";
			localFilePath += this.CurrentVolumeLabel + "\\" + this.CurrentFullTextSubdirectoryLabel + "\\";
			if (!System.IO.Directory.Exists(localFilePath))
				System.IO.Directory.CreateDirectory(localFilePath);
			return localFilePath + doc.FullTextFileName(this.NameTextFilesAfterIdentifier());
		}

		private string GetNativeFileName(ObjectExportInfo doc)
		{
			switch (_parent.ExportNativesToFileNamedFrom) {
				case ExportNativeWithFilenameFrom.Identifier:
					return doc.NativeFileName(this.Settings.AppendOriginalFileName);
				case ExportNativeWithFilenameFrom.Production:
					return doc.ProductionBeginBatesFileName(this.Settings.AppendOriginalFileName);
			}
			return null;
		}

		public void Close()
		{
			if ((_imageFileWriter != null)) {
				_imageFileWriter.Flush();
				_imageFileWriter.Close();
			}
			if ((_nativeFileWriter != null)) {
				_nativeFileWriter.Flush();
				_nativeFileWriter.Close();
			}
		}

		#region "Image Export"

		private string GetImageExportLocation(ImageExportInfo image)
		{
			string localFilePath = this.Settings.FolderPath;
			string subfolderPath = this.CurrentVolumeLabel + "\\" + this.CurrentImageSubdirectoryLabel + "\\";
			if (localFilePath[localFilePath.Length - 1] != '\\')
				localFilePath += "\\";
			localFilePath += subfolderPath;
			if (!System.IO.Directory.Exists(localFilePath))
				System.IO.Directory.CreateDirectory(localFilePath);
			return localFilePath + image.FileName;
		}


		public void ExportImages(System.Collections.ArrayList images, string localFullTextPath, bool successfulRollup)
		{
			ImageExportInfo image = null;
			Int32 i = 0;
			System.IO.StreamReader fullTextReader = null;
			string localFilePath = this.Settings.FolderPath;
			string subfolderPath = this.CurrentVolumeLabel + "\\" + this.CurrentImageSubdirectoryLabel + "\\";
			long pageOffset = 0;
			if (localFilePath[localFilePath.Length - 1] != '\\')
				localFilePath += "\\";
			localFilePath += subfolderPath;
			if (!System.IO.Directory.Exists(localFilePath) && this.Settings.VolumeInfo.CopyFilesFromRepository)
				System.IO.Directory.CreateDirectory(localFilePath);
			try {
				if (this.Settings.LogFileFormat == LoadFileType.FileFormat.IPRO_FullText) {
					if (System.IO.File.Exists(localFullTextPath)) {
						fullTextReader = new System.IO.StreamReader(localFullTextPath, _encoding, true);
					}
				}
				if (images.Count > 0 && (this.Settings.TypeOfImage == ExportFile.ImageType.MultiPageTiff || this.Settings.TypeOfImage == ExportFile.ImageType.Pdf) && successfulRollup) {
					ImageExportInfo marker = (ImageExportInfo)images[0];
					this.ExportDocumentImage(localFilePath + marker.FileName, marker.FileGuid, marker.ArtifactID, marker.BatesNumber, marker.TempLocation);
					string copyfile = null;
					switch (this.Settings.TypeOfExportedFilePath) {
						case ExportFile.ExportedFilePathType.Absolute:
							copyfile = localFilePath + marker.FileName;
							break;
						case ExportFile.ExportedFilePathType.Relative:
							copyfile = ".\\" + subfolderPath + marker.FileName;
							break;
						case ExportFile.ExportedFilePathType.Prefix:
							copyfile = this.Settings.FilePrefix.TrimEnd('\\') + "\\" + subfolderPath + marker.FileName;
							break;
					}
					if (this.Settings.LogFileFormat == LoadFileType.FileFormat.Opticon) {
						this.CreateImageLogEntry(marker.BatesNumber, copyfile, localFilePath, 1, fullTextReader, !string.IsNullOrEmpty(localFullTextPath), Int64.MinValue, images.Count);
					} else {
						for (Int32 j = 0; j <= images.Count - 1; j++) {
							if ((j == 0 && ((ImageExportInfo)images[j]).PageOffset == null) || j == images.Count - 1) {
								pageOffset = Int64.MinValue;
							} else {
								ImageExportInfo nextImage = (ImageExportInfo)images[j + 1];
								if (nextImage.PageOffset == null) {
									pageOffset = Int64.MinValue;
								} else {
									pageOffset = nextImage.PageOffset.Value;
								}
							}
							image = (ImageExportInfo)images[j];
							this.CreateImageLogEntry(image.BatesNumber, copyfile, localFilePath, j + 1, fullTextReader, !string.IsNullOrEmpty(localFullTextPath), pageOffset, images.Count);
						}
					}
					marker.TempLocation = copyfile;
				} else {
					foreach (ImageExportInfo image_loopVariable in images) {
						image = image_loopVariable;
						if ((i == 0 && image.PageOffset == null) || i == images.Count - 1) {
							pageOffset = Int64.MinValue;
						} else {
							ImageExportInfo nextImage = (ImageExportInfo)images[i + 1];
							if (nextImage.PageOffset == null) {
								pageOffset = Int64.MinValue;
							} else {
								pageOffset = nextImage.PageOffset.Value;
							}
						}
						if (this.Settings.VolumeInfo.CopyFilesFromRepository) {
							this.ExportDocumentImage(localFilePath + image.FileName, image.FileGuid, image.ArtifactID, image.BatesNumber, image.TempLocation);
							string copyfile = null;
							switch (this.Settings.TypeOfExportedFilePath) {
								case ExportFile.ExportedFilePathType.Absolute:
									copyfile = localFilePath + image.FileName;
									break;
								case ExportFile.ExportedFilePathType.Relative:
									copyfile = ".\\" + subfolderPath + image.FileName;
									break;
								case ExportFile.ExportedFilePathType.Prefix:
									copyfile = this.Settings.FilePrefix.TrimEnd('\\') + "\\" + subfolderPath + image.FileName;
									break;
							}
							this.CreateImageLogEntry(image.BatesNumber, copyfile, localFilePath, i + 1, fullTextReader, !string.IsNullOrEmpty(localFullTextPath), pageOffset, images.Count);
							image.TempLocation = copyfile;
						} else {
							this.CreateImageLogEntry(image.BatesNumber, image.SourceLocation, image.SourceLocation, i + 1, fullTextReader, !string.IsNullOrEmpty(localFullTextPath), pageOffset, images.Count);
						}
						i += 1;
					}
				}

			} catch (System.Exception ex) {
				if ((fullTextReader != null))
					fullTextReader.Close();
				throw;
			}
			if ((fullTextReader != null))
				fullTextReader.Close();
		}

		private Int64 DownloadImage(ImageExportInfo image)
		{
			if (string.IsNullOrEmpty(image.FileGuid))
				return 0;
			Int64 start = System.DateTime.Now.Ticks;
			string tempFile = this.GetImageExportLocation(image);
			//If Me.Settings.TypeOfImage = ExportFile.ImageType.Pdf Then
			//	tempFile = System.IO.Path.GetTempFileName
			//	kCura.Utility.File.Instance.Delete(tempFile)
			//End If
			if (System.IO.File.Exists(tempFile)) {
				if (_settings.Overwrite) {
					kCura.Utility.File.Instance.Delete(tempFile);
					_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, string.Format("Overwriting image for {0}.", image.BatesNumber), false);
				} else {
					_parent.WriteWarning(string.Format("{0} already exists. Skipping file export.", tempFile));
					return 0;
				}
			}
			Int32 tries = 0;
			Int32 maxTries = NumberOfRetries + 1;
			while (tries < maxTries && !this.Halt) {
				tries += 1;
				try {
					_downloadManager.DownloadFileForDocument(tempFile, image.FileGuid, image.SourceLocation, image.ArtifactID, _settings.CaseArtifactID.ToString());
					image.TempLocation = tempFile;
					break; // TODO: might not be correct. Was : Exit While
				} catch (System.Exception ex) {
					if (tries == 1) {
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Warning, "Second attempt to download image " + image.BatesNumber + " - exact error: " + ex.ToString(), true);
					} else if (tries < maxTries) {
						Int32 waitTime = WaitTimeBetweenRetryAttempts;
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Warning, "Additional attempt to download image " + image.BatesNumber + " failed - retrying in " + waitTime.ToString() + " seconds - exact error: " + ex.ToString(), true);
						System.Threading.Thread.CurrentThread.Join(waitTime * 1000);
					} else {
						throw;
					}
				}
			}
			_statistics.FileTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1);
			return kCura.Utility.File.Instance.Length(tempFile);
		}

		private void ExportDocumentImage(string fileName, string fileGuid, Int32 artifactID, string batesNumber, string tempFileLocation)
		{
			if (!string.IsNullOrEmpty(tempFileLocation) && !(tempFileLocation.ToLower() == fileName.ToLower())) {
				if (System.IO.File.Exists(fileName)) {
					if (_settings.Overwrite) {
						kCura.Utility.File.Instance.Delete(fileName);
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, string.Format("Overwriting document image {0}.", batesNumber), false);
						kCura.Utility.File.Instance.Move(tempFileLocation, fileName);
					} else {
						_parent.WriteWarning(string.Format("{0}.tif already exists. Skipping file export.", batesNumber));
					}
				} else {
					_timekeeper.MarkStart("VolumeManager_ExportDocumentImage_WriteStatus");
					_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, string.Format("Now exporting document image {0}.", batesNumber), false);
					_timekeeper.MarkEnd("VolumeManager_ExportDocumentImage_WriteStatus");
					_timekeeper.MarkStart("VolumeManager_ExportDocumentImage_MoveFile");
					kCura.Utility.File.Instance.Move(tempFileLocation, fileName);
					_timekeeper.MarkEnd("VolumeManager_ExportDocumentImage_MoveFile");
				}
				_timekeeper.MarkStart("VolumeManager_ExportDocumentImage_WriteStatus");
				_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, string.Format("Finished exporting document image {0}.", batesNumber), false);
				_timekeeper.MarkEnd("VolumeManager_ExportDocumentImage_WriteStatus");
			}
			//_parent.DocumentsExported += 1
		}

		private string GetLfpFullTextTransform(char c)
		{


		    if (c == Strings.ChrW(10) || c == ' ')
		    {
		        return "|0|0|0|0^";
		    }
            else if (c == ',')
					return "";
            else { 
					return c.ToString();
			}
		}

		private void CreateImageLogEntry(string batesNumber, string copyFile, string pathToImage, Int32 pageNumber, System.IO.StreamReader fullTextReader, bool expectingTextForPage, long pageOffset, Int32 numberOfImages)
		{
			try {
				switch (_settings.LogFileFormat) {
					case LoadFileType.FileFormat.Opticon:
						this.WriteOpticonLine(batesNumber, pageNumber == 1, copyFile, numberOfImages);
						break;
					case LoadFileType.FileFormat.IPRO:
						this.WriteIproImageLine(batesNumber, pageNumber, copyFile);
						break;
					case LoadFileType.FileFormat.IPRO_FullText:
						long currentPageFirstByteNumber = 0;
						if (fullTextReader == null) {
							if (pageNumber == 1 && expectingTextForPage)
								_parent.WriteWarning(string.Format("Could not retrieve full text for document '{0}'", batesNumber));
						} else {
							if (pageNumber == 1) {
								currentPageFirstByteNumber = 0;
							} else {
								currentPageFirstByteNumber = fullTextReader.BaseStream.Position;
							}
							_imageFileWriter.Write("FT,");
							_imageFileWriter.Write(batesNumber);
							_imageFileWriter.Write(",1,1,");
							switch (pageOffset) {
								case Int64.MinValue:
									Int32 c32 = fullTextReader.Read();
									while (c32 != -1) {
										_imageFileWriter.Write(this.GetLfpFullTextTransform(Strings.ChrW(c32)));
										c32 = fullTextReader.Read();
									}
									break;
								default:
									Int32 i = 0;
									Int32 c = fullTextReader.Read();
									while (i < pageOffset && c != -1) {
										_imageFileWriter.Write(this.GetLfpFullTextTransform(Strings.ChrW(c)));
										c = fullTextReader.Read();
										i += 1;
									}
									break;
							}
							_imageFileWriter.Write(Microsoft.VisualBasic.Constants.vbNewLine);
						}
						_imageFileWriter.Flush();
						this.WriteIproImageLine(batesNumber, pageNumber, copyFile);
						break;
				}
			} catch (System.IO.IOException ex) {
				throw new Exceptions.FileWriteException(kCura.Relativity.Export.Exceptions.FileWriteException.DestinationFile.Image, ex);
			}
		}

		private void WriteIproImageLine(string batesNumber, Int32 pageNumber, string fullFilePath)
		{
			LineFactory.SimpleIproImageLineFactory linefactory = new LineFactory.SimpleIproImageLineFactory(batesNumber, pageNumber, fullFilePath, this.CurrentVolumeLabel, this.Settings.TypeOfImage.Value);
			linefactory.WriteLine(_imageFileWriter);
		}

		private void WriteOpticonLine(string batesNumber, bool firstDocument, string copyFile, Int32 imageCount)
		{
			System.Text.StringBuilder log = new System.Text.StringBuilder();
			log.AppendFormat("{0},{1},{2},", batesNumber, this.CurrentVolumeLabel, copyFile);
			if (firstDocument)
				log.Append("Y");
			log.Append(",,,");
			if (firstDocument)
				log.Append(imageCount);
			_imageFileWriter.WriteLine(log.ToString());
		}
		#endregion

		private string ExportNative(string exportFileName, string fileGuid, Int32 artifactID, string systemFileName, string tempLocation)
		{
			if (!string.IsNullOrEmpty(tempLocation) && !(tempLocation.ToLower() == exportFileName.ToLower()) && this.Settings.VolumeInfo.CopyFilesFromRepository) {
				if (System.IO.File.Exists(exportFileName)) {
					if (_settings.Overwrite) {
						kCura.Utility.File.Instance.Delete(exportFileName);
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, string.Format("Overwriting document {0}.", systemFileName), false);
						kCura.Utility.File.Instance.Move(tempLocation, exportFileName);
					} else {
						_parent.WriteWarning(string.Format("{0} already exists. Skipping file export.", systemFileName));
					}
				} else {
					_timekeeper.MarkStart("VolumeManager_ExportNative_WriteStatus");
					_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, string.Format("Now exporting document {0}.", systemFileName), false);
					_timekeeper.MarkEnd("VolumeManager_ExportNative_WriteStatus");
					_timekeeper.MarkStart("VolumeManager_ExportNative_MoveFile");
					kCura.Utility.File.Instance.Move(tempLocation, exportFileName);
					_timekeeper.MarkEnd("VolumeManager_ExportNative_MoveFile");
				}
			}
			_timekeeper.MarkStart("VolumeManager_ExportNative_WriteStatus");
			_timekeeper.MarkEnd("VolumeManager_ExportNative_WriteStatus");
			return null;
		}

		private Int64 DownloadNative(ObjectExportInfo artifact)
		{
			if (this.Settings.ArtifactTypeID == (int)ArtifactType.Document && string.IsNullOrEmpty(artifact.NativeFileGuid))
				return 0;
			if (!(this.Settings.ArtifactTypeID == (int)ArtifactType.Document) && (!(artifact.FileID > 0) || artifact.NativeSourceLocation.Trim() == string.Empty))
				return 0;
			string nativeFileName = this.GetNativeFileName(artifact);
			string tempFile = this.GetLocalNativeFilePath(artifact, nativeFileName);
			Int64 start = System.DateTime.Now.Ticks;
			if (System.IO.File.Exists(tempFile)) {
				if (Settings.Overwrite) {
					kCura.Utility.File.Instance.Delete(tempFile);
					_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, string.Format("Overwriting document {0}.", nativeFileName), false);
				} else {
					_parent.WriteWarning(string.Format("{0} already exists. Skipping file export.", tempFile));
					artifact.NativeTempLocation = tempFile;
					return kCura.Utility.File.Instance.Length(tempFile);
				}
			}
			Int32 tries = 0;
			Int32 maxTries = NumberOfRetries + 1;
			while (tries < maxTries && !this.Halt) {
				tries += 1;
				try {
					if (this.Settings.ArtifactTypeID == (int)ArtifactType.Document) {
						_downloadManager.DownloadFileForDocument(tempFile, artifact.NativeFileGuid, artifact.NativeSourceLocation, artifact.ArtifactID, _settings.CaseArtifactID.ToString());
					} else {
						_downloadManager.DownloadFileForDynamicObject(tempFile, artifact.NativeSourceLocation, artifact.ArtifactID, _settings.CaseArtifactID.ToString(), artifact.FileID, this.Settings.FileField.FieldID);
					}
					break; // TODO: might not be correct. Was : Exit While
				} catch (System.Exception ex) {
					if (tries == 1) {
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Warning, "Second attempt to download native for document " + artifact.IdentifierValue, true);
					} else if (tries < maxTries) {
						Int32 waitTime = WaitTimeBetweenRetryAttempts;
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Warning, "Additional attempt to download native for document " + artifact.IdentifierValue + " failed - retrying in " + waitTime.ToString() + " seconds", true);
						System.Threading.Thread.CurrentThread.Join(waitTime * 1000);
					} else {
						throw;
					}
				}
			}
			artifact.NativeTempLocation = tempFile;
			_statistics.FileTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1);
			return kCura.Utility.File.Instance.Length(tempFile);
		}

		private void WriteLongText(System.IO.TextReader source, System.IO.TextWriter output, ILongTextStreamFormatter formatter)
		{
			Int32 c = source.Read();
			bool doTransform = (object.ReferenceEquals(output, _nativeFileWriter));
			try {
				while (c != -1) {
					formatter.TransformAndWriteCharacter(c, output);
					c = source.Read();
				}
			} finally {
				if ((source != null)) {
					try {
						source.Close();
					} catch {
					}
				}
				if ((output != null) && !doTransform) {
					try {
						output.Close();
					} catch {
					}
				}
			}
		}

		private long ManageLongText(object sourceValue, Types.ViewFieldInfo textField, ref string downloadedTextFilePath, ObjectExportInfo artifact, string startBound, string endBound)
		{
			_nativeFileWriter.Write(startBound);
			if (sourceValue is byte[]) {
				sourceValue = System.Text.Encoding.Unicode.GetString((byte[])sourceValue);
			}
			if (sourceValue == null)
				sourceValue = string.Empty;
			string textValue = sourceValue.ToString();
			System.IO.TextReader source = null;
			System.IO.TextWriter destination = null;
			bool downloadedFileExists = !string.IsNullOrEmpty(downloadedTextFilePath) && System.IO.File.Exists(downloadedTextFilePath);
			if (textValue == RelativityConstants.LONG_TEXT_EXCEEDS_MAX_LENGTH_FOR_LIST_TOKEN) {
				if (this.Settings.SelectedTextFields != null && textField is CoalescedTextViewField && downloadedFileExists) {
					if (Settings.SelectedTextFields.Count() == 1) {
						source = this.GetLongTextStream(downloadedTextFilePath, textField);
					} else {
						Types.ViewFieldInfo precedenceField = GetFieldForLongTextPrecedenceDownload(textField, artifact);
						source = this.GetLongTextStream(downloadedTextFilePath, precedenceField);
					}
				} else {
					source = this.GetLongTextStream(artifact, textField);
				}
			} else {
				source = new System.IO.StringReader(textValue);
			}
			bool destinationPathExists = false;
			string destinationFilePath = string.Empty;
			ILongTextStreamFormatter formatter = null;
			if (this.Settings.ExportFullTextAsFile && textField is CoalescedTextViewField) {
				destinationFilePath = this.GetLocalTextFilePath(artifact);
				destinationPathExists = System.IO.File.Exists(destinationFilePath);
				if (destinationPathExists && !_settings.Overwrite) {
					_parent.WriteWarning(destinationFilePath + " already exists. Skipping file export.");
				} else {
					if (destinationPathExists)
						_parent.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Overwriting: " + destinationFilePath, false);
					destination = new System.IO.StreamWriter(destinationFilePath, false, this.Settings.TextFileEncoding);
				}
				formatter = new NonTransformFormatter();
			} else {
				if (this.Settings.LoadFileIsHtml) {
					formatter = new HtmlFileLongTextStreamFormatter(_settings, source);
				} else {
					formatter = new DelimitedFileLongTextStreamFormatter(_settings, source);
				}
				destination = _nativeFileWriter;
			}
			if (string.IsNullOrEmpty(downloadedTextFilePath) && (source != null) && source is System.IO.StreamReader && ((System.IO.StreamReader)source).BaseStream is System.IO.FileStream) {
				downloadedTextFilePath = ((System.IO.FileStream)((System.IO.StreamReader)source).BaseStream).Name;
			}
			if ((destination != null)) {
				this.WriteLongText(source, destination, formatter);
			}
			long retval = 0;
			if (destinationFilePath != string.Empty) {
				retval = kCura.Utility.File.Instance.GetFileSize(destinationFilePath);
				string textLocation = string.Empty;
				switch (this.Settings.TypeOfExportedFilePath) {
					case ExportFile.ExportedFilePathType.Absolute:
						textLocation = destinationFilePath;
						break;
					case ExportFile.ExportedFilePathType.Relative:
						textLocation = ".\\" + this.CurrentVolumeLabel + "\\" + this.CurrentFullTextSubdirectoryLabel + "\\" + artifact.FullTextFileName(this.NameTextFilesAfterIdentifier());
						break;
					case ExportFile.ExportedFilePathType.Prefix:
						textLocation = this.Settings.FilePrefix.TrimEnd('\\') + "\\" + this.CurrentVolumeLabel + "\\" + this.CurrentFullTextSubdirectoryLabel + "\\" + artifact.FullTextFileName(this.NameTextFilesAfterIdentifier());
						break;
				}
				if (Settings.LoadFileIsHtml) {
					_nativeFileWriter.Write("<a href='" + textLocation + "' target='_textwindow'>" + textLocation + "</a>");
				} else {
					_nativeFileWriter.Write(textLocation);
				}
			}
			_nativeFileWriter.Write(endBound);
			return retval;
		}

		private System.IO.TextReader GetLongTextStream(ObjectExportInfo artifact, Types.ViewFieldInfo field)
		{
			return new System.IO.StreamReader(this.DownloadTextFieldAsFile(artifact, field), this.GetLongTextFieldFileEncoding(field));
		}

		private System.IO.TextReader GetLongTextStream(string filename, Types.ViewFieldInfo field)
		{
			return new System.IO.StreamReader(filename, this.GetLongTextFieldFileEncoding(field));
		}

		private System.Text.Encoding GetLongTextFieldFileEncoding(Types.ViewFieldInfo field)
		{
			if (field.IsUnicodeEnabled)
				return System.Text.Encoding.Unicode;
			return System.Text.Encoding.Default;
		}


		public void UpdateLoadFile(object[] record, bool hasFullText, Int32 documentArtifactID, string nativeLocation, ref string fullTextTempFile, ObjectExportInfo doc, ref Int64 extractedTextByteCount)
		{
			if (_nativeFileWriter == null)
				return;
			Int32 count = default(Int32);
			string fieldValue = null;
			string columnName = null;
			string location = nativeLocation;
			string rowPrefix = _loadFileFormatter.RowPrefix;
			if (!string.IsNullOrEmpty(rowPrefix))
				_nativeFileWriter.Write(rowPrefix);
			for (count = 0; count <= _parent.Columns.Count - 1; count++) {
				Types.ViewFieldInfo field = (Types.ViewFieldInfo)_parent.Columns[count];
				columnName = field.AvfColumnName;
				object val = record[_ordinalLookup[columnName]];
				if (field.FieldType == FieldTypeHelper.FieldType.Text || field.FieldType == FieldTypeHelper.FieldType.OffTableText) {
					if (this.Settings.LoadFileIsHtml) {
						extractedTextByteCount += this.ManageLongText(val, field, ref fullTextTempFile, doc, "<td>", "</td>");
					} else {
						extractedTextByteCount += this.ManageLongText(val, field, ref fullTextTempFile, doc, _settings.QuoteDelimiter.ToString(), _settings.QuoteDelimiter.ToString());
					}
				} else {
					if (val is byte[])
						val = System.Text.Encoding.Unicode.GetString((byte[])val);
					if (field.FieldType == FieldTypeHelper.FieldType.Date && field.Category != FieldCategory.MultiReflected) {
						if (object.ReferenceEquals(val, System.DBNull.Value)) {
							val = string.Empty;
						} else if (val is System.DateTime) {
							val = ((System.DateTime)val).ToString(field.FormatString);
						}
						//If TypeOf val Is System.datete Then

						//End If
						//If Me.Settings.LoadFileIsHtml Then
						//	Dim datetime As String = kCura.Utility.NullableTypesHelper.DBNullString(val)
						//	If datetime Is Nothing OrElse datetime = "" Then
						//		val = ""
						//	Else
						//		val = System.DateTime.Parse(datetime, System.Globalization.CultureInfo.InvariantCulture).ToString(field.FormatString)
						//	End If
						//Else
						//	val = Me.ToExportableDateString(val, field.FormatString)
						//End If
					}
					fieldValue = kCura.Utility.NullableTypesHelper.ToEmptyStringOrValue(kCura.Utility.NullableTypesHelper.DBNullString(val));
					if (field.IsMultiValueField) {
						fieldValue = this.GetMultivalueString(fieldValue, field);
					} else if (field.IsCodeOrMulticodeField) {
						fieldValue = this.GetCodeValueString(fieldValue);
					}
					_nativeFileWriter.Write(_loadFileFormatter.TransformToCell(fieldValue));
				}
				if (!(count == _parent.Columns.Count - 1) && !this.Settings.LoadFileIsHtml) {
					_nativeFileWriter.Write(_settings.RecordDelimiter);
				}
			}
			string imagesCell = _loadFileFormatter.CreateImageCell(doc);
			if (!string.IsNullOrEmpty(imagesCell))
				_nativeFileWriter.Write(imagesCell);
			if (_settings.ExportNative) {
				if (this.Settings.VolumeInfo.CopyFilesFromRepository) {
					_nativeFileWriter.Write(_loadFileFormatter.CreateNativeCell(location, doc));
				} else {
					_nativeFileWriter.Write(_loadFileFormatter.CreateNativeCell(doc.NativeSourceLocation, doc));
				}
			}
			if (!string.IsNullOrEmpty(_loadFileFormatter.RowSuffix))
				_nativeFileWriter.Write(_loadFileFormatter.RowSuffix);
			_nativeFileWriter.Write(Microsoft.VisualBasic.Constants.vbNewLine);
		}

		private string ToExportableDateString(object val, string formatString)
		{
			string datetime = kCura.Utility.NullableTypesHelper.DBNullString(val);
			string retval = null;
			if (datetime == null || string.IsNullOrEmpty(datetime.Trim())) {
				retval = "";
			} else {
				retval = System.DateTime.Parse(datetime, System.Globalization.CultureInfo.InvariantCulture).ToString(formatString);
			}
			return retval;
		}

		private string GetCodeValueString(string input)
		{
			input = System.Web.HttpUtility.HtmlDecode(input);
			input = input.Trim(new char[] { Strings.ChrW(11) }).Replace(Strings.ChrW(11), _settings.MultiRecordDelimiter);
			return input;
		}

		private bool NameTextFilesAfterIdentifier()
		{
			if (this.Settings.TypeOfExport == ExportFile.ExportType.Production) {
				return _parent.ExportNativesToFileNamedFrom == ExportNativeWithFilenameFrom.Identifier;
			} else {
				return true;
			}
		}

		private string GetMultivalueString(string input, Types.ViewFieldInfo field)
		{
			string retVal = input;
			if (input.Contains("<objects>")) {
				System.Xml.XmlTextReader xr = new System.Xml.XmlTextReader(new System.IO.StringReader("<objects>" + input + "</objects>"));
				bool firstTimeThrough = true;
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				while (xr.Read()) {
					if (xr.Name == "object" & xr.IsStartElement()) {
						xr.Read();
						if (firstTimeThrough) {
							firstTimeThrough = false;
						} else {
							sb.Append(this.Settings.MultiRecordDelimiter);
						}
						string cleanval = xr.Value.Trim();
						switch (field.FieldType) {
							case FieldTypeHelper.FieldType.Code:
							case FieldTypeHelper.FieldType.MultiCode:
								cleanval = this.GetCodeValueString(cleanval);
								break;
							case FieldTypeHelper.FieldType.Date:
								cleanval = this.ToExportableDateString(cleanval, field.FormatString);
								break;
						}
						//If isCodeOrMulticodeField Then cleanval = Me.GetCodeValueString(cleanval)
						sb.Append(cleanval);
					}
				}
				xr.Close();
				retVal = sb.ToString();
			}
			return retVal;

		}

		public void UpdateVolume()
		{
			_currentVolumeSize = 0;
			_currentImageSubdirectorySize = 0;
			_currentNativeSubdirectorySize = 0;
			_currentTextSubdirectorySize = 0;
			_currentSubdirectoryNumber = _settings.VolumeInfo.SubdirectoryStartNumber;
			_currentVolumeNumber += 1;
		}

		public void UpdateSubdirectory()
		{
			_currentImageSubdirectorySize = 0;
			_currentNativeSubdirectorySize = 0;
			_currentTextSubdirectorySize = 0;
			_currentSubdirectoryNumber += 1;
		}

	}
}
