using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.Exports;
using kCura.Relativity.Export.FileObjects;
using Relativity;

namespace kCura.Relativity.Export.Exports
{

	public class HtmlCellFormatter : ILoadFileCellFormatter
	{
		private ExportFile _settings;
		private const string ROW_PREFIX = "<tr>";
		private const string ROW_SUFFIX = "</tr>";
		public HtmlCellFormatter(ExportFile settings)
		{
			_settings = settings;
		}

		public string TransformToCell(string contents)
		{
			contents = System.Web.HttpUtility.HtmlEncode(contents);
			return string.Format("{0}{1}{2}", "<td>", contents, "</td>");
		}

		private string GetNativeHtmlString(ObjectExportInfo artifact, string location)
		{
			if (_settings.ArtifactTypeID == (int)ArtifactType.Document && artifact.NativeCount == 0)
				return "";
			if (_settings.ArtifactTypeID != (int)ArtifactType.Document && !(artifact.FileID > 0))
				return "";
			System.Text.StringBuilder retval = new System.Text.StringBuilder();
			retval.AppendFormat("<a style='display:block' href='{0}'>{1}</a>", location, artifact.NativeFileName(_settings.AppendOriginalFileName));
			return retval.ToString();
		}

		public string RowPrefix {
			get { return ROW_PREFIX; }
		}

		public string RowSuffix {
			get { return ROW_SUFFIX; }
		}

		private string GetImagesHtmlString(ObjectExportInfo artifact)
		{
			if (artifact.Images.Count == 0)
				return "";
			System.Text.StringBuilder retval = new System.Text.StringBuilder();
			foreach (ImageExportInfo image in artifact.Images) {
				string loc = image.TempLocation;
				if (!_settings.VolumeInfo.CopyFilesFromRepository) {
					loc = image.SourceLocation;
				}
				retval.AppendFormat("<a style='display:block' href='{0}'>{1}</a>", loc, image.FileName);
				if (_settings.TypeOfImage == ExportFile.ImageType.MultiPageTiff || _settings.TypeOfImage == ExportFile.ImageType.Pdf)
					break; // TODO: might not be correct. Was : Exit For
			}
			return retval.ToString();
		}

		public string CreateImageCell(ObjectExportInfo artifact)
		{
			if (!_settings.ExportImages || _settings.ArtifactTypeID != (int)ArtifactType.Document)
				return string.Empty;
			return string.Format("<td>{0}</td>", this.GetImagesHtmlString(artifact));
		}

		public string CreateNativeCell(string location, ObjectExportInfo artifact)
		{
			return string.Format("<td>{0}</td>", this.GetNativeHtmlString(artifact, location));
		}
	}
}

