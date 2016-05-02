using System;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	internal class LazyExceptArtifactFieldDto : ArtifactFieldDTO
	{
		private readonly Exception _exception;

		public LazyExceptArtifactFieldDto(Exception exception)
		{
			_exception = exception;
		}

		public override object Value
		{
			get
			{
				if (_exception != null)
				{
					throw _exception;
				}
				return base.Value;
			}
			set
			{
				base.Value = value;
			}
		}
	}
}