using Relativity.Core;
using Relativity.Core.DTO;
using ArtifactType = Relativity.Query.ArtifactType;

namespace kCura.IntegrationPoints.Data.Commands.MassEdit
{
	public class RelativityMassEdit : RelativityMassEditBase, IMassEditCommand
	{
		private readonly BaseServiceContext _context;
		private readonly Field _field;
		private readonly int _count;
		private readonly int _rdoArtifactId;
		private readonly string _tempTableDataSource;
		private readonly ArtifactType _artifactType = new ArtifactType(global::Relativity.ArtifactType.Document);

		public RelativityMassEdit(BaseServiceContext context, Field field, int count, int rdoArtifactId, string tempTableDataSource)
		{
			_context = context;
			_field = field;
			_count = count;
			_rdoArtifactId = rdoArtifactId;
			_tempTableDataSource = tempTableDataSource;
		}

		public void Execute()
		{
			base.TagFieldsWithRdo(_context, _field, _count, _artifactType, _rdoArtifactId, _tempTableDataSource);
		}
	}
}