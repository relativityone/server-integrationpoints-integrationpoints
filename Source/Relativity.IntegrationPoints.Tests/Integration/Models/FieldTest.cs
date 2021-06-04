using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
	public class FieldTest : RdoTestBase
	{
		public string Name { get; set; }

		public bool IsDocumentField { get; set; }

		public bool IsIdentifier { get; set; }

		public FieldTest() : base("Field")
		{
		}

		public override List<Guid> Guids => new List<Guid>();

		public override RelativityObject ToRelativityObject()
		{
			return new RelativityObject
			{
				ArtifactID = ArtifactId,
				ParentObject = new RelativityObjectRef
				{
					ArtifactID = ParenObjectArtifactId
				},
				FieldValues = new List<FieldValuePair>
				{
					new FieldValuePair
					{
						Field = new Field
						{
							Name = "Is Identifier"
						},
						Value = IsIdentifier
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Name = "Name"
						},
						Value = Name
					}
				},
			};
		}
	}
}
