using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrThrowIfKeyExists<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key,
            TValue value,
            string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentException("You should provide a meaningful error message.");
            }

            if (dictionary.ContainsKey(key))
            {
                string exceptionMessage = $"{errorMessage}, key: {key}, value: {value}";
                var ex = new IntegrationPointsException(exceptionMessage);
                throw ex;
            }

            dictionary.Add(key, value);
        }
    }
}
