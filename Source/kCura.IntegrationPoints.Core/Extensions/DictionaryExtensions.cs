using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Extensions
{
	public static class DictionaryExtensions
	{
		public static void AddAndFailJobIfKeyExists<TKey, TValue>(
			this Dictionary<TKey, TValue> dictionary,
			TKey key,
			TValue value,
			string errorMessage,
			IAPILog logger)
		{
			if (dictionary.ContainsKey(key))
			{
				string exceptionMessage = $"{errorMessage}, key: {key}, value: {value}";
				string logMessageTemplate = $"{errorMessage}, key: @{{{nameof(key)}}}, value: @{{{nameof(value)}}}";
				var ex = new IntegrationPointsException(exceptionMessage);
				logger.LogError(ex, logMessageTemplate, key, value);
				throw ex;
			}

			dictionary.Add(key, value);
		}
	}
}
