namespace kCura.IntegrationPoints.Core.Services.Keywords
{
    public interface IKeyword
    {
        string KeywordName { get; }

        string Convert();

    }
}
