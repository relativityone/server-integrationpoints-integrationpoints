using System;
using System.DirectoryServices;
using System.Text;

namespace kCura.IntegrationPoints.LDAPProvider
{
	public class LDAPDataFormatterDefault : ILDAPDataFormatter
	{
		internal LDAPSettings _settings;
		public LDAPDataFormatterDefault(LDAPSettings settings)
		{
			_settings = settings;
		}

		public object FormatData(object initialData)
		{
		    if (!(initialData is ResultPropertyValueCollection))
		    {
		        throw new InvalidOperationException($"Unsupported data type: {initialData?.GetType().FullName}.");
            }
		    string returnValue = null;
		    foreach (object item in (ResultPropertyValueCollection) initialData)
		    {
		        object dataValue = ConvertData(item);
		        if (_settings.MultiValueDelimiter.HasValue)
		        {
		            if (!string.IsNullOrEmpty(returnValue)) returnValue += _settings.MultiValueDelimiter.ToString();
		            returnValue += dataValue;
		        }
		        else
		        {
		            throw new InvalidOperationException("LDAPSettings.MultiValueDelimiter has no value.");
		        }
		    }
		    return returnValue;
		}

		public virtual object ConvertData(object value)
		{
		    if (value == null) return null;
            
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
			var bString = new StringBuilder();
			foreach (byte b in value)
			{
				bString.Append(Microsoft.VisualBasic.Conversion.Hex(b).PadLeft(2, '0'));
			}
			return bString.ToString();
		}

		public virtual object ConvertDate(DateTime value)
		{
			return value.ToString("s");
		}
	}
}
