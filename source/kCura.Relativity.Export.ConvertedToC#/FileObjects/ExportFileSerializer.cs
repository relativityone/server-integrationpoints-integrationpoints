using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using kCura.Relativity.Export.Types;
using Relativity;

namespace kCura.Relativity.Export.FileObjects
{
	public class ExportFileSerializer
	{
		private ExportSettingsValidator _settingsValidator = new ExportSettingsValidator();
		public ExportSettingsValidator SettingsValidator {
			get {
				if (_settingsValidator == null)
					_settingsValidator = new ExportSettingsValidator();
				return _settingsValidator;
			}
			set { _settingsValidator = value; }
		}

		public virtual string TransformExportFileXml(XDocument input)
		{
			return input.ToString();
		}

		public virtual ExportFile DeserializeExportFile(ExportFile currentExportFile, string xml)
		{
			ExportFile retval = new ExportFile(currentExportFile.ArtifactTypeID);
			ExportFile deserialized = this.DeserializeExportFile(XDocument.Parse(xml));
			foreach (System.Reflection.PropertyInfo p in retval.GetType().GetProperties().Where(prop => prop.CanWrite)) {
				p.SetValue(retval, p.GetValue(PropertyIsReadFromExisting(p) ? currentExportFile : deserialized, null), null);
			}
			//TODO: test
			switch (retval.TypeOfExport) {
				case ExportFile.ExportType.AncestorSearch:
				case ExportFile.ExportType.ParentSearch:
					retval.ArtifactID = currentExportFile.ArtifactID;
					break;
				case ExportFile.ExportType.Production:
					retval.ImagePrecedence = new List<Pair>();
					break;
			}
			if (!SqlNameHelper.GetSqlFriendlyName(currentExportFile.ObjectTypeName).Equals(SqlNameHelper.GetSqlFriendlyName(retval.ObjectTypeName))) {
				retval = new ErrorExportFile("Cannot load '" + currentExportFile.ObjectTypeName + "' settings from a saved '" + retval.ObjectTypeName + "' export");
			}
			if (!this.SettingsValidator.IsValidExportDirectory(retval.FolderPath))
				retval.FolderPath = string.Empty;
			return retval;
		}

		private bool PropertyIsReadFromExisting(System.Reflection.PropertyInfo p)
		{
			foreach (Attribute att in p.GetCustomAttributes(typeof(ReadFromExisting), false)) {
				return true;
			}
			return false;
		}

		public virtual ExportFile DeserializeExportFile(XDocument xml)
		{
			System.Runtime.Serialization.Formatters.Soap.SoapFormatter deserializer = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
			string cleansedInput = this.TransformExportFileXml(xml);
			System.IO.MemoryStream sr = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(cleansedInput));
			ExportFile deserialized = null;
			try {
				deserialized = (ExportFile)deserializer.Deserialize(sr);
			} catch {
				throw;
			} finally {
				sr.Close();
			}
			return deserialized;
		}

		public class ExportSettingsValidator
		{
			public virtual bool IsValidExportDirectory(string path)
			{
				return System.IO.Directory.Exists(path);
			}
		}
	}
}

