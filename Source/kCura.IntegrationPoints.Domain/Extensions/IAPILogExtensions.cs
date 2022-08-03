using System;
using System.Reflection;
using Relativity.API;

namespace kCura.IntegrationPoints.Domain.Extensions
{
    public static class IAPILogExtensions
    {
        private const string _RIP_LOG_PREFIX = "RIP.";

        /// <summary>
        /// This method pushes all public properties from given object to logger LogContext
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDisposable LogContextPushProperties(this IAPILog logger, object obj)
        {
            var stackOfDisposables = new StackOfDisposables();

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                string propertyValue = property.GetValue(obj, null)?.ToString() ?? string.Empty;

                IDisposable disposable = logger.LogContextPushProperty($"{_RIP_LOG_PREFIX}{propertyName}", propertyValue);
                stackOfDisposables.Push(disposable);
            }
            return stackOfDisposables;
        }
    }
}
