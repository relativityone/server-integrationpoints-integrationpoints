using Relativity.Import.V1;

namespace Relativity.Sync.Extensions
{
    internal static class ResponseExtensions
    {
        public static void Validate(this Response response, string customMessage = "")
        {
            if (!response.IsSuccess)
            {
                string message = $"{customMessage} ImportJobId: {response.ImportJobID}, Error code: {response.ErrorCode}, message: {response.ErrorMessage}";
                throw new SyncException(message);
            }
        }

        public static T UnwrapOrThrow<T>(this ValueResponse<T> response, string customMessage = "")
        {
            response.Validate(customMessage);

            return response.Value;
        }
    }
}
