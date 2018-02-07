
var Dragon;
(function (D, $, undefinded) {
	(function (Grid) {
		(function (F) {
			var getGrid = (function () {
				gridCache = {};
				return function (gridID) {
					if (typeof gridCache[gridID] === "undefined") {
						gridCache[gridID] = $('#' + gridID);
					}
					return gridCache[gridID];
				};
			})();

			function getAnchor(url, text) {
				var array = [];
				array.push("<a ");
				array.push("href='");
				array.push(url);
				array.push("' target='_top' ");
				array.push("'>");
				array.push(D.htmlEscape(text));
				array.push("</a>");
				return array.join('');
			};

			F.anchor = function (cellValue, options, rowObject) {
				var url = rowObject[options.colModel.gridFormatter.UrlLocation];
				return getAnchor(url, cellValue);
			};

			F.templateUrl = function (cellValue, options, rowObject) {
				var formattedUrl = D.formatString(unescape(options.colModel.gridFormatter.TemplateURL), rowObject);
				return getAnchor(formattedUrl, cellValue);
			};

			F.favorite = function (cellValue, options, rowObject) {
				var changeState = function ($el, state) {
					$el.toggleClass('selected', state);//.toggleClass('selected', !state);
				};
				var id = options.gid + 'fav_' + options.rowId,
					$a = $('<span/>').attr('id', id).addClass('icon-priority icon'),
					FAV_CLASS = "selected";

				changeState($a, cellValue);
				//$('#' + options.gid).on('click', '#' + id, function () {
				//	var artifactID = rowObject.artifactID,
				//		$this = $(this),
				//		shouldBefavorited = !$this.hasClass(FAV_CLASS);
				//	window[options.gid].raise({type:'favClicked'}, [shouldBefavorited]);
				//	changeState($this, shouldBefavorited);
				//	Method.API.ajax(JSON.stringify({ state: shouldBefavorited }), { type: 'Post', url: Method.API.generateWebURL('Project', 'SetFavorite') + '/'+ 0+ '/' + artifactID });
				//});
				return $a[0].outerHTML;
			};

			F.statsControl = function (cellValue, options, rowObject) {
				if (cellValue.Hide === false) {
					var $template = $('#' + options.colModel.gridFormatter.templateID).clone(),
					    d;

					$template.attr('id', cellValue.ID + '_' + options.gid + '_' + options.pos + '_' + options.rowId);
					$template.addClass(cellValue.URLType);
					d = Dragon.StatsControl($template, true);
					//d.Numerator().url('#');
					d.Numerator().url(cellValue.NumeratorURL, cellValue.URLType === 'redirect');
					d.Numerator().set(cellValue.Numerator);
					d.Denominator().set(cellValue.Denominator);
					d.title(cellValue.Title);
					return d.$elements[0].outerHTML;
				} else {
					return '';
				}
			};

			F.radio = function (cellValue, options, rowObject) {
				//jqgrid takes care of this as long as the class has cbox
				return '<input class="cbox" type="radio" data-row="' + options.rowId + '" name="radio_' + options.gid + '"  />';
			};

			F.displayValue = function (cellValue, options, rowObject) {
				return rowObject[options.colModel.gridFormatter.DisplayValuePropertyName];
			};

			F.toggleSelectable = (function () {
				var selectCache = (function () {
					var cache = {};
					return {
						get: function (cacheKey) {
							return cache[cacheKey] || [];
						},
						add: function (cacheKey, item) {
							var local = this.get(cacheKey);
							local.push(item);
							cache[cacheKey] = local;
						},
						clear: function (cacheKey) {
							cache[cacheKey].length = 0;
						}
					};
				})();
				//we save all the potential lock downs and wait till the entire grid
				//is loaded and disable the rows
				//jqgrid does not hand us the row when we are formatting
				return function (cellValue, options, rowObject) {
					if (cellValue) {
						selectCache.add(options.gid, options.rowId);
					}
					if (selectCache.get(options.gid).length === 1) {
						getGrid(options.gid).on('jqGridLoadComplete', function () {
							var $grid = getGrid(options.gid);
							$.each(selectCache.get(this.id), function () {
								var rowID = this;
								Grid.utils.getCommandColumn($grid, rowID).prop('disabled', 'disabled').hide();
							});
							selectCache.clear(this.id);
						});
					}
					return '';
				}
			})();

		})(Grid.Formatters || (Grid.Formatters = {}));
	})(D.Grid || (D.Grid = {}));
})(Dragon || (Dragon = {}), jQuery);