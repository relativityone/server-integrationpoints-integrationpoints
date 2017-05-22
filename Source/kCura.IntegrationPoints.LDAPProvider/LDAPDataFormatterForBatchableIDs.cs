using System;
using System.Text;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider
{
	public class LDAPDataFormatterForBatchableIDs : LDAPDataFormatterDefault
	{
		public LDAPDataFormatterForBatchableIDs(LDAPSettings settings, IHelper helper)
			: base(settings, helper)
		{ }

		public override object ConvertByteArray(byte[] value)
		{
			var bString = new StringBuilder();
			foreach (byte b in value)
			{
				bString.Append($"\\{Microsoft.VisualBasic.Conversion.Hex(b).PadLeft(2, '0')}");
			}
			return bString.ToString();
		}
	}
}
