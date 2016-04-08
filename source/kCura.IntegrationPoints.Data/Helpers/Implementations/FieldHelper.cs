using System;
using System.Security.Claims;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Core.DTO;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Data.Helpers
{
	public class FieldHelper : IFieldHelper
	{
		private readonly Lazy<IFieldManagerImplementation> _manager;
		private readonly BaseServiceContext _context;

		private IFieldManagerImplementation Manager
		{
			get { return _manager.Value; }
		}

		public FieldHelper(int workspaceArtifactId)
		{
			_manager = new Lazy<IFieldManagerImplementation>(() => new FieldManagerImplementation());
			_context = ClaimsPrincipal.Current.GetServiceContextUnversionShortTerm(workspaceArtifactId);
		}

		public void SetOverlayBehavior(int fieldArtifactId, bool value)
		{
			Field fieldDto = Manager.Read(_context, fieldArtifactId);
			fieldDto.OverlayBehavior = value;
			Manager.Update(_context, fieldDto, fieldDto.DisplayName, fieldDto.IsArtifactBaseField);
		}
	}
}