using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal class UserService : IUserService
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
		private readonly IAPILog _logger;
		
		internal UserService(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IAPILog logger)
		{
			_serviceFactoryForAdmin = serviceFactoryForAdmin;
			_logger = logger;
		}
		
		public async Task<bool> ExecutingUserIsAdminAsync(IUserContextConfiguration userContext)
		{
			_logger.LogInformation("Check if User {userId} is Admin", userContext.ExecutingUserId);
			using (IGroupManager groupManager = await _serviceFactoryForAdmin.CreateProxyAsync<IGroupManager>().ConfigureAwait(false))
			{
				QueryRequest request = BuildAdminGroupsQuery();
				QueryResultSlim result = await groupManager.QueryGroupsByUserAsync(request, 0, 1, userContext.ExecutingUserId).ConfigureAwait(false);

				return result.Objects.Any();
			}
		}
		
		private static QueryRequest BuildAdminGroupsQuery()
		{
			const string adminGroupType = "System Admin";
			var request = new QueryRequest()
			{
				Condition = $"'Group Type' == '{adminGroupType}'",
			};

			return request;
		}
	}
}