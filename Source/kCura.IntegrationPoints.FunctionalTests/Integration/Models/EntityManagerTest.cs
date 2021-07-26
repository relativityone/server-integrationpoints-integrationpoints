using System;
using System.Data;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class EntityManagerTest
	{
		public int Id { get; set; }

		public string EntityId { get; set; }

		public string ManagerId { get; set; }

		public long? LockedByJobId {get; set; }

		public DateTime CreatedOn { get; set; }

		public static EntityManagerTest FromRow(DataRow row)
		{
			return new EntityManagerTest
			{
				Id = ArtifactProvider.NextId(),
				EntityId = row["EntityID"].ToString(),
				ManagerId = row["ManagerID"].ToString(),
				CreatedOn = (DateTime)row["CreatedOn"]
			};
		}

		public DataRow AsDataRow()
		{
			return AsTable().Rows[0];
		}

		public DataTable AsTable()
		{
			DataTable dt = DatabaseSchema.EntityManagerSchema();

			DataRow row = dt.NewRow();

			row["ID"] = Id;
			row["EntityID"] = EntityId;
			row["ManagerID"] = ManagerId;
			row["LockedByJobID"] = (object)LockedByJobId ?? DBNull.Value;
			row["CreatedOn"] = CreatedOn;

			dt.Rows.Add(row);

			return dt;
		}
	}
}
