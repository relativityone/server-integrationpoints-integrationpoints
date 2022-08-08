using System.DirectoryServices;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public enum AuthenticationTypesEnum
    {
        Anonymous = AuthenticationTypes.Anonymous,
        Delegation = AuthenticationTypes.Delegation,
        Encryption = AuthenticationTypes.Encryption,
        FastBind = AuthenticationTypes.FastBind,
        None = AuthenticationTypes.None,
        ReadonlyServer = AuthenticationTypes.ReadonlyServer,
        Sealing = AuthenticationTypes.Sealing,
        Secure = AuthenticationTypes.Secure,
        SecureSocketsLayer = AuthenticationTypes.SecureSocketsLayer,
        ServerBind = AuthenticationTypes.ServerBind,
        Signing = AuthenticationTypes.Signing
    }
}
