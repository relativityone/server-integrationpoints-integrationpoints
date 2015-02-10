(function (D, $, undefinded) {
	(function (Grid) {
		(function (F) {
			F.GridFilter = (function () {
				function base(settings) {
					this.settings = settings;
					this.$grid = $('#' + settings.gridID);
					this.controlID = "gs_" + this.settings.fieldName;

					this.headerID = 'gbox_' + this.settings.gridID;
				};

				base.prototype.getHeaderElement = function () {
					throw "Abstract function";
				};

				base.prototype.triggerSearch = function () {
					this.$grid[0].triggerToolbar();
				};

				base.prototype.getRule = function () {
					return undefined;
				};

				return base;
			})();

			F.dateFilter = (function (Base) {
				function validate($control) {
					var date = $control.val(),
							$parent = $control.parent(),
							result = true;

					if (Date.parse(date) === null && date != '') {
						$control.val('');
						if ($parent.find('.error').length === 0) {
							$parent.append('<span class="error" style="position: absolute; left: 5px;">Incorrect Syntax</span>');
						} else {
							$parent.find('.error').show();
						}

						$control.blur();
						result = false;
					}
					return result;
				};

				function filter(settings) {
					Base.call(this, settings);
				}

				filter.prototype = Object.create(Base.prototype);

				filter.prototype.getHeaderElement = function () {
					var self = this;
					var $el = $('<input type="text">').attr("id", this.controlID).datepicker({
						onSelect: function () {
							self.triggerSearch()
							$(this).trigger("change");
						}
					}),
					 $clear = $('<a/>').addClass('clearsearchclass').css({ 'padding-right': '.5em', 'padding-left': '.3em', 'font-weight': 'bold', display: 'none' }).attr('title', "Clear Search Value").text('x'),
				   $td = $('<td/>').addClass('ui-search-input').append($el).append($clear),
					 $header = $('#' + this.headerID);
					$clear.on('click', function () {
						$("#" + self.controlID).val('');
						self.triggerSearch();
					});

					$header.on('blur', '#' + this.controlID, function (e) {
						validate($(this));
					});

					$header.on('focus', '#' + this.controlID, function () {
						var $parent = $(this).parent();
						$parent.find('.error').hide();
					});

					$header.on('click', '.ui-search-input span.error', function () {
						$(this).parent().find('input').focus();
					});

					$header.on('keyup', '#' + this.controlID, function (e) {
						if (e.keyCode === D.RETURN) {
							$(this).datepicker("hide");
							Base.prototype.triggerSearch.call(self);
						}
					});
					//<a title="Clear Search Value" style="padding-right: 0.3em; padding-left: 0.3em; display: none;" class="clearsearchclass">x</a>
					var $std = $('<td class="ui-search-clear"></td>').append($clear);
					return $('<table/>').addClass('ui-search-table').attr('cellSpacing', 0).append($('<tbody/>').append($('<tr/>').append($td).append($std)));
				};

				filter.prototype.getRule = function () {
					var $control = $("#" + this.controlID);

					if (validate($control)) {
						return { data: $control.val(), field: this.settings.fieldName, op: 'eq' };
					}
				};
				return filter;
			})(F.GridFilter);

			F.selectFilter = (function (Base) {
				var ALL_VALUE = 'All';
				function filter(settings) {
					Base.call(this, settings);
					this.controlID = this.settings.fieldName;
					this.options = [];
					this.val = ALL_VALUE;
				}
				filter.prototype.getHeaderElement = function (settings) {
					var $el = $('<select/>').attr("id", this.controlID).val(''),
						self = this, headerID = this.headerID;

					self.settings.callbacks.on("loadComplete", function (result) {
						result.filterSelectionLists = result.filterSelectionLists || {};
						var options = result.filterSelectionLists[self.settings.fieldName] || [];
						self.options = options;
						if (options.length > 0) {
							$el.find("option").remove();
						}
						$el.append($("<option/>").attr("value", "All").text(ALL_VALUE));
						if (options.length > 0) {
							$.each(options, function (idx, val) {
								var $opt = $("<option/>").attr("value", val).text(val);
								if (self.val === val) {
									$opt.attr('checked', 'checked');
								}
								//this could cause perf problems
								//may want to add these elements to a fragment first then append
								$el.append($opt);
							});
						}

						var select2Options = {};
						if (typeof settings.subGrid !== "undefined" && settings.subGrid === false) {
							select2Options = { containerCssClass: "subgrid-filter-container", dropdownCssClass: "subgrid-filter-select", dropdownAutoWidth: false };
						} else {
							select2Options = { containerCssClass: "filter-container", dropdownCssClass: "filter-select", dropdownAutoWidth: false };
						}
						$el.select2(select2Options).addClass('ui-search-input').select2("val", self.val);

						$('#gbox_' + self.$grid.attr('id') + ' .filter-container').find('span.select2-arrow').removeClass("select2-arrow").addClass("icon icon-chevron-down");
						$('#gbox_' + self.$grid.attr('id') + ' .subgrid-filter-container').find('span.select2-arrow').removeClass("select2-arrow").addClass("icon icon-chevron-down");
					});

					$('#' + this.headerID).on('change', '#' + this.controlID, function () {
						var $this = $(this),
							val = $this.prop("selectedIndex"),
							selector = "#" + headerID.replace("gbox", "jqgh") + "_" + this.id,
							$elCount = $(selector + " span.icon.icon-filter-collapse").length;
						self.val = $this.val();
						if (val != 0 && $elCount == 0) {
							$(selector).append("<span class='icon icon-filter-collapse'/>");
						} else if (val == 0) {
							$(selector + " span.icon.icon-filter-collapse").detach();
						}
						Base.prototype.triggerSearch.call(self);
					});
					return $el;
				};
				filter.prototype.getRule = function () {
					var val = $('#' + this.headerID).find("#" + this.controlID).val();
					self.val = val;
					if (val === "All" || $("#" + this.controlID).find('option').length === 0) {
						return {};
					} else {
						return { data: val, field: this.settings.fieldName, op: 'eq' };
					}
				};
				return filter;
			})(F.GridFilter);

		})(Grid.Filters || (Grid.Filters = {}));
	})(D.Grid || (D.Grid = {}));
})(Dragon, jQuery);