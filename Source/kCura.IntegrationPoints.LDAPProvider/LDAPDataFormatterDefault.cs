using System;
using System.DirectoryServices;
using System.Text;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LDAPDataFormatterDefault : ILDAPDataFormatter
    {
        internal LDAPSettings _settings;
        private readonly IAPILog _logger;

        public LDAPDataFormatterDefault(LDAPSettings settings, IHelper helper)
        {
            _settings = settings;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<LDAPDataFormatterDefault>();
        }

        public object FormatData(object initialData)
        {
            if (!(initialData is ResultPropertyValueCollection))
            {
                string message = $"Unsupported data type: {initialData?.GetType().FullName}.";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }
            string returnValue = null;
            foreach (object item in (ResultPropertyValueCollection) initialData)
            {
                object dataValue = ConvertData(item);
                if (_settings.MultiValueDelimiter.HasValue)
                {
                    if (!string.IsNullOrEmpty(returnValue))
                    {
                        returnValue += _settings.MultiValueDelimiter.ToString();
                    }
                    returnValue += dataValue;
                }
                else
                {
                    var message = "LDAPSettings.MultiValueDelimiter has no value.";
                    _logger.LogError(message);
                    throw new InvalidOperationException(message);
                }
            }
            return returnValue;
        }

        public virtual object ConvertData(object value)
        {
            if (value == null)
            {
                return null;
            }
            
            if (value is byte[])
            {
                return ConvertByteArray((byte[])value);
            }
            if (value is DateTime)
            {
                return ConvertDate((DateTime)value);
            }
            return value.ToString();
        }

        public virtual object ConvertByteArray(byte[] value)
        {
            var bString = new StringBuilder(value.Length * 2);
            foreach (byte b in value)
            {
                bString.AppendFormat("{0:x2}", b);
            }
            return bString.ToString();
        }

        public virtual object ConvertDate(DateTime value)
        {
            return value.ToString("s");
        }
    }
}
