namespace Relativity.Sync.Executors
{
	internal sealed class TagsContainer
	{
		public object SourceJobDto { get; }
		public object SourceWorkspaceDto { get; }

		public TagsContainer(object sourceJobDto, object sourceWorkspaceDto)
		{
			SourceJobDto = sourceJobDto;
			SourceWorkspaceDto = sourceWorkspaceDto;
		}
	}
}