using System;
using System.Data;
using System.Security.Claims;
using Relativity;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Data;
using Field = Relativity.Core.DTO.Field;
using QueryFieldLookup = Relativity.Core.QueryFieldLookup;

namespace kCura.IntegrationPoints.Data.Commands.MassEdit
{
	public class MassEditCommandFactory
	{
		public IMassEditCommand BuildMassEditCommand(int workspaceId, Guid fieldGuid, int count, int rdoArtifactId, string tempTableName)
		{
			BaseServiceContext context = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(workspaceId);
			string exception = $"Unable to find for field[guid:{fieldGuid}]";

			Guid[] guids = { fieldGuid };
			DataRowCollection fieldRows;
			try
			{
				fieldRows = FieldQuery.RetrieveAllByGuids(context.ChicagoContext.DBContext, guids).Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception(exception, ex);
			}

			if (fieldRows.Count == 0)
			{
				throw new Exception(exception);
			}

			Field field = new Field(context, fieldRows[0]);

			return new RelativityMassEdit(context, field, count, rdoArtifactId, tempTableName);
		}

		public IMassEditCommand BuildMassEditCommand(int workspaceId, int fieldArtifactId, int count, int rdoArtifactId, string tempTableName)
		{
			BaseServiceContext context = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(workspaceId);
			string exception = $"Unable to find for field[artifactId:{0}]";

			DataRowCollection fieldRows;
			try
			{
				IQueryFieldLookup fieldLookupHelper = new QueryFieldLookup(context, (int)Relativity.Client.ArtifactType.Document);
				ViewFieldInfo fieldInfo = fieldLookupHelper.GetFieldByArtifactID(fieldArtifactId);
				kCura.Data.DataView dataView = FieldQuery.RetrieveByArtifactViewFieldID(context.ChicagoContext.DBContext, fieldInfo.AvfId);
				fieldRows = dataView.Table.Rows;
			}
			catch (Exception ex)
			{
				throw new Exception(exception, ex);
			}

			if (fieldRows.Count == 0)
			{
				throw new Exception(exception);
			}

			Field field = new Field(context, fieldRows[0]);

			return new RelativityMassEdit(context, field, count, rdoArtifactId, tempTableName);
		}


	}
}