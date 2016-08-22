using System;
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
			string returnValue = null;
			if (initialData != null)
			{
				if (initialData is System.DirectoryServices.ResultPropertyValueCollection)
				{
					foreach (var item in initialData as System.DirectoryServices.ResultPropertyValueCollection)
					{
						object dataValue = ConvertData(item);
						if (_settings.MultiValueDelimiter.HasValue)
						{
							if (!string.IsNullOrEmpty(returnValue)) returnValue += _settings.MultiValueDelimiter.ToString();
							returnValue += dataValue;
						}
						else
						{
							//TODO: determine what we want to do if no delimiter specified,
						}
					}
				}
			}
			return returnValue;
		}

		public virtual object ConvertData(object value)
		{
			if (value != null)
			{
				//if (value.GetType() == Type.GetType("System.Byte[]"))
				if (value is System.Byte[])
				{
					return ConvertByteArray((Byte[])value);
				}
				else if (value is System.DateTime)
				{
					return ConvertDate((DateTime)value);
				}
				else
				{
					return value.ToString();
				}
			}
			return value;
		}

		public virtual object ConvertByteArray(Byte[] value)
		{
			StringBuilder bString = new StringBuilder();
			foreach (Byte b in (value))
			{
				bString.Append(Microsoft.VisualBasic.Conversion.Hex(b).ToString().PadLeft(2, '0'));
			}
			return bString.ToString();
		}
		public virtual object ConvertDate(DateTime value)
		{
			return value.ToString("s");
		}
	}
}
