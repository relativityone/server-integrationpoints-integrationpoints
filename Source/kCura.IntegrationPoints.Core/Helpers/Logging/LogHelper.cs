using System;
using kCura.IntegrationPoints.Domain.Logging;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Helpers.Logging
{
    public static class LogHelper
    {
        public static T GetValueAndLogEx<T>(Func<T> valueGetter, string errorMessage, IAPILog logger)
        {
            try
            {
                return valueGetter();
            }
            catch (Exception e)
            {
                logger?.LogError(e, errorMessage);
                throw new CorrelationContextCreationException(errorMessage, e);
            }
        }
    }
}
