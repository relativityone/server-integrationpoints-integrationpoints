(function () {
	var mapGrid = new Dragon.Grid.GridControl(window.grid);
	new Dragon.Grid.Pager({
		pagerID: 'mappedFieldsPager',
		grid: mapGrid
	});
})();