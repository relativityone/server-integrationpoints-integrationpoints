namespace kCura.IntegrationPoints.Core.Interfaces.TextSanitizer
{
    public class SanitizationResult
    {
        public string SanitizedText { get; }

        public bool HasErrors { get; }

        public SanitizationResult(string sanitizedText, bool hasErrors)
        {
            SanitizedText = sanitizedText;
            HasErrors = hasErrors;
        }
    }
}
