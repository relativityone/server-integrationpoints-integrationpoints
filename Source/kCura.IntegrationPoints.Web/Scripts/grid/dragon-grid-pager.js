var Dragon = Dragon || {};

(function (D, $) {
	(function (G) {
		G.Pager = (function () {
						
			var pagerDefaults = {
				pagerIcons: {
					'first': 'icon-paging-first',
					'previous': 'icon-paging-previous',
					'next': 'icon-paging-next',
					'last': 'icon-paging-last',
					'prefix': 'legal-hold'
				},
				pageText: 'Items {0} - {1} (of {2})',
				createPageLocationMenu: function () {
					var span = document.createElement('span');
					
					var self = this;
					this.grid.on('loadComplete', function (result) {
						var pageSize = self.grid.getPageSize();
						var currentRecords = ((pageSize * (result.page - 1)) + 1);
						if (result.records === 0) {
							currentRecords = 0;
						}
						var input = '<input type="text" value="' + currentRecords + '"/>';
						span.innerHTML = D.formatString(self.pageText, {
							'0': input,
							'1': Math.min(pageSize * result.page,result.records),
							'2': result.records
							});
					});
					
					return span;
				}
			};

			var _buildPageBtn = function (prefix, page,type) {
				var li = document.createElement('li');
				var a = document.createElement('a');
				a.href = '#';
				a.setAttribute('data-type', type);
				a.className = prefix + ' ' + page + ' page';
				li.appendChild(a);
				return li;
			};

			var _buildPager = function () {
				//don't loop because we can't guarantee order
				var ul = document.createElement('ul');
				var pageIcons = this._settings.pagerIcons;
				ul.appendChild(_buildPageBtn(pageIcons.prefix, pageIcons.first, 'first'));
				ul.appendChild(_buildPageBtn(pageIcons.prefix, pageIcons.previous, 'prev'));
				ul.appendChild(_buildPageBtn(pageIcons.prefix, pageIcons.next, 'next'));
				ul.appendChild(_buildPageBtn(pageIcons.prefix, pageIcons.last, 'last'));
				this._$el.append(ul);
			};

			var _buildPageLocation = function () {
				var el = this._settings.createPageLocationMenu();
				this._$el.append(el);
			};

			function pager(settings) {
				this._settings = $.extend({}, pagerDefaults, settings);
				this._$el = $('#' + this._settings.pagerID);
				this._$el.addClass('dragon-pager');
				$('#' + this._settings.grid.settings.pagerID + '_left').find('table').eq(0).hide();

				_buildPageLocation.call(this);
				_buildPager.call(this);
				var self = this;
				this._$el.on('click', 'li a.page', function (e) {
					var el = e.srcElement || e.target;
					var $el = $(el);
					var type = $el.data('type') + 'Page';

					self._settings.grid[type]();
				});

			}



			$.extend(pager.prototype, {

			});

			return pager;
		})();
	})(Dragon.Grid || (Dragon.Grid = {}));
})(Dragon, jQuery);