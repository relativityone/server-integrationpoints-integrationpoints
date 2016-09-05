namespace kCura.IntegrationPoints.Injection
{
	public class InjectionPointDto
	{
		public InjectionPointDto(string injectionPointId, string description, string feature)
		{
			Id = injectionPointId;
			Description = description;
			Feature = feature;
		}

		public string Description { get; }
		public string Feature { get; }
		public string Id { get; }
	}

	public static class InjectionPointExtensions
	{
		public static kCura.Injection.InjectionPoint ConvertToKcuraInjection(this InjectionPointDto injectionPointDto)
		{
			return new kCura.Injection.InjectionPoint(
				injectionPointDto.Id,
				injectionPointDto.Description,
				injectionPointDto.Feature);
		}
	}
}