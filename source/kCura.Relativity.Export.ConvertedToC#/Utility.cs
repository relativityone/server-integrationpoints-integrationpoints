using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.Types;

namespace kCura.Relativity.Export
{
	public class Utility
	{
		public static DataTable BuildProxyCharacterDatatable()
		{
			Int32 i = default(Int32);
			ArrayList row = null;
			DataTable dt = null;
			dt = new DataTable();
			dt.Columns.Add("Display", typeof(string));
			dt.Columns.Add("CharValue", typeof(Int32));
			char rowValue = '\0';
			string rowDisplay = null;
			for (i = 1; i <= 255; i++) {
				row = new ArrayList();
				rowDisplay = string.Format("{0} (ASCII:{1})", Strings.ChrW(i), i.ToString().PadLeft(3, '0'));
				row.Add(rowDisplay);
				rowValue = Strings.ChrW(i);
				row.Add(rowValue);
				dt.Rows.Add(row.ToArray());
			}
			return dt;
		}

		public static string[] GetFieldNamesFromFieldArray(DocumentField[] documentFields)
		{
			Int32 i = default(Int32);
			string[] retval = new string[documentFields.Length];
			for (i = 0; i <= retval.Length - 1; i++) {
				retval[i] = documentFields[i].FieldName;
			}
			return retval;
		}

		public static string GetFilesystemSafeName(string input)
		{
			string output = string.Copy(input);
			output = output.Replace("/", " ");
			output = output.Replace(":", " ");
			output = output.Replace("?", " ");
			output = output.Replace("*", " ");
			output = output.Replace("<", " ");
			output = output.Replace(">", " ");
			output = output.Replace("|", " ");
			output = output.Replace("\\", " ");
			output = output.Replace("\"", " ");
			return output;
		}

		/// <summary>
		/// Attempts to determine the encoding for a file by detecting the Byte Order Mark (BOM).
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="returnEncodingOnly">if set to <c>true</c> [return encoding only].</param>
		/// <param name="performFileExistsCheck">if set to <c>true</c> [perform file exists check].</param>
		/// <returns>
		/// Returns System.Text.Encoding.UTF8, Unicode, or BigEndianUnicode if the BOM is found and Nothing otherwise.
		/// </returns>
		public static DeterminedEncodingStream DetectEncoding(string filename, bool returnEncodingOnly, bool performFileExistsCheck)
		{
			System.Text.Encoding enc = null;
			System.IO.FileStream filein = null;
			if (!performFileExistsCheck || (performFileExistsCheck && System.IO.File.Exists(filename))) {
				filein = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				if ((filein.CanSeek)) {
					byte[] bom = new byte[5];
					filein.Read(bom, 0, 4);
					//EF BB BF       = Unicode (UTF-8)
					//FF FE          = ucs-2le, ucs-4le, and ucs-16le OR Unicode
					//FE FF          = utf-16 and ucs-2 OR Unicode (Big-Endian)
					//00 00 FE FF    = ucs-4 OR Unicode (UTF-32 Big-Endian)  NOT SUPPORTING THIS
					//FF FE 00 00		= Unicode (UTF-32) NOT SUPPORTING THIS
					if ((((bom[0] == 0xef) & (bom[1] == 0xbb) & (bom[2] == 0xbf)))) {
						enc = System.Text.Encoding.UTF8;
					}
					if (((bom[0] == 0xff) & (bom[1] == 0xfe))) {
						enc = System.Text.Encoding.Unicode;
					}
					if (((bom[0] == 0xfe) & (bom[1] == 0xff))) {
						enc = System.Text.Encoding.BigEndianUnicode;
					}
					if ((bom[0] == 0x0 & bom[1] == 0x0 & bom[2] == 0xfe & bom[3] == 0xff)) {
						//enc = System.Text.Encoding.GetEncoding(12001)	' Unicode (UTF-32 Big-Endian)
					}
					if ((bom[0] == 0xff & bom[1] == 0xfe & bom[2] == 0x0 & bom[3] == 0x0)) {
						//enc = System.Text.Encoding.GetEncoding(12000)	'Unicode (UTF-32)
					}

					//Position the file cursor back to the start of the file
					filein.Seek(0, System.IO.SeekOrigin.Begin);
				}
				if (returnEncodingOnly) {
					filein.Close();
				}
			}
			if (returnEncodingOnly) {
				return new DeterminedEncodingStream(enc);
			} else {
				return new DeterminedEncodingStream(filein, enc);
			}
		}

		/// <summary>
		/// Attempts to determine the encoding for a file by detecting the Byte Order Mark (BOM).
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="returnEncodingOnly">if set to <c>true</c> [return encoding only].</param>
		/// <returns>
		/// Returns System.Text.Encoding.UTF8, Unicode, or BigEndianUnicode if the BOM is found and Nothing otherwise.
		/// </returns>
		public static DeterminedEncodingStream DetectEncoding(string filename, bool returnEncodingOnly)
		{
			return DetectEncoding(filename, returnEncodingOnly, true);
		}
	}

	public class DeterminedEncodingStream
	{
		private System.IO.FileStream _fileStream;

		private System.Text.Encoding _determinedEncoding;
		public System.IO.Stream UnderlyingStream {
			get { return _fileStream; }
		}

		public System.Text.Encoding DeterminedEncoding {
			get { return _determinedEncoding; }
		}

		public DeterminedEncodingStream(System.IO.FileStream fileStream, System.Text.Encoding determinedEncoding)
		{
			_fileStream = fileStream;
			_determinedEncoding = determinedEncoding;
		}

		public DeterminedEncodingStream(System.Text.Encoding determinedEncoding)
		{
			_determinedEncoding = determinedEncoding;
		}

		public void Close()
		{
			try {
				if ((_fileStream != null))
					_fileStream.Close();
			} catch {
			}
		}

	}
}
