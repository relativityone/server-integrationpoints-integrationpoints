using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class FolderTest : RdoTestBase
	{
		public string Name { get; set; }

		public FolderTest() : base("Folder")
		{
		}

		public override List<Guid> Guids => new List<Guid>();

		public override RelativityObject ToRelativityObject()
		{
			throw new System.NotImplementedException();
		}
	}
}
