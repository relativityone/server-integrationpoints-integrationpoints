namespace Relativity.Sync
{
    internal interface IProgressStateCounter
    {
        int Next();
        int GetOrderForGroup(string groupName);
    }
}
