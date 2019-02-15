using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using Relativity.DragonGrid.Core.Grid;

namespace kCura.IntegrationPoints.Web
{
	public class GridModelFactory // TODO to remove
	{
		private readonly int DEFAULT_PAGE_SIZE = 25;
		private readonly UserService _userService;

		public GridModelFactory(UserService service)
		{
			_userService = service;
		}

		public GridModel CreateModel(string id, int userID)
		{
			var model = new GridModel(id);
			var user = _userService.Read(userID);
			var itemsPerPage = user.ItemListPageLength.GetValueOrDefault(DEFAULT_PAGE_SIZE);
			model.showFilterToolbar = user.ShowFilters.GetValueOrDefault(false);
			model.rowNum = itemsPerPage;
			if (model.Pager.All(x => x.Size != itemsPerPage))
			{
				var list = model.Pager.ToList();
				list.Add(new PageSettings { Display = string.Format("{0} per page", itemsPerPage), Size = itemsPerPage });
				model.Pager = list.OrderBy(x => x.Size).ToList();
			}
			return model;
		}
	}
}