using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using FieldType = kCura.Relativity.Client.FieldType;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Fields
	{
		private static Relativity.Client.DTOs.Field CreateFieldDto(FieldType fieldType, string fieldName)
		{
			Relativity.Client.DTOs.Field dto = new Relativity.Client.DTOs.Field
			{
				Name = fieldName ?? $"RIP - {fieldType}",
				ObjectType = new Relativity.Client.DTOs.ObjectType
				{
					DescriptorArtifactTypeID = (int)ArtifactType.Document
				},
				FieldTypeID = fieldType,
				IsRequired = false,
				AllowGroupBy = false,
				AllowPivot = false,
				IgnoreWarnings = true,
				Width = String.Empty
			};

			switch (fieldType)
			{
				case FieldType.Date:
				case FieldType.Decimal:
				case FieldType.WholeNumber:
					dto.Linked = false;
					dto.AllowSortTally = true;
					dto.Wrapping = true;
					dto.OpenToAssociations = false;
					break;

				case FieldType.FixedLengthText:
					dto.Linked = false;
					dto.AllowSortTally = true;
					dto.Wrapping = true;
					dto.OpenToAssociations = false;
					dto.IncludeInTextIndex = false;
					dto.AllowHTML = false;
					dto.Unicode = false;
					dto.Length = 255;
					dto.IsRelational = false;
					break;

				case FieldType.LongText:
					dto.Linked = false;
					dto.AllowSortTally = true;
					dto.Wrapping = true;
					dto.OpenToAssociations = false;
					dto.IncludeInTextIndex = false;
					dto.AllowHTML = false;
					dto.Unicode = false;
					dto.AvailableInViewer = true;
					break;

				case FieldType.MultipleChoice:
				case FieldType.SingleChoice:
					dto.Linked = false;
					dto.AllowSortTally = true;
					dto.Wrapping = true;
					dto.OpenToAssociations = false;
					dto.Unicode = true;
					dto.AvailableInFieldTree = true;
					break;

				case FieldType.MultipleObject:
					dto.AvailableInFieldTree = true;
					break;

				case FieldType.SingleObject:
					dto.Linked = false;
					dto.AllowSortTally = true;
					dto.Wrapping = true;
					dto.AvailableInFieldTree = true;
					break;

				case FieldType.YesNo:
					dto.Linked = false;
					dto.AllowSortTally = true;
					dto.Wrapping = true;
					dto.OpenToAssociations = false;
					dto.YesValue = "Hell Yes";
					dto.NoValue = "Hell No";
					break;
			}

			return dto;
		}

		public static int CreateField(int workspaceArtifactId, FieldType fieldType, Relativity.Client.DTOs.ObjectType objectType = null, string fieldName = null)
		{
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				proxy.APIOptions.WorkspaceID = workspaceArtifactId;

				Relativity.Client.DTOs.Field fieldDto = CreateFieldDto(fieldType, fieldName);
				if ((fieldType == FieldType.MultipleObject || fieldType == FieldType.SingleObject) && objectType != null)
				{
					fieldDto.AssociativeObjectType = objectType;
				}

				WriteResultSet<Relativity.Client.DTOs.Field> writeResult;
				try
				{
					writeResult = proxy.Repositories.Field.Create(fieldDto);
				}
				catch (Exception e)
				{
					throw new Exception("Error while creating field: " + e.Message);
				}

				if (!writeResult.Success)
				{
					throw new Exception("Error while creating field, result set failure: " + writeResult.Message);
				}

				Result<Relativity.Client.DTOs.Field> field = writeResult.Results.FirstOrDefault();
				int fieldArtifactId = field.Artifact.ArtifactID;
				return fieldArtifactId;
			}
		}

		public static Relativity.Client.DTOs.Field ReadField(int workspaceArtifactId, int fieldArtifactId)
		{
			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				proxy.APIOptions.WorkspaceID = workspaceArtifactId;

				Relativity.Client.DTOs.Field fieldDto = new Relativity.Client.DTOs.Field(fieldArtifactId)
				{
					Fields = FieldValue.AllFields
				};

				ResultSet<Relativity.Client.DTOs.Field> fieldReadResult;
				try
				{
					fieldReadResult = proxy.Repositories.Field.Read(fieldDto);
				}
				catch (Exception ex)
				{
					throw new Exception("Error while reading field: " + ex.Message);
				}

				if (!fieldReadResult.Success)
				{
					throw new Exception("Error while reading field, result set failure: " + fieldReadResult.Message);
				}

				Result<Relativity.Client.DTOs.Field> fieldResult = fieldReadResult.Results.FirstOrDefault();
				Relativity.Client.DTOs.Field fieldArtifact = fieldResult?.Artifact;
				return fieldArtifact;
			}
		}

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
	}
}