using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class ListOfFieldValuePairExtensions
	{
		public static string GetTextFieldValue(this IEnumerable<FieldValuePair> fieldValues, string fieldName)
			=> fieldValues
				.Single(fieldValuePair => fieldValuePair.Field.Name == fieldName)
				.Value as string;
	}
}
