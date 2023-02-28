using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public static class ExportApiDataHelper
    {
        private const string MultiObjectParsingError = "Encountered an error while processing multi-object field, this may due to out-of-date version of the software. Please contact administrator for more information.";
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
                value = value.Replace("&#x0B;", string.Empty);
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
                    string tempValue = string.Format("<{0}>{1}</{0}>", TempRootGuid, value);

                    using (XmlReader reader = XmlReader.Create(new StringReader(tempValue)))
                    {
                        XmlSerializerRoot data = MultiObjectFieldSerializer.Value.Deserialize(reader) as XmlSerializerRoot;
                        if (data.Object.Length == 1)
                        {
                            value = data.Object[0];
                        }
                        else
                        {
                            value = string.Join(IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER.ToString(), data.Object);
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

    }
}
