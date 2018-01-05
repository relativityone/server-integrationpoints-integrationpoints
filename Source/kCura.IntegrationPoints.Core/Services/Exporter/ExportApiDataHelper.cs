using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using kCura.EDDS.DocumentCompareGateway;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Data;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public static class ExportApiDataHelper
	{
		private const String MultiObjectParsingError = "Encountered an error while processing multi-object field, this may due to out-of-date version of the software. Please contact administrator for more information.";
		private static readonly Lazy<XmlSerializer> MultiObjectFieldSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(XmlSerializerRoot)));
		private const string TempRootGuid = "e7913be0-cd0a-4833-a432-f2d67a2f1349";

		/// <summary>
		/// Sanitize single choice string that comes from export api.
		/// </summary>
		/// <param name="rawValue">an untyped object from export api</param>
		/// <returns>a string contains the text identifier represent single choice data</returns>
		public static object SanitizeSingleChoiceField(object rawValue)
		{
			string value = rawValue as string;
			if (String.IsNullOrEmpty(value) == false)
			{
				StringBuilder builder = new StringBuilder(value.Length);
				for (int index = 0; index < value.Length; index++)
				{
					char tmp = value[index];
					if (tmp != '\v')
					{
						builder.Append(tmp);
					}
				}
				value = builder.ToString();
				value = value.Replace("&#x0B;", String.Empty);
				return value;
			}
			return rawValue;
		}

		/// <summary>
		/// Sanitize multi-object string that comes from export api.
		/// </summary>
		/// <param name="rawValue">an untyped object from export api</param>
		/// <returns>a string contains the text identifier represent multi-object data, separating object by ';'</returns>
		public static object SanitizeMultiObjectField(object rawValue)
		{
			try
			{
				string value = rawValue as string;
				if (String.IsNullOrEmpty(value) == false)
				{
					string tempValue = String.Format("<{0}>{1}</{0}>", TempRootGuid, value);

					using (XmlReader reader = XmlReader.Create(new StringReader(tempValue)))
					{
						XmlSerializerRoot data = MultiObjectFieldSerializer.Value.Deserialize(reader) as XmlSerializerRoot;
						if (data.Object.Length == 1)
						{
							value = data.Object[0];
						}
						else
						{
							value = String.Join(IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER.ToString(), data.Object);
						}
					}
					return value;
				}
				return rawValue;
			}
			catch (Exception exception)
			{
				Exception ex = new Exception(MultiObjectParsingError, exception);
				throw ex;
			}
		}

		// used to format multi object field data
		[XmlRoot(TempRootGuid)]
		public class XmlSerializerRoot
		{
			[XmlElement("object")]
			public String[] Object { get; set; }
		}

		public class RelativityLongTextStreamFactory : IILongTextStreamFactory
		{
			private readonly BaseServiceContext _context;
			private readonly DataGridContext _dataGridContext;
			private readonly int _caseId;

			public RelativityLongTextStreamFactory(BaseServiceContext context, DataGridContext dgContext, int caseId)
			{
				_context = context;
				_dataGridContext = dgContext;
				_caseId = caseId;
			}

			public ILongTextStream CreateLongTextStream(int documentArtifactId, int fieldArtifactId)
			{
				return new LongTextStream(_context, documentArtifactId, _caseId, _dataGridContext, fieldArtifactId);
			}
		}

		public static async Task<string> RetrieveLongTextFieldAsync(IILongTextStreamFactory longTextStreamFactory,
			int documentArtifactId, int fieldArtifactId, IAPILog logger)
		{
			const int bufferSize = 4016;
			return await Task.Run(() =>
			{
				long textSize = 0;
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				StringBuilder strBuilder = null;
				using (ILongTextStream stream = longTextStreamFactory.CreateLongTextStream(documentArtifactId, fieldArtifactId))
				{
					Encoding encoding = stream.IsUnicode ? Encoding.Unicode : Encoding.ASCII;
					strBuilder = new StringBuilder();
					byte[] buffer = new byte[bufferSize];
					int read;
					while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
					{
						textSize += read;
						strBuilder.Append(encoding.GetString(buffer, 0, read));
						buffer = new byte[bufferSize];
					}
				}
				string result = strBuilder.ToString();

				stopwatch.Stop();
				long textInMB = textSize / (1024 * 1024);
				double timeInS = Math.Round(stopwatch.Elapsed.TotalSeconds, 1);
				logger.LogInformation(
					"Retrieving of Long Text field exceeding limit finished.  Real Long Text size: {longTextFieldLength}MB; Load time: {fieldRetrievalTime}s; Transfer rate: {transfer}MB/s",
					textInMB, timeInS, Math.Round(textInMB / timeInS, 2));
				logger.LogVerbose("Document ArtifactId: {documentArtifactId}. Field ArtifactId: {fieldArtifactId}.", documentArtifactId, fieldArtifactId);

				return result;

			});
		}
	}
}