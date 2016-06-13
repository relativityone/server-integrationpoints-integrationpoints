using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Models;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Commands.MassEdit
{
	public class RelativityMassEdit : RelativityMassEditBase, IMassEditCommand
	{
		private readonly BaseServiceContext _context;
		private readonly List<MassEditObject> _massEditObjects;
		private readonly int _count;
		private readonly string _tempTableDataSource;
		private readonly global::Relativity.Query.ArtifactType _artifactType = new global::Relativity.Query.ArtifactType(global::Relativity.ArtifactType.Document);

		public RelativityMassEdit(BaseServiceContext context, List<MassEditObject> massEditObjects, int count, string tempTableDataSource)
		{
			_context = context;
			_massEditObjects = massEditObjects;
			_count = count;
			_tempTableDataSource = tempTableDataSource;
		}

		public void Execute()
		{
			base.TagFieldsWithRdo(_context, _massEditObjects, _count, _artifactType, _tempTableDataSource);
		}
	}
}