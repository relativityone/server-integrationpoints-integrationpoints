using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Fields
	{
		public static string GetDocumentIdentifierFieldName(IFieldQueryRepository fieldQueryRepository)
		{
			ArtifactDTO[] fieldArtifacts = fieldQueryRepository.RetrieveFieldsAsync(
				10,
				new HashSet<string>(new[]
				{
					"Name",
					"Is Identifier"
				})).ConfigureAwait(false).GetAwaiter().GetResult();

			string fieldName = String.Empty;
			foreach (ArtifactDTO fieldArtifact in fieldArtifacts)
			{
				int isIdentifierFieldValue = 0;
				foreach (ArtifactFieldDTO field in fieldArtifact.Fields)
				{
					if (field.Name == "Name")
					{
						fieldName = field.Value.ToString();
					}
					if (field.Name == "Is Identifier")
					{
						try
						{
							isIdentifierFieldValue = Convert.ToInt32(field.Value);
						}
						catch
						{
							// suppress error for invalid casts
						}
					}
				}
				if (isIdentifierFieldValue == 1)
				{
					break;
				}
			}
			return fieldName;
		}

        public static int GetFieldObjectLength(RelativityObject fieldObject)
        {
            FieldValuePair lengthFieldValuePair = fieldObject.FieldValues.SingleOrDefault(x => x.Field.Name == "Length");
            return (int?)lengthFieldValuePair.Value ?? 0;
		}

        public static string GetFieldValueStringByFieldName(RelativityObject fieldObject, string fieldName)//return based on type
        {
            return fieldObject.FieldValues.First(fv => fv.Field.Name == fieldName).Value.ToString();
        }
        public static bool GetFieldValueBoolByFieldName(RelativityObject fieldObject, string fieldName)//return based on type
        {
            return (bool) fieldObject.FieldValues.First(fv => fv.Field.Name == fieldName).Value;
        }
	}
}