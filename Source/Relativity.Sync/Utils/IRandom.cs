namespace Relativity.Sync.Utils
{
    internal interface IRandom
    {
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
    }
}