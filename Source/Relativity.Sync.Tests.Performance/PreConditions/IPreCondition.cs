namespace Relativity.Sync.Tests.Performance.PreConditions
{
    internal interface IPreCondition
    {
        string Name { get; }
        bool Check();
        FixResult TryFix();
    }
}
