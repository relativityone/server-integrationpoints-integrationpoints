namespace kCura.IntegrationPoints.Data.QueryOptions
{
    public class IntegrationPointQueryOptions
    {
        public bool Decrypt { get; }
        public bool FieldMapping { get; }

        private IntegrationPointQueryOptions(bool decrypt, bool withFieldMapping)
        {
            Decrypt = decrypt;
            FieldMapping = withFieldMapping;
        }

        public static IntegrationPointQueryOptions All()
        {
            return new IntegrationPointQueryOptions(decrypt: false, withFieldMapping: false);
        }

        public IntegrationPointQueryOptions Decrypted()
        {
            return new IntegrationPointQueryOptions(decrypt: true, withFieldMapping: FieldMapping);
        }

        public IntegrationPointQueryOptions WithFieldMapping()
        {
            return new IntegrationPointQueryOptions(Decrypt, withFieldMapping: true);
        }
    }
}
