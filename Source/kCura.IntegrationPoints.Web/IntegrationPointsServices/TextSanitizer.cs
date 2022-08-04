using System;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.IntegrationPointsServices
{
    public class TextSanitizer : ITextSanitizer
    {
        private readonly IStringSanitizer _internalSanitizer;

        public TextSanitizer(IStringSanitizer stringSanitizer)
        {
            _internalSanitizer = stringSanitizer ?? throw new ArgumentNullException(nameof(stringSanitizer));
        }

        public SanitizationResult Sanitize(string textToSanitize)
        {
            SanitizeHtmlContentResult sanitizationResult = _internalSanitizer.SanitizeHtmlContent(textToSanitize);

            bool hasAnyErrors = sanitizationResult.ErrorMessages != null
                                && sanitizationResult.ErrorMessages.Count > 0;
            return new SanitizationResult(
                sanitizedText: sanitizationResult.CleanHtml,
                hasErrors: hasAnyErrors);
        }
    }
}
