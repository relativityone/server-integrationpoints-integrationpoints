using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
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

        public static Dictionary<TKey, TValue> AddDictionary<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            Dictionary<TKey, TValue> inputDictionary)
        {
            if (inputDictionary.IsNullOrEmpty())
            {
                return dictionary;
            }

            foreach (var pair in inputDictionary)
            {
                dictionary.AddOrThrowIfKeyExists(pair.Key, pair.Value,"Value for key already exists");
            }

            return dictionary;
        }
    }
}
