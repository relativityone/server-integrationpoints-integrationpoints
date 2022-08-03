namespace kCura.IntegrationPoints.Email.Dto
{
    internal class SmtpConfigurationDto
    {
        public string Domain { get; }
        public uint? Port { get; }
        public string UserName { get; }
        public string Password { get; }
        public bool? UseSSL { get; }
        public string EmailFromAddress { get; }

        public SmtpConfigurationDto(
            string domain,
            uint? port,
            bool? useSsl,
            string userName,
            string password,
            string emailFromAddress)
        {
            Domain = domain;
            Port = port;
            UseSSL = useSsl;
            UserName = userName;
            Password = password;
            EmailFromAddress = emailFromAddress;
        }
    }
}