namespace kCura.IntegrationPoints.Web.Models.Validation
{
    public class ValidationErrorDTO
    {
        public string Code { get; }
        public string Message { get; }
        public string HelpUrl { get; }

        public ValidationErrorDTO(string code, string message, string helpUrl)
        {
            Code = code;
            Message = message;
            HelpUrl = helpUrl;
        }
    }
}