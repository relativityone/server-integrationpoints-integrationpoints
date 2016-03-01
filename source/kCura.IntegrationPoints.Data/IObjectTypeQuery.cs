﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data
{
	public interface IObjectTypeQuery
	{
		List<ObjectType> GetAllTypes(int userId);

		Dictionary<Guid, int> GetRdoGuidToArtifactIdMap(int userId);
	}
}
