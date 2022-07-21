namespace Relativity.Sync.Transfer
{
    internal struct ItemLevelError
    {
        public string Identifier { get; }

        public string Message { get; }

        public ItemLevelError(string identifier, string message)
            : this()
        {
            Identifier = identifier;
            Message = message;
        }
    }
}
