
namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
    public static class SecureStringExtension
    {
        public static string ToPlainString(this System.Security.SecureString secureStr)
        {
            return new System.Net.NetworkCredential(string.Empty, secureStr).Password;
        }
    }
}
