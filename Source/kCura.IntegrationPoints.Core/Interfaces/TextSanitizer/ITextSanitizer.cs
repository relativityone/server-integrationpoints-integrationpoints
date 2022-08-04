namespace kCura.IntegrationPoints.Core.Interfaces.TextSanitizer
{
    public interface ITextSanitizer
    {
        SanitizationResult Sanitize(string textToSanitize);
    }
}
