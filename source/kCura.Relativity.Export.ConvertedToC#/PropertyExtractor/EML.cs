using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace PropertyExtractor
{
	public class EML
	{

		public System.Collections.Hashtable Extract(string fileName)
		{

			System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
			string lineBuffer = null;
			string header = null;
			string headerValue = null;

			System.Text.StringBuilder body = new System.Text.StringBuilder();
			bool inBody = false;

			System.Collections.Hashtable hashTable = new System.Collections.Hashtable();
			hashTable.Add("File Type".ToLower(), "Email Message");

			lineBuffer = sr.ReadLine();


			while (!(lineBuffer == null)) {

				if (!inBody) {
					if (!string.IsNullOrEmpty(lineBuffer)) {
						if (!string.IsNullOrEmpty(this.ExtractSMTPHeader(lineBuffer))) {
							if (this.IsValidSMTPHeader(this.ExtractSMTPHeader(lineBuffer))) {
								header = this.ExtractSMTPHeader(lineBuffer);
								if (header.ToLower() == "date") {
									headerValue = lineBuffer.Substring(lineBuffer.IndexOf(":") + 1, lineBuffer.IndexOf("-") - lineBuffer.IndexOf(":") - 2);
									try {
										System.DateTime dt = System.DateTime.Parse(headerValue);
									} catch (System.Exception ex) {
										// if the field cannot be parsed then set it as empty
										headerValue = string.Empty;
									}
								} else {
									headerValue = lineBuffer.Substring(lineBuffer.IndexOf(":") + 1);
								}

							} else {
								headerValue = headerValue + lineBuffer;
							}
						} else {
							headerValue = headerValue + lineBuffer;
						}

						if (hashTable.Contains(header.ToLower())) {
							hashTable[header.ToLower()] = headerValue;
						} else {
							hashTable.Add(header.ToLower(), headerValue);
						}

					} else {
						inBody = true;
					}
				} else {
					body.Append(lineBuffer);
					body.Append(Constants.vbCrLf);
				}

				lineBuffer = sr.ReadLine();
			}
			sr.Close();

			hashTable.Add("body", body.ToString());

			return hashTable;

		}

		private string ExtractSMTPHeader(string lineBuffer)
		{
			if (lineBuffer.IndexOf(":") > -1) {
				return lineBuffer.Substring(0, lineBuffer.IndexOf(":"));
			}
			return null;
		}

		private bool IsValidSMTPHeader(string header)
		{
			switch (header.ToLower()) {
				case "to":
				case "bcc":
				case "cc":
				case "message-id":
				case "x-filename":
				case "x-origin":
				case "x-bcc":
				case "from":
				case "x-to":
				case "x-from":
				case "content-type":
				case "content-transfer-Encoding":
				case "x-cc":
				case "x-folder":
				case "mime-version":
				case "subject":
				case "date":
					return true;
				default:
					return false;
			}
		}

	}
}
