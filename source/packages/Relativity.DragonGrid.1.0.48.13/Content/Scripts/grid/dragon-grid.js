//TODO: Move this out of the global scope
var storage = (function (storageLocation) {
	return {
		getItem: function (key) {
			var value = storageLocation.getItem(key);
			if (typeof value === "string") {
				try {
					value = JSON.parse(value);
				} catch (exp) {
					//this is actually a string!!
					value = value;
				}
			}
			return value;
		},
		setItem: function (key, value) {
			var vs = JSON.stringify(value);
			storageLocation.setItem(key, vs);
		}
	};
});


var Dragon = window.Dragon;
(function (D, $, storage, undefined) {
	(function (Grid) {
		var gridSettings = {
			classes: {
				selected: 'selected',
				expanded: 'expanded'
			},
			DATA_KEY: 'dragon-grid'
		};

		var instanceID = 0;

		var Helpers = {};
		(function (H) {
			function setupMassAction(settings) {
				$('#' + settings.ID).data('grid', this).on('click', function () {
					var $this = $(this),
						op = $this.attr('data-op'),
						id = $this.attr('id'),
						grid = $this.data('grid'),
						options = {
							ids: [],
							confirmText: settings.ConfirmText,
							gridType: settings.gridType,
							gridFilters: grid.$grid.getGridParam("postData"),
							resultURL: settings.ResultURL,
							grid: grid
						};
					isValidIDs = false;
					options.type = parseInt($this.parents('.mass-action').children('select').val());
					if (options.type === 1) {
						options.ids = grid.getSelected().keys;
					}
					options.objectTypeID = grid.objectTypeID;
					options.total = grid.$grid.getGridParam("records");

					if (options.type === 1) {
						//checked
						isValidIDs = (options.ids || []).length > 0;
					}
					else if (options.type === 2) {
						//all
						isValidIDs = options.total > 0;
					}
					else if (options.type === 3) {
						//priority?
						isValidIDs = $this.parents('.mass-action').children('select').find(':selected').text().match(/\d+/g)[0] > 0;
					}
					if (!isValidIDs) {
						Dragon.utils.showAlertDialog('No items have been selected for this operation.', {
							buttons: [
								{
									text: 'Ok',
									click: function () {
										$(this).dialog('close');
									},
									'class': 'button primary',
								}
							],
							title: 'Error'
						}, $('<div/>'));
						return;
					}
					Dragon.MassOperationsFactory(op, settings).execute(options);
				});
			};

			H.findObj = function (root, string) {
				return D.findObj(string, root);
			};

			H.findFunctionOrDefault = function (funcString, gridContext) {
				var dataType;

				try {
					var func = H.findFunction(funcString);
					if (typeof func === "function") {
						dataType = function (postData, _, rcnt, npage, adjust) {
							var ts = this;
							var done = function (data) {
								if (typeof data != "undefined") {
									ts.addJSONData(data, ts.grid.bDiv, rcnt, npage > 1, adjust);
								}
								gridContext.$grid.triggerHandler("jqGridLoadComplete", [data]);
								var lcf = $.isFunction(ts.p.loadComplete),
										lc = lcf ? ts.p.loadComplete : null;
								if (lcf) {
									lc.call(ts, data);
								}
								gridContext.$grid.triggerHandler("jqGridAfterLoadComplete", [data]);
								if (ts.p.scroll && npage === false) {
									ts.grid.populateVisible();
								}
							};
							
							var result = func.call(gridContext, postData, done);
							if (D.isPromise(result)) {
								result.then(function (r) {
									done(r);
								});
							} else if(typeof result != "undefined"){
								done(result);
							}
						};
					} else {
						dataType = funcString;
					}
				} catch (e) {
					//we are just a string so return JSON instead
					dataType = funcString;
				}
				return dataType;
			};

			H.findFunction = function findFunction(funcString) {
				if (typeof funcString === "function") {
					return funcString;
				}
				return H.findObj(window, funcString);
			};

			H.toggleSelected = function (selecting, event, rowID) {
				//DVB: this was commented out due to the fact the selector for the row wasn't specific enough
				//I know Id not specific enough crazy
				//if there is a bug with the selected row please let me know 

				//var $subGrid = $('#' + this.settings.ID + '_' + rowID).parents('.ui-subgrid').eq(0),
				//		$element;

				//if (event && event.srcElement) {
				//	$element = $(event.srcElement).parents('tr');
				//} else {
				//	$element = $('#' + rowID, this.$grid);
				//}

				//$element.toggleClass(gridSettings.classes.selected, selecting);
				//$subGrid.toggleClass(gridSettings.classes.selected, selecting);
			};

			H.sizeGrid = function sizeGrid($grid, isNonSubgrid) {
				var $gbox = $grid.closest(".ui-jqgrid"),
					$gridParent = $gbox.parent();
				// DO NOT REMOVE. This comes in handy for debugging grid resizing issues.
				//var $gridHeight = $grid.height(),
				//$gridHeightApi = $grid.getGridParam("height"),
				//$gboxHeight = $gbox.outerHeight(),
				//$gridHeaderHeight = $gbox.find(".ui-jqgrid-hdiv").height(),
				//$gridBodyHeight = $gbox.find(".ui-jqgrid-bdiv").height(),
				//$gridFooterHeight = $gbox.find(".ui-jqgrid-pager").height(),
				//gridCalculatedHeight = $gbox.outerHeight() - $grid.getGridParam("height");
				if ($gridParent.length) {
					$grid.setGridWidth($gridParent.width());

					if (typeof isNonSubgrid !== "undefined" && isNonSubgrid === false) {
						$grid.jqGrid('setGridHeight', 'auto');
					} else {
						$grid.jqGrid('setGridHeight', $gridParent.height() * 0.80);
					}
				}

				$('.ui-jqgrid-hdiv, .ui-jqgrid-view, .ui-jqgrid-bdiv').css('width', '100%');
				if ($grid.parents('.dragon-tab-panel').length !== 0) {
					$grid.parents('.ui-jqgrid-bdiv').css('height', '');
				}

			};

			H.setupFilterStyles = function ($headers) {
				var grid = this,
						$search = $headers.find('a.clearsearchclass').hide();
				//$search.each(function () {
				//	var $this = $(this);
				//	$this.text('X');
				//	$this.parent().siblings('.ui-search-input').append($(this));
				//});

				$('.ui-search-table').parent().css('padding', '');
				$('.ui-search-input').find('input').removeAttr('style');
				$('.ui-search-toolbar .ui-state-default.ui-th-column.ui-th-ltr:first-child > div').removeAttr('style');
				//$search.hide().parent().append('<span class="icon-search"></span>');

				$search.on('mousedown', function () {
					//mouse down > click for blurring yeah problems!!;
					$(this).click();
				});
				$headers.on("change keyup paste", '.ui-search-input >input', function (e) {
					var $this = $(this),
						$val = $this.val(),
						show = $val !== '',
						selector = "#" + $this.attr('id').replace("gs_", "jqgh_" + grid.$grid[0].id + "_"),
						$el = $(selector + " span.icon.icon-filter-collapse");

					$this.parent().parent().find('a.clearsearchclass').toggle(show);
					if (e.keyCode === D.keyCode.RETURN || $this.hasClass("hasDatepicker")) {
						if (show && $el.length === 0) {
							$(selector).append("<span class='icon icon-filter-collapse'/>");
						} else if (!show && e.keyCode != D.keyCode.BACK_SPACE) {
							$el.detach();
						}
					} else if (!show && $el.length > 0 && e.keyCode !== D.keyCode.BACK_SPACE) {
						$el.detach();
					}
				});
				$headers.on('click', 'a.clearsearchclass', function () {
					var ptr = $(this).parents("tr:first");
					$("td.ui-search-input input", ptr).val('').trigger("change");
					//grid.$grid[0].triggerToolbar();
				});

				$('.ui-grid-ico-sort.ui-icon-asc').removeClass('ui-icon-asc').addClass('icon-sort-asc');
				$('.ui-grid-ico-sort.ui-icon-desc').removeClass('ui-icon-desc').addClass('icon-sort-desc');

			};

			H.setUpPagerStyles = function () {
				var $pager = $('#pg_' + this.settings.pagerID);

				$pager.find('.ui-icon.ui-icon-seek-first').addClass('icon-paging-first');
				$pager.find('.ui-icon.ui-icon-seek-prev').addClass('icon-paging-previous');
				$pager.find('.ui-icon.ui-icon-seek-next').addClass('icon-paging-next');
				$pager.find('.ui-icon.ui-icon-seek-end').addClass('icon-paging-last');
			};

			H.setUpMassActions = function () {
				var self = this;
				$.each(this.settings.MassOperations, function (_, op) {
					setupMassAction.call(self, op);
				});
			};

			H.getPropertyFromDetails = function (data, property, nestedProperty) {
				if (data.hasOwnProperty(property)) {
					return data[property];
				} else if (data.hasOwnProperty(nestedProperty) && data.Data.hasOwnProperty(property)) {
					return data[nestedProperty][property];
				}
			};

			H.parseURL = function (urlFormat, rowData) {
				return D.formatString(urlFormat, rowData);
			};

			H.getData = function ($grid) {
				return $grid.data(gridSettings.DATA_KEY);
			};

		})(Helpers);
		Grid.utils = {
			getCommandColumn: function ($grid, rowID) {
				var $row = $($grid.jqGrid('getGridRowById', rowID)),
						$input = $('#jqg_' + $grid[0].id + '_' + rowID, $row);
				//look for the check box first if you can't find it then go for the radio
				if ($input.length === 0) {
					$input = $row.find('input[name="radio_' + $grid[0].id + '"]');
				}
				return $input;
			}
		};



		Grid.GridControl = (function (Helpers) {

			function initGrid() {
				var self = this;
				//here we go create a new grid object and have a whole bunch of fun!
				this.$grid.jqGrid({
					url: self.settings.url,
					mtype: 'Post',
					//serializeGridData: function (postData) {return JSON.stringify(postData);},
					datatype: Helpers.findFunctionOrDefault(self.settings.dataType || 'JSON', self),
					ajaxGridOptions: {
						contentType: "application/json; charset=utf-8",
						beforeSend: function () {
							var result = self.raise({ type: 'beforeSend', target: 'grid' }, arguments);
							if (typeof result === "undefined") {
								result = true;
							}
							return result;
						},
						complete: function () {
							self.raise({ type: 'complete', target: 'grid' }, arguments);

						},
						error: function () {
							self.raise({ type: 'error', target: 'grid' }, arguments);
						},
						timeout: self.settings.timeout,
						cache: false
					},
					jsonReader: {
						root: self.settings.jsonReaderOptions.root,
						page: self.settings.jsonReaderOptions.page,
						total: self.settings.jsonReaderOptions.total,
						records: self.settings.jsonReaderOptions.records,
					},
					colModel: self.settings.colModel,
					pager: '#' + self.settings.pagerID,
					rowNum: self.settings.rowNum,
					viewrecords: false,
					recordpos: 'left',
					//rowList: [10,20,30],
					//toppager: true,
					gridview: true,
					autoencode: true,
					postData: $.extend(true, {}, { gridID: self.settings.ID }, self.settings.gridPostData, self.settings.postData),
					scroll: self.settings.scroll,
					multiselect: self.settings.showCommandColumn,
					multiboxonly: true,
					rownumbers: false,
					hidegrid: false,
					sortable: false, //we don't want column reorder
					forceFit: true,
					pgbuttons: true,
					multiselectWidth: 25,
					pagerpos: 'left',
					pgtext: '<span id="' + this.settings.currPageID + '">{0}</span> of {1}',
					// all the events can be found here: http://www.trirand.com/jqgridwiki/doku.php?id=wiki:events
					beforeSelectRow: function () {
						var result = self.raise({ type: 'beforeSelectRow', target: 'grid' }, arguments);
						if (typeof result === "undefined") {
							result = true;
						}
						return result;
					},
					beforeProcessing: function () {
						//debugger;
					},
					onSelectRow: function () {
						return self.raise({ type: 'onSelectRow' }, arguments);
					},
					loadComplete: function (result) {
						var options = self.settings.jsonReaderOptions,
						    obj = {};
						for (var key in options) {
							if (options.hasOwnProperty(key)) {
								obj[key] = Helpers.findObj(result, options[key]);
							}
						}
						self.raise({ type: 'loadComplete', target: 'grid' }, [obj]);
					},
					beforeRequest: function () {
						self.raise({ type: 'beforeRequest', target: 'grid' }, arguments);
					},
					onSortCol: function () {
						self.raise({ type: 'onSortCol', target: 'grid' }, arguments);
					},
					onPaging: function () {
						self.raise({ type: 'onPaging', target: 'grid' }, arguments);
					},
					onSelectAll: function (rowIDs, selected) {
						var result = self.getCellValue(rowIDs);
						$.each(result || [], function (idx) {
							selectRow.call(self, idx + 1, selected, this)
						});
						self.raise({ type: 'onSelectRow', target: 'grid' }, [-1, selected]);
					},
					gridComplete: function () {
						self.raise({ type: 'gridComplete', target: 'grid' }, arguments);
					},
					subGrid: self.settings.subGrid,
					subGridOptions: {
						"plusicon": "icon icon-plus",
						"minusicon": "icon icon-minus",
						"openicon": "ui-icon-arrowreturn-1-e hidden",
						"reloadOnExpand": true,
						"selectOnExpand": false
					},
					subGridRowExpanded: function (subgridID, rowID) {
						var subgridTableID = subgridID + "_t",
								pagerID = "p_" + subgridTableID,
								subGrid,
								rowData = {},
								subGridSettings = $.extend(true, {}, self.settings.SubGridSettings),
								toggleID = subGridSettings.ID + 'FilterToggle';

						self.$grid.find('#' + rowID).addClass(gridSettings.classes.expanded); //this could cause problems with multi level!!
						$('#' + subgridID).html('<div>' + subGridSettings.title + '<span class="filter-toggle icon icon-search-minus" id="' + toggleID + '"></span> </div><table id="' + subgridTableID + '"></table><div id="' + pagerID + '"></div>');

						subGridSettings.toggleID = toggleID;
						subGridSettings.ID = subgridTableID;
						subGridSettings.subGrid = false;
						subGridSettings.pagerID = pagerID;
						subGridSettings.showCommandColumn = false;

						rowData = self.getCellValue(rowID, $.map(self.getColumns(), function (c) { return c.name }))[0];
						subGridSettings.url = Helpers.parseURL(subGridSettings.url, rowData);

						subGrid = new control(subGridSettings);
						if (self.$grid.find('#' + rowID).hasClass(gridSettings.classes.selected)) {
							$('#' + subGridSettings.ID).parents('.ui-subgrid').addClass(gridSettings.classes.selected);
						}
						self.subGrids[subGridSettings.ID] = subGrid;
						self.raise({ type: 'subGridRowExpanded', target: 'grid' }, [subgridID, rowID, subGrid]);

						setupGridEvents.call(subGrid);
					},
					subGridRowColapsed: function (subgridID, rowID) {
						var subGrid = self.subGrids[subgridID];
						self.raise({ type: 'subGridRowColapsed', target: 'grid' }, [subgridID, rowID, subGrid]);
						delete self.subGrids[subgridID];
						self.$grid.find('#' + rowID).removeClass(gridSettings.classes.expanded); //this could cause problems with multi level!!
						//Helpers.sizeGrid(self.$grid);
					}
				});

				//Add custom buttons to top pager.
				var pager = '#' + self.settings.pagerID,
					pagerNavID = self.settings.pagerID + "Nav";
				self.$grid.jqGrid('navGrid', pager, {
					refresh: false,
					search: false,
					edit: false,
					view: false,
					del: false,
					add: false,
					afterRefresh: function () {
						resetSelectedItemCount();
					}
				}).navButtonAdd(pager, {
					//buttonicon: "pager-nav",
					//onClickButton: function () {
					//	//self.$grid[0].toggleToolbar();
					//	//sizeGrid();
					//},
					position: "last",
					//title: "Toggle filters",
					cursor: "none",
					caption: '',
					id: pagerNavID
				});

				$("div.ui-pg-div span.ui-icon.ui-icon-newwin").remove();

				var $selContainer = $('#' + pagerNavID + ' div.ui-pg-div'),
					$sel = $('<select>').appendTo($selContainer);


				if (typeof $sel !== "undefined") {
					$.each(self.settings.pager || [], function () {
						$sel.append($("<option>").attr('value', this.size).text(this.display));
					});

					if (typeof self.settings.subGrid !== "undefined" && self.settings.subGrid === false) {
						$sel.select2({
							containerCssClass: "subgrid-pager-container",
							dropdownCssClass: "subgrid-select",
							dropdownAutoWidth: false
						});
					} else {
						$sel.select2({
							containerCssClass: "pager-container",
							dropdownCssClass: "pager-select",
							dropdownAutoWidth: false
						});
					}

					$selContainer.find('span.select2-arrow').removeClass("select2-arrow").addClass("icon icon-chevron-down");

					$sel.on("change", function (e) {
						self.$grid.setGridParam({ rowNum: $(this).val() }).trigger("reloadGrid");
					});
				}

				$(window).resize(function () {
					Helpers.sizeGrid(self.$grid, self.settings.subGrid);
				});

				Helpers.sizeGrid(self.$grid);
				//on tab expansion trigger reload
				var $el = $('[href="#' + self.$grid.parents('.dragon-tab-panel').attr('id') + '"]'); //second iteration I still don't like it may have to be a bit smarter here
				$el.on('click', function () {
					//this has to be here to ensure it's the last thing that is run :(
					setTimeout(function () {
						self.raise({ type: 'gridVisible', target: 'grid' }, arguments);
					}, 1);

				});
				//debugger;
				//$(this.$grid[0].grid.bDiv).on('change', 'input[type="radio"]', function (e) {
				//	var row = $(e.currentTarget).data('row');
				//	self.$grid.find('.jqgrow.selected').removeClass(gridSettings.classes.selected);
				//	self.$grid.jqGrid('resetSelection');
				//	self.$grid.jqGrid('setSelection', row, true);
				//});
			};

			function setupFilterToggle() {
				var $filterToggle = $('#' + this.settings.toggleID),
						self = this,
						$toggleSpan = $filterToggle.find('>span');

				$filterToggle.on('click', function () {
					var result = self.raise({ type: 'beforeFilterToggle', target: 'grid' }, arguments);
					$toggleSpan.toggleClass('icon-filter-collapse');
					$toggleSpan.toggleClass('icon-filter-expand');

					if (typeof result === "undefined") {
						result = true;
					}
					if (result) {
						self.$grid[0].toggleToolbar();
						self.raise({ type: 'afterFilterToggle', target: 'grid' }, arguments);
					}
				});
			}

			function setupResetCol() {
				var $reset = $('#' + this.settings.colResetID),
						self = this;
				$reset.on('click', function () {
					self.resetColWidth();
				});
			}

			function appendFilters() {
				var postData = this.$grid.getGridParam('postData'),
						searchData,
						getCustomFilterData,
						exportedFilters = [];

				if (typeof postData.filters === "string" && postData.filters !== '') {
					searchData = JSON.parse(postData.filters);
				} else {
					searchData = $.extend({}, { groupOp: 'AND', rules: [] }, postData.filters);
				}

				exportedFilters = $.map(this.customFilters, function (filter) {
					return filter.getRule();
				});
				Array.prototype.push.apply(searchData.rules, exportedFilters.concat(this.layoutFilters));
				this.$grid.jqGrid('setGridParam', { postData: { filters: searchData } });

			};

			function setupFilterToolbar() {
				var self = this,
					$header;
				//Set up the filter toolbar.
				self.$grid.jqGrid('filterToolbar', {
					stringResult: true,
					defaultSearch: 'cn',
					beforeSearch: function () {
						//This code takes any custom filter data and appends it to the postData so that it gets sent along to the server.
						appendFilters.call(self);
						return false;
					},
					afterSearch: function () {
						//resetSelectedItemCount();
					}
				});
				var getCustomFilterData = function (customFilters) {
					return $.map(self.customFilters, function (filter) {
						return filter.getRule();
					});
				};
				$header = self.$grid.parents('.ui-jqgrid').find(".ui-jqgrid-htable>thead>.ui-search-toolbar>th");
				self.customFilters = $.map(self.$grid.jqGrid('getGridParam', 'colModel') || [], function (column, idx) {

					if (column != undefined && column.search == false) {
						column.filterType = column.filterType || {};
						var func = Helpers.findFunction(column.filterType.filterType),
								thisFilter,
								filterSettings = column.filterType;
						filterSettings.gridID = self.settings.ID;
						filterSettings.fieldName = column.name;
						filterSettings.colID = idx;
						filterSettings.callbacks = {
							on: function () {
								return self.on.apply(self, arguments);
							}
						};
						if (typeof func === 'function') {
							thisFilter = new func(filterSettings);
						} else {
							thisFilter = undefined;
						}
						if (thisFilter != undefined) {
							$header.eq(idx).find(">div").append(thisFilter.getHeaderElement({ subGrid: self.settings.subGrid }));
							if ($.isFunction(thisFilter.postAppend)) {
								thisFilter.postAppend();
							}
							return thisFilter;
						}
					}
				});
				Helpers.setupFilterStyles.call(self, $header);

			};

			function setupFiltersAndFormatters() {
				var self = this;

				$.each(self.settings.colModel, function (_, column) {
					column.gridFormatter = column.gridFormatter || { formatter: column.formatter };
					var func = Helpers.findFunction(column.gridFormatter.formatter),
							filtertype = "search";

					if (typeof func === "function") {
						column.formatter = func;
					}
					if (column.filterType) {
						filtertype = column.filterType.filterType;
					}
					switch (filtertype.toLowerCase()) {
						case 'search':
							column.search = true;
							column.stype = 'text';
							break;
						default:
							column.search = false;
							break;
					}
				});
			};

			function selectRow(rowID, selecting, rowData) {
				var key = this.settings.key,
						cols, $cbox;
				//make sure we save ourselves from selectALL
				$cbox = Grid.utils.getCommandColumn(this.$grid, rowID);

				if ($cbox.prop('disabled')) {
					$cbox.prop('checked', false);
					selecting = false;
					$cbox.parent('tr').toggleClass(gridSettings.classes.selected, selecting);
				} else {
					$cbox.prop('checked', selecting);
				}
				if ($cbox.is(":radio")) {
					selecting = true;
					$cbox.prop('checked', selecting);
					this.$grid.find('.' + gridSettings.classes.selected).removeClass(gridSettings.classes.selected);
				}
				Helpers.toggleSelected.call(this, selecting, {}, rowID);
				if (selecting) {
					this.addItem(rowData[key], rowData);
				} else {
					this.removeItem(rowData[key]);
				}
			}

			function setupGridEvents() {
				var self = this;
				this.on('onSelectRow', function (rowID, selecting, event) {
					if (rowID === -1) {
						return; //select all was hit
					}
					var value = this.getCellValue(rowID)[0];
					selectRow.call(this, rowID, selecting, value);
					return false;
				});

				this.on('beforeSelectRow', function (rowID, e) {
					//only select if the ement is an input and is not disabled!!
					return e.target.tagName === "INPUT" && Grid.utils.getCommandColumn(this.$grid, rowID).prop('disabled') === false;
				});

				this.on("beforeSend", function (jqXHR, request) {
					if (this.settings.loadDataOnInit === false) {
						jqXHR.abort();
						this.settings.loadDataOnInit = true;
						return false;
					}
					var postData = self.$grid.getGridParam('postData');
					postData.selected = self.getSelected().keys;
					request.data = JSON.stringify(postData);
					return true;
				});

				this.on("complete", function () {
					var $pager = $('#' + this.settings.pagerID),
						page = this.$grid.jqGrid('getGridParam', 'page');
					$('#' + this.settings.currPageID).text(page);
					//this.$grid.getGridParam('lastpage') <= 1 ? $pager.hide() : $pager.show();
					this.settings.loadDataOnInit = true;
					this.resize(this.settings.subGrid);
				});

				this.on('beforeRequest', function () {
					var filters;
					if (self.firstLoad === true) {
						filters = storage.getItem(getKey.call(self));
						if (typeof filters === "object") {
							self.layoutFilters = filters;
							appendFilters.call(self);
						}
					}
					$("body").addClass("grid-request-wait");
				});

				this.on('loadComplete', function (records) {
					var root = records.root;
					this.records = root;
					if (records.selection && this.settings.addSelectedItems) {
						setSelectedItemsOnGrid(this, root, records.selection);
					}

					updateSelectedItemsOnGrid(this, root, records.selectedKeys);
				});

				this.on("gridVisible", function () {
					if (self.settings.loadDataOnInit === false) {
						self.reload();
					}
					this.resize();
				});

				this.on("afterFilterToggle", function () {
					this.resize();
				});

				this.on("gridComplete", function () {
					//$('#reportsGrid-pager_center').text(self.$grid.jqGrid('getGridParam', 'records') + ' items total');
					self.firstLoad = false;
					self.resize();
					$("body").removeClass("grid-request-wait");
				});

				this.on('onSortCol', function (field, _, sort) {
					var $th = $('#gbox_' + this.settings.ID).find('.ui-th-column.ui-th-ltr');
					$th.find('.ui-grid-ico-sort').removeClass('sort-active');
					$th.parent().find('#' + this.settings.ID + '_' + field).find('[sort="' + sort + '"]').addClass('sort-active');

				});

				this.on("gridResize", function () {
					var $gbox = $('#gbox_' + this.settings.ID), sub;
					$.each(this.cols, function () {
						$('#' + this.id, $gbox).css({ 'width': this.width });
					});

					for (sub in this.subGrids) {
						if (this.subGrids.hasOwnProperty(sub)) {
							this.subGrids[sub].raise({ type: 'gridResize', target: 'grid' }, arguments)
						}
					}
					this.resize(); // ? optional
				});
			};

			function getKey() {
				return this.settings.ID;
			}

			function updateSelectedItemsOnGrid(grid, rows, selectedKeys) {
				rows = rows || [];
				var key = grid.settings.key;
				var RAISE_ONSELECT_EVENT = true;

				//The server will do the join against what the grid thinks should be selected and
				//what the server thinks should be selected, this allows for finer filtering
				//we trust the server code and just blindly update our internal list and raise the correct events;
				grid.clearSelected();
				grid.addSelected(selectedKeys);

				$.each(rows, function (idx, entry) {
					if (grid.isSelected(entry[key])) {
						grid.$grid.jqGrid('setSelection', idx + 1, RAISE_ONSELECT_EVENT);
					}
				});
			}

			function setSelectedItemsOnGrid(grid, rows, ids) {
				rows = rows || [];
				var key = grid.settings.key;
				$.each(ids, function () {
					grid.addItem(this, {});
				});
			}

			function getDefaultGridWidths() {
				var self = this;
				$('#gbox_' + this.settings.ID).find('.ui-jqgrid-labels > th').each(function () {
					self.cols.push({
						id: this.id,
						width: this.style.width
					});
				});
			}

			function control(gridSettings) {
				//gridSettings = $.extend({}, gridSettings); //we want to create a clean copy not mess with original reference
				gridSettings.MassOperations = gridSettings.MassOperations || [];
				gridSettings.colModel = gridSettings.colModel || [];
				this.settings = gridSettings;
				//TODO: not save all of this data in the object but in the $grid's data
				if (this.settings.SubGridSettings && typeof this.settings.SubGridSettings === "object") {
					this.settings.subGrid = true;
				}
				this.settings.ID = this.settings.ID || this.settings.iD; //this is bad :(
				if (this.settings.pagerID === '-pager') {
					this.settings.pagerID = this.settings.ID + this.settings.pagerID;
				}
				this.settings.toggleID = this.settings.toggleID || this.settings.ID + 'FilterToggle';
				this.settings.colResetID = this.settings.colResetID || this.settings.ID + 'ColReset';
				this.objectTypeID = this.settings.objectTypeID;
				this.numSelectedRows = 0;
				this.$grid = $('#' + this.settings.ID);
				this.events = {};
				this.instanceID = instanceID++;
				this.subGrids = {};
				this.selectedItems = {};
				this.settings.currPageID = 'pgcurr_' + this.settings.pagerID;
				this.layoutFilters = [];
				this.firstLoad = true;
				this.customFilters = [];
				this.initialData = {};
				this.cols = [];
				this.records = {};
				this.$grid.data(gridSettings.DATA_KEY, {
					selectedItems: {}
				});
				//this has to be first to ensure we capture every event
				setupGridEvents.call(this);
				setupFiltersAndFormatters.call(this);
				initGrid.call(this);
				setupFilterToolbar.call(this);
				setupResetCol.call(this);

				if (this.settings.showFilterToggle) {
					setupFilterToggle.call(this);
				}
				Helpers.setUpPagerStyles.call(this);
				Helpers.setUpMassActions.call(this);
				getDefaultGridWidths.call(this);
			};

			control.prototype.on = function (name, func) {
				if (typeof this.events[name] === "undefined") {
					this.events[name] = [];
				}
				this.events[name].push(func);
			};

			control.prototype.raise = function (e, args) {
				var func = this.events[e.type],
						self = this,
						result = undefined;

				if (typeof func !== "undefined") {
					result = true;
					$.each(func, function () {
						if (typeof this === "function") {
							result &= this.apply(self, args);
						}
					});
					result = result === 1;
				}
				return result;
			};

			control.prototype.reload = function () {
				this.$grid.trigger("reloadGrid");
			};

			control.prototype.addItem = function (key, row) {
				if (this.settings.singleSelect) {
					this.clearSelected();
				}
				if ($.isArray(row)) {
					row = row[0];
				}
				this.selectedItems[key] = row;
			};

			control.prototype.addSelected = function (ids) {
				var self = this;
				ids = ids || [];
				if (!$.isArray(ids)) {
					ids = [ids];
				}
				$.each(ids, function () {
					self.addItem(this, {});
				});
			};

			control.prototype.removeItem = function (key) {
				if (typeof key === "undefined") {
					this.selectedItems = {};
				}
				delete this.selectedItems[key];
			};

			control.prototype.isSelected = function (key) {
				return this.selectedItems[key] !== undefined;
			};

			control.prototype.getSelected = function (key) {
				if (typeof key !== "undefined") {
					return {
						keys: [key],
						data: [this.selectedItems[key]]
					};
				}

				//I think we should start caching this stuff instead of 
				//doing such a tight loop everytime
				var keys = [], data = {};
				for (var k in this.selectedItems) {
					if (this.selectedItems.hasOwnProperty(k)) {
						keys.push(k);
						data[k] = this.selectedItems[k];
					}
				}
				return { keys: keys, data: data };
			};

			control.prototype.getSelectedCount = function () {
				return Object.keys(this.selectedItems).length;
			};

			control.prototype.getCellValue = function (rowIDs, columns) {
				var self = this,
						objList = [];

				if (!$.isArray(rowIDs)) {
					rowIDs = [rowIDs];
				}
				if (!$.isArray(columns)) {
					columns = [columns];
				}
				var records = this.records;
				for (var i = 0; i < rowIDs.length; i++) {
					objList.push(records[rowIDs[i] - 1])
				}
				return objList;
			};

			control.prototype.getColumns = function () {
				return $.map(this.settings.colModel, function (model) {
					return {
						label: model.label,
						name: model.name
					};
				});
			};

			control.prototype.appendFiltersAndRefresh = function (filters, persist) {
				if (typeof filters === "object") {
					filters = [filters];
				}
				if (persist === true) {
					storage.setItem(getKey.call(this), filters);
				}

				this.layoutFilters = filters;
				this.$grid[0].triggerToolbar();
			};

			control.prototype.resize = function (IsNonSubgrid) {
				Helpers.sizeGrid(this.$grid, IsNonSubgrid);
			};

			//should this raise and event?
			control.prototype.show = function () {
				this.$grid.closest(".ui-jqgrid").show();
			};
			//should this raise and event?
			control.prototype.hide = function () {
				this.$grid.closest(".ui-jqgrid").hide();
			};

			control.prototype.clearSelected = function () {
				for (var k in this.selectedItems) {
					if (this.selectedItems.hasOwnProperty(k)) {
						delete this.selectedItems[k];
					}
				}
			};

			control.prototype.remove = function (settings) {
				try {
					//TODO: instead of doing try/catch, check to see if the grid has been inited before unloading
					this.$grid.jqGrid('GridUnload');
				} catch (e) { }

			};

			control.prototype.resetColWidth = function () {
				this.raise({ type: 'gridResize', target: 'grid' }, arguments)
			};

			return control;
		})(Helpers);

	})(D.Grid || (D.Grid = {}));

})(Dragon || (Dragon = {}), jQuery, storage(sessionStorage));

