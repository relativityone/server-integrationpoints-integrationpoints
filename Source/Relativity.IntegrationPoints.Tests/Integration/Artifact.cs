namespace Relativity.IntegrationPoints.Tests.Integration
{
	public static class Artifact
	{
		private static int _currentArtifactId = 100000;
		
		public static int NextId()
		{
			return ++_currentArtifactId;
	}
	}
}
