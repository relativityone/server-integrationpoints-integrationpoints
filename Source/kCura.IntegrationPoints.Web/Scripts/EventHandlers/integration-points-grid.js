$(function () {
	//we d
	var $grid = $('#_dynamicTemplate__kCuraScrollingDiv__dynamicViewFieldRenderer_div_4');

	$grid.append('<table><tr><td><div id="mapFieldsGridPager"></div></td></tr></table><table><tr><td><table id="mapFieldsGrid"></table></td></tr></table>');
	function generateWebURL() {
		var baseURL = IP.cpPath;
		for (var i = 0; i < arguments.length; i++) {
			baseURL = baseURL + '/' + arguments[i];
		}
		return baseURL;
	};

	function getParameterByName(name, w) {
		w = w || window;
		name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
		var regexS = "[\\?&]" + name + "=([^&#]*)";
		var regex = new RegExp(regexS);
		var results = regex.exec(w.location.search);
		if (results == null) {
			return "";
		} else {
			return decodeURIComponent(results[1].replace(/\+/g, " "));
		}
	};

	$.get(generateWebURL('integrationpoints', 'GetGridModel', getParameterByName('ArtifactID'))).then(function (settings) {
		$.get(settings.url).then(function (result) {
			var grid = new Dragon.Grid.GridControl(settings);
			new Dragon.Grid.Pager({
				pagerID: 'mapFieldsGridPager',
				grid: grid
			});
		}, function () {

		});

	}, function () {
	});
});