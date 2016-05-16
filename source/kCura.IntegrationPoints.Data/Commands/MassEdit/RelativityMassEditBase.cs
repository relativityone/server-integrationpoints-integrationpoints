using System;
using System.Collections.Generic;
using Relativity.Core;
using Relativity.Core.Process;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Data.Commands.MassEdit
{
	public abstract class RelativityMassEditBase
	{
		private const int _BATCH_SIZE = 1000;

		protected void TagFieldsWithRdo(BaseServiceContext context, global::Relativity.Core.DTO.Field fieldToUpdate, int numberOfDocuments, global::Relativity.Query.ArtifactType objectType, int rdoArtifactId, string tempTableName)
		{
			fieldToUpdate.Value = GetMultiObjectListUpdate(rdoArtifactId);

			ExecuteMassEditAction(context, fieldToUpdate, numberOfDocuments, objectType, tempTableName);
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

		protected void UpdateSingleChoiceField(BaseServiceContext context, global::Relativity.Core.DTO.Field fieldToUpdate, int numberOfErrors, global::Relativity.Query.ArtifactType objectType, int choiceArtifactId, string tempTableName)
		{
			//Providing some extra properties for the field to update
			ICodeManagerImplementation codeManagerImplementation = new CodeManagerImplementation();
			fieldToUpdate.CodeTypeID = codeManagerImplementation.GetCodeTypeIdsByCodeArtifactIds(context, new List<int>() { choiceArtifactId })[0];
			fieldToUpdate.FieldArtifactTypeID = objectType.Id;
			fieldToUpdate.Value = choiceArtifactId;

			ExecuteMassEditAction(context, fieldToUpdate, numberOfErrors, objectType, tempTableName);
		}

		private void ExecuteMassEditAction(BaseServiceContext context, global::Relativity.Core.DTO.Field fieldToUpdate, int numberToUpdate, global::Relativity.Query.ArtifactType objectType, string tempTableName)
		{
			MassProcessHelper.MassProcessInitArgs initArgs = new MassProcessHelper.MassProcessInitArgs(tempTableName, numberToUpdate, false);
			using (SqlMassProcessBatch batch = new SqlMassProcessBatch(context, initArgs, _BATCH_SIZE))
			{
				global::Relativity.Core.DTO.Field[] fields =
				{
					fieldToUpdate
				};

				Edit massEdit = new Edit(context, batch, fields, _BATCH_SIZE, String.Empty, true, true, true, objectType);
				massEdit.Execute(true);
			}
		}
	}
}