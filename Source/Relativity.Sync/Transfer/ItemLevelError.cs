namespace Relativity.Sync.Transfer
{
    internal struct ItemLevelError
    {
        public ItemLevelError(string identifier, string message)
            : this()
        {
            Identifier = identifier;
            Message = message;
        }

        public string Identifier { get; }

        public string Message { get; }
    }
}
