using System;
using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
	public interface IRelativitySourceRdoFields
	{
		void CreateFields(int workspaceId, IDictionary<Guid, Field> fields);
	}
}