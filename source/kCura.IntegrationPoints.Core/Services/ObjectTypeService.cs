﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public class ObjectTypeService
	{
		public const int NON_SYSTEM_FIELD_IDS = 1000000;

		private readonly RSAPIRdoQuery _rdoQuery;
		public ObjectTypeService(RSAPIRdoQuery rdoQuery)
		{
			_rdoQuery = rdoQuery;
		}

		public bool HasParent(int objectType)
		{
			var rdo = _rdoQuery.GetType(objectType);
			return rdo.ParentArtifactTypeID > NON_SYSTEM_FIELD_IDS;
		}

		public int GetObjectTypeID(string artifactTypeName)
		{
			return _rdoQuery.GetObjectTypeID(artifactTypeName);
		}

	}
}
