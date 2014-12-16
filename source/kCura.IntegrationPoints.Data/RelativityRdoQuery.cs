﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using Artifact = kCura.Relativity.Client.Artifact;
using Field = kCura.Relativity.Client.Field;

namespace kCura.IntegrationPoints.Data
{
	public class RelativityRdoQuery
	{
		private readonly IRSAPIClient _client;

		public RelativityRdoQuery(IRSAPIClient client)
		{
			_client = client;

		}

		public virtual List<ObjectType> GetAllRdo()
		{

			var qry = new Query<Relativity.Client.DTOs.ObjectType>();
			qry.Fields = new List<FieldValue>()
				{
					new FieldValue(Relativity.Client.DTOs.ObjectTypeFieldNames.DescriptorArtifactTypeID),
					new FieldValue(Relativity.Client.DTOs.ObjectTypeFieldNames.Name),
					new FieldValue(Relativity.Client.DTOs.ObjectTypeFieldNames.ParentArtifactTypeID)
				};
			qry.Sorts = new List<Sort>
			{
				new Sort
				{
					Field = ObjectTypeFieldNames.Name,
					Direction = SortEnum.Ascending
				}
			};

			var result = _client.Repositories.ObjectType.Query(qry);
			if (!result.Success)
			{
				var messages = result.Results.Where(x => !x.Success).Select(x => x.Message);
				var e = new AggregateException(result.Message, messages.Select(x => new Exception(x)));
				throw e;
			}
			return result.Results.Select(x => x.Artifact).ToList();
		}

		public virtual ObjectType hasParent(int id)
		{
			
			var qry = new Query<Relativity.Client.DTOs.ObjectType>();
			qry.Fields = new List<FieldValue>()
				{
					new FieldValue(Relativity.Client.DTOs.ObjectTypeFieldNames.DescriptorArtifactTypeID),
					new FieldValue(Relativity.Client.DTOs.ObjectTypeFieldNames.Name),
					new FieldValue(Relativity.Client.DTOs.ObjectTypeFieldNames.ParentArtifactTypeID)
				};
			qry.Condition = new ObjectCondition
			{
				Field = Relativity.Client.DTOs.ObjectTypeFieldNames.DescriptorArtifactTypeID,
				Operator = ObjectConditionEnum.EqualTo,
				Value = new List<int> { id }
			};
			qry.Sorts = new List<Sort>
			{
				new Sort
				{
					Field = ObjectTypeFieldNames.Name,
					Direction = SortEnum.Ascending
				}
			};

			var result = _client.Repositories.ObjectType.Query(qry);
			if (!result.Success)
			{
				var messages = result.Results.Where(x => !x.Success).Select(x => x.Message);
				var e = new AggregateException(result.Message, messages.Select(x => new Exception(x)));
				throw e;
			}
			return result.Results.Select(x => x.Artifact).FirstOrDefault(y => y.ParentArtifactTypeID > 1000000);
		} 
	}
}
