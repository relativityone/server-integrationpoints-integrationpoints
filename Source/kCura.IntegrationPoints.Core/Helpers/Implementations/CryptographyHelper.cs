using System.Text;
using System.Security.Cryptography;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class CryptographyHelper : ICryptographyHelper
    {
        private const string _HEXADECIMAL_FORMAT = "x2";

        public string CalculateHash(string value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hashBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                StringBuilder builder = new StringBuilder();

                foreach (byte hashByte in hashBytes)
                {
                    builder.Append(hashByte.ToString(_HEXADECIMAL_FORMAT));
                }

                return builder.ToString();
            }
        }
    }
}
