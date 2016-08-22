using System;
using System.Text;

namespace kCura.IntegrationPoints.LDAPProvider
{
	public class LDAPDataFormatterForBatchableIDs : LDAPDataFormatterDefault
	{
		public LDAPDataFormatterForBatchableIDs(LDAPSettings settings)
			: base(settings)
		{ }

		public override object ConvertByteArray(Byte[] value)
		{
			StringBuilder bString = new StringBuilder();
			foreach (Byte b in ((Byte[])value))
			{
				bString.Append(string.Format("\\{0}", Microsoft.VisualBasic.Conversion.Hex(b).ToString().PadLeft(2, '0')));
			}
			return bString.ToString();
		}
	}
}
