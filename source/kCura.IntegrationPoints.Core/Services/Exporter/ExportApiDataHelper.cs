using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using kCura.EDDS.DocumentCompareGateway;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Data;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public static class ExportApiDataHelper
	{
		private const String MultiObjectParsingError = "Encounter error while processing multi-object field, this may due to out-of-date version of the software. Please contact administrator for more information.";
		private static readonly Lazy<XmlSerializer> MultiObjectFieldSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(XmlSerializerRoot)));

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
		/// <returns>a string contains the text identifier represent multi-object data, seperating object by ';'</returns>
		public static object SanitizeMultiObjectField(object rawValue)
		{
			try
			{
				string value = rawValue as string;
				if (String.IsNullOrEmpty(value) == false)
				{
					string tempValue = "<e7913be0-cd0a-4833-a432-f2d67a2f1349>" + value + "</e7913be0-cd0a-4833-a432-f2d67a2f1349>";

					using (XmlReader reader = XmlReader.Create(new StringReader(tempValue)))
					{
						XmlSerializerRoot data = MultiObjectFieldSerializer.Value.Deserialize(reader) as XmlSerializerRoot;
						if (data.Object.Length == 1)
						{
							value = data.Object[0];
						}
						else
						{
							value = String.Join(IntegrationPoints.Contracts.Constants.MULTI_VALUE_DEIMITER.ToString(), data.Object);
						}
					}
					return value;
				}
				else
				{
					return rawValue;
				}
			}
			catch (Exception exception)
			{
				Exception ex = new Exception(MultiObjectParsingError, exception);
				throw ex;
			}
		}

		// used to formatize multi object field data
		[XmlRoot("e7913be0-cd0a-4833-a432-f2d67a2f1349")]
		public class XmlSerializerRoot
		{
			[XmlElement("object")]
			public String[] Object { get; set; }
		}

		public static async Task<string> RetrieveLongTextFieldAsync(BaseServiceContext context, DataGridContext dgContext, int documentArtifactId, int caseId, int fieldArtifactId)
		{
			const int bufferSize = 4016;
			return await Task.Run(() =>
			{
				StringBuilder strBuilder = null;
				using (ILongTextStream stream = new LongTextStream(context, documentArtifactId, caseId, dgContext, fieldArtifactId))
				{
					strBuilder  = new StringBuilder((int)stream.Length);
					byte[] buffer = new byte[bufferSize];
					while (stream.Read(buffer, 0, buffer.Length) != 0)
					{
						strBuilder.Append(Encoding.UTF8.GetString(buffer));
						buffer = new byte[bufferSize];
					}
				}
				return strBuilder.ToString();
			});
		}
	}
}