namespace Relativity.Sync.WorkspaceGenerator.FileGenerating.SizeCalculator
{
    public interface IFileSizeCalculatorStrategy
    {
        long GetNext();
    }
}