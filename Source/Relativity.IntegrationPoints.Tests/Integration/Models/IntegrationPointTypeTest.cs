using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class IntegrationPointTypeTest : RdoTestBase
	{
		public string ApplicationIdentifier { get; set; }

		public string Identifier { get; set; }

		public string Name { get; set; }

		public IntegrationPointTypeTest() : base("IntegrationPointType")
		{
		}

		public override List<Guid> Guids => new List<Guid>();

		public override RelativityObject ToRelativityObject()
		{
			return new RelativityObject()
			{
				ArtifactID = ArtifactId,
				Guids = new List<Guid>()
				{
					new Guid("5be4a1f7-87a8-4cbe-a53f-5027d4f70b80")
				},
				Name = Name,
				FieldValues = new List<FieldValuePair>()
				{
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Application Identifier",
							Guids = new List<Guid>()
							{
								new Guid("9720e543-cce0-445c-8af7-042355671a71")
							}
						},
						Value = ApplicationIdentifier
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Identifier",
							Guids = new List<Guid>()
							{
								new Guid("3bd675a0-555d-49bc-b108-e2d04afcc1e3")
							}
						},
						Value = Identifier
					},
					new FieldValuePair()
					{
						Field = new Field()
						{
							Name = "Name",
							Guids = new List<Guid>()
							{
								new Guid("ae4fe868-5428-49fd-b2e4-bb17abd597ef")
							}
						},
						Value = Name
					},
				}
			};
		}

		public IntegrationPointType ToRdo()
		{
			return new IntegrationPointType
			{
				RelativityObject = ToRelativityObject(),
				ArtifactId = ArtifactId,
				ParentArtifactId = ParenObjectArtifactId,
				ApplicationIdentifier = ApplicationIdentifier,
				Identifier = Identifier,
				Name = Name
			};
		}
	}
}