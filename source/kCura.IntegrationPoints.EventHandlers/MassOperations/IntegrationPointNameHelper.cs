using System;
using Relativity;
using Relativity.Core;

namespace kCura.IntegrationPoints.EventHandlers.MassOperations
{
	public class IntegrationPointNameHelper : IIntegrationPointNameHelper
	{
		private readonly BaseContext _baseContext;

		public IntegrationPointNameHelper(BaseContext baseContext)
		{
			_baseContext = baseContext;
		}

		public string CreateNameForCopy(Data.IntegrationPoint integrationPoint)
		{
			var artifactTypeId = integrationPoint.Rdo.ArtifactTypeID;
			if (!artifactTypeId.HasValue)
			{
				throw new ArgumentException("Given Integration Point RDO doesn't have ArtifactTypeID");
			}
			return NameHelper.CreateNameForCopy(_baseContext, (ArtifactType) artifactTypeId.Value, integrationPoint.Name);
		}
	}
}