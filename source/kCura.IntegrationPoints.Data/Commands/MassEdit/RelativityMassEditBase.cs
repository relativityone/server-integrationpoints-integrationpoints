using System;
using System.Collections.Generic;
using System.Data;
using Relativity.Core;
using Relativity.Core.Process;
using Relativity.Data;
using Field = Relativity.Core.DTO.Field;
using ArtifactType = Relativity.Query.ArtifactType;

namespace kCura.IntegrationPoints.Data.Commands.MassEdit
{
	public abstract class RelativityMassEditBase
	{
		private const int _BATCH_SIZE = 1000;
		private readonly ArtifactType _artifactType = new ArtifactType(10 , "Document");

		protected void TagDocumentsWithRdo(BaseServiceContext context, Field fieldToUpdate, int numberOfDocuments, int rdoArtifactId, string tempTableName)
		{
			fieldToUpdate.Value = GetMultiObjectListUpdate(rdoArtifactId);

			MassProcessHelper.MassProcessInitArgs initArgs = new MassProcessHelper.MassProcessInitArgs(tempTableName, numberOfDocuments, false);
			SqlMassProcessBatch batch = new SqlMassProcessBatch(context, initArgs, _BATCH_SIZE);

			Field[] fields =
			{
				fieldToUpdate
			};

			Edit massEdit = new Edit(context, batch, fields, _BATCH_SIZE, String.Empty, true, true, false, _artifactType);
			massEdit.Execute(true);
		}

		internal MultiObjectListUpdate GetMultiObjectListUpdate(int destinationWorkspaceInstanceId)
		{
			var objectstoUpdate = new MultiObjectListUpdate();
			var instances = new List<int>()
			{
				destinationWorkspaceInstanceId
			};

			objectstoUpdate.tristate = true;
			objectstoUpdate.Selected = instances;

			return objectstoUpdate;
		}
	}
}