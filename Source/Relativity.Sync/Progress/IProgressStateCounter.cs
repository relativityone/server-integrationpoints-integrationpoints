namespace Relativity.Sync.Progress
{
    internal interface IProgressStateCounter
    {
        int Next();

        int GetOrderForGroup(string groupName);
    }
}
