namespace kCura.IntegrationPoints.Data.QueryOptions
{
    public class IntegrationPointQueryOptions
    {
        private IntegrationPointQueryOptions(
            bool decrypt,
            bool withFieldMapping,
            bool withConfiguration)
        {
            Decrypt = decrypt;
            FieldMapping = withFieldMapping;
            Configuration = withConfiguration;
        }

        public bool Decrypt { get; }

        public bool FieldMapping { get; }

        public bool Configuration { get; }

        public static IntegrationPointQueryOptions All()
        {
            return new IntegrationPointQueryOptions(
                decrypt: false, withFieldMapping: false, withConfiguration: false);
        }

        public IntegrationPointQueryOptions Decrypted()
        {
            return new IntegrationPointQueryOptions(
                decrypt: true, withFieldMapping: FieldMapping, withConfiguration: Configuration);
        }

        public IntegrationPointQueryOptions WithFieldMapping()
        {
            return new IntegrationPointQueryOptions(
                decrypt: Decrypt, withFieldMapping: true, withConfiguration: Configuration);
        }

        public IntegrationPointQueryOptions WithConfiguration()
        {
            return new IntegrationPointQueryOptions(
                decrypt: Decrypt, withFieldMapping: FieldMapping, withConfiguration: true);
        }
    }
}
