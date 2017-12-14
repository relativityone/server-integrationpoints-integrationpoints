using System.Collections.Generic;

namespace kCura.IntegrationPoints.Domain.Logging
{
	public class BaseCorrelationContext
	{
		public string ActionName { get; set; }
		public int? WorkspaceId { get; set; }
		public int? UserId { get; set; }

		public virtual Dictionary<string, object> ToDictionary()
		{
			return new Dictionary<string, object>
			{
				[nameof(ActionName)] = ActionName,
				[nameof(WorkspaceId)] = WorkspaceId,
				[nameof(UserId)] = UserId
			};
		}

		public virtual void SetValuesFromDictionary(Dictionary<string, object> dictionary)
		{
			if (dictionary == null)
			{
				return;
			}
			ActionName = GetValueOrDefault<string>(dictionary, nameof(ActionName));
			WorkspaceId = GetValueOrDefault<int?>(dictionary, nameof(WorkspaceId));
			UserId = GetValueOrDefault<int?>(dictionary, nameof(UserId));
		}

		protected T GetValueOrDefault<T>(Dictionary<string, object> dictionary, string key)
		{
			object output;
			if (dictionary.TryGetValue(key, out output))
			{
				return (T)output;
			}
			return default(T);
		}
	}
}
