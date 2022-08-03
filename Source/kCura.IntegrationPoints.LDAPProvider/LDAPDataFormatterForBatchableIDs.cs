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
            var bString = new StringBuilder(value.Length * 3);
            foreach (byte b in value)
            {
                bString.AppendFormat("\\{0:x2}", b);
            }
            return bString.ToString();
        }
    }
}
