namespace kCura.IntegrationPoints.Email.Dto
{
    internal class SmtpClientSettings
    {
        public string Domain { get; }
        public int Port { get; }
        public string UserName { get; }
        public string Password { get; }
        public bool UseSSL { get; }

        public SmtpClientSettings(
            string domain,
            int port,
            bool useSsl,
            string userName,
            string password)
        {
            Domain = domain;
            Port = port;
            UseSSL = useSsl;
            UserName = userName;
            Password = password;
        }
    }
}
