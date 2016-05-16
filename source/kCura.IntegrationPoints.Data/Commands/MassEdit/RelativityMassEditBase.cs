using System;
using System.Collections.Generic;
using Relativity.Core;
using Relativity.Core.Process;
using Relativity.Core.Service;
using ArtifactType = Relativity.Query.ArtifactType;
using Field = Relativity.Core.DTO.Field;

namespace kCura.IntegrationPoints.Data.Commands.MassEdit
{
	public abstract class RelativityMassEditBase
	{
		private const int _BATCH_SIZE = 1000;
		private readonly ArtifactType _artifactType = new ArtifactType(global::Relativity.ArtifactType.Document);

		protected void TagDocumentsWithRdo(BaseServiceContext context, Field fieldToUpdate, int numberOfDocuments, int rdoArtifactId, string tempTableName)
		{
			fieldToUpdate.Value = GetMultiObjectListUpdate(rdoArtifactId);

			MassProcessHelper.MassProcessInitArgs initArgs = new MassProcessHelper.MassProcessInitArgs(tempTableName, numberOfDocuments, false);
			using (SqlMassProcessBatch batch = new SqlMassProcessBatch(context, initArgs, _BATCH_SIZE))
			{
				Field[] fields =
				{
					fieldToUpdate
				};

				Edit massEdit = new Edit(context, batch, fields, _BATCH_SIZE, String.Empty, true, true, true, _artifactType);
				massEdit.Execute(true);
			}
		}

		internal MultiObjectListUpdate GetMultiObjectListUpdate(int destinationWorkspaceInstanceId)
		{
			var objectsToUpdate = new MultiObjectListUpdate();
			var instances = new List<int>()
			{
				destinationWorkspaceInstanceId
			};

			objectsToUpdate.tristate = true;
			objectsToUpdate.Selected = instances;

			return objectsToUpdate;
		}

		protected void UpdateSingleChoiceField(BaseServiceContext context, Field fieldToUpdate, int numberOfErrors, ArtifactType objectType, int choiceArtifactId, string tempTableName)
		{
			//Providing some extra properties for the field to update
			ICodeManagerImplementation codeManagerImplementation = new CodeManagerImplementation();
			fieldToUpdate.CodeTypeID = codeManagerImplementation.GetCodeTypeIdsByCodeArtifactIds(context, new List<int>() { choiceArtifactId })[0];
			fieldToUpdate.FieldArtifactTypeID = objectType.Id;
			fieldToUpdate.Value = choiceArtifactId;

			MassProcessHelper.MassProcessInitArgs initArgs = new MassProcessHelper.MassProcessInitArgs(tempTableName, numberOfErrors, false);
			using (SqlMassProcessBatch batch = new SqlMassProcessBatch(context, initArgs, _BATCH_SIZE))
			{
				Field[] fields =
				{
					fieldToUpdate
				};

				Edit massEdit = new Edit(context, batch, fields, _BATCH_SIZE, String.Empty, true, true, false, _artifactType);
				massEdit.Execute(true);
			}
		}
	}
}