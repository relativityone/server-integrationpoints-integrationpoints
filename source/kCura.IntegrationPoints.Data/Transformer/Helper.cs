using System;
using System.Collections.Generic;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data.Transformer
{
	public static class Helper
	{
		/// <summary>
		/// Converts the Choice to predefined choice value(s)
		/// </summary>
		/// <typeparam name="T">Enumeration for the choice values</typeparam>
		/// <param name="choice">Choice to be converted</param>
		/// <param name="guidValues">Dictionary of Guids and choice values</param>
		/// <returns>List of converted choice values</returns>
		public static List<T> ConvertToChoiceValue<T>(this Choice choice, Dictionary<Guid, T> guidValues)
		{
			List<T> result = null;

			if (choice != null && choice.ArtifactGuids != null && choice.ArtifactGuids.Count > 0)
			{
				result = new List<T>();
				foreach (Guid guid in choice.ArtifactGuids)
				{
					result.Add(guidValues[guid]);
				}
			}

			return result;
		}
	}
}
