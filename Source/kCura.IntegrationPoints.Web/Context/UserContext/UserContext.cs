using kCura.IntegrationPoints.Web.Context.UserContext.Exceptions;
using kCura.IntegrationPoints.Web.Context.UserContext.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace kCura.IntegrationPoints.Web.Context.UserContext
{
	internal class UserContext : IUserContext
	{
		private const int _VALUE_NOT_FOUND_SPECIAL_VALUE = 0;

		private readonly IEnumerable<IUserContextService> _userContextServices;

		public UserContext(IEnumerable<IUserContextService> userContextServices)
		{
			_userContextServices = userContextServices.ToList();
		}

		public int GetUserID() => GetValue(x => x.GetUserID());

		public int GetWorkspaceUserID() => GetValue(x => x.GetWorkspaceUserID());

		private int GetValue(Func<IUserContextService, int> valueGetter, [CallerMemberName] string propertyName = "")
		{
			foreach (IUserContextService userContextService in _userContextServices)
			{
				int propertyValue = valueGetter(userContextService);
				if (propertyValue != _VALUE_NOT_FOUND_SPECIAL_VALUE)
				{
					return propertyValue;
				}
			}

			throw new UserContextNotFoundException(propertyName);
		}
	}
}