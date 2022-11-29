using Relativity.Import.V1;

namespace Relativity.Sync.Extensions
{
    internal static class ResponseExtensions
    {
        public static void Validate(this Response response)
        {
            if (!response.IsSuccess)
            {
                string message = $"ImportJobId: {response.ImportJobID}, Error code: {response.ErrorCode}, message: {response.ErrorMessage}";
                throw new SyncException(message);
            }
        }

        public static T UnwrapOrThrow<T>(this ValueResponse<T> response)
        {
            response.Validate();

            return response.Value;
        }
    }
}
