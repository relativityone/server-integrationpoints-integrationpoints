
ko.bindingHandlers.select2 = {
	init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
		ko.utils.domNodeDisposal.addDisposeCallback(el, function () {
			$(el).select2('destroy');
		});

		var allBindings = allBindingsAccessor(),
			select2 = ko.utils.unwrapObservable(allBindings.select2),
			$element = $(el);

		$element.select2(
			$.extend({
				dropdownAutoWidth: false,
				dropdownCssClass: "filter-select",
				containerCssClass: "filter-container",
				minimumResultsForSearch: Infinity
			}, valueAccessor())
		);

		if (viewModel.disable) {
			$element.select2('disable');
		}

		$element.parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon legal-hold icon-chevron-down");
	},
	update: function (el, valueAccessor, allBindingsAccessor, viewModel) {
		var allBindings = allBindingsAccessor();
		if ("value" in allBindings) {
			$(el).select2("val", allBindings.value());
		} else if ("selectedOptions" in allBindings) {
			var converted = [];
			var textAccessor = function (value) { return value; };
			if ("optionsText" in allBindings) {
				textAccessor = function (value) {
					var valueAccessor = function (item) { return item; }
					if ("optionsValue" in allBindings) {
						valueAccessor = function (item) { return item[allBindings.optionsValue]; }
					}
					var items = $.grep(allBindings.options(), function (e) { return valueAccessor(e) == value });
					if (items.length == 0 || items.length > 1) {
						return "UNKNOWN";
					}
					return items[0][allBindings.optionsText];
				}
			}
			$.each(allBindings.selectedOptions(), function (key, value) {
				converted.push({ id: value, text: textAccessor(value) });
			});
			$(el).select2("data", converted);
		}
	}
};

ko.bindingHandlers.select2searchable = {
	init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
		ko.utils.domNodeDisposal.addDisposeCallback(el, function () {
			$(el).select2('destroy');
		});

		var allBindings = allBindingsAccessor(),
			select2 = ko.utils.unwrapObservable(allBindings.select2searchable),
			$element = $(el);

		$element.select2(
			$.extend({
				dropdownAutoWidth: false,
				dropdownCssClass: "filter-select",
				containerCssClass: "filter-container",
			}, valueAccessor())
		);

		if (viewModel.disable) {
			$element.select2('disable');
		}

		$element.parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon legal-hold icon-chevron-down");
	},
	update: function (el, valueAccessor, allBindingsAccessor, viewModel) {

		var allBindings = allBindingsAccessor();
		if ("value" in allBindings) {
			var value = allBindings.value();
			if (value) {
				$(el).val(value).trigger('change');
			}
		} else if ("selectedOptions" in allBindings) {
			var converted = [];
			var textAccessor = function (value) { return value; };
			if ("optionsText" in allBindings) {
				textAccessor = function (value) {
					var valueAccessor = function (item) { return item; }
					if ("optionsValue" in allBindings) {
						valueAccessor = function (item) { return item[allBindings.optionsValue]; }
					}
					var items = $.grep(allBindings.options(), function (e) { return valueAccessor(e) == value });
					if (items.length == 0 || items.length > 1) {
						return "UNKNOWN";
					}
					return items[0][allBindings.optionsText];
				}
			}
			$.each(allBindings.selectedOptions(), function (key, value) {
				converted.push({ id: value, text: textAccessor(value) });
			});
			$(el).select2("data", converted);
		}
	}
};

ko.bindingHandlers.select2lazySearchable = {
	init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
		ko.utils.domNodeDisposal.addDisposeCallback(el, function () {
			$(el).select2('destroy');
		});

		var allBindings = allBindingsAccessor(),
			select2 = ko.utils.unwrapObservable(allBindings.select2lazySearchable),
			$element = $(el);

		var optionsUrl = IP.data.cacheBustUrl(allBindings.optionsUrl);
		var itemDetailsIdParameterName = allBindings.itemDetailsIdParameterName;

		var optionsPlaceholder = 'Select ...';
		if (!!allBindings.optionsCaption) {
			optionsPlaceholder = allBindings.optionsCaption;
		}


		var valuePropertyName = allBindings.valuePropertyName;
		var textPropertyName = allBindings.textPropertyName;
		var mapItemData = function(item) {
			if (!!valuePropertyName) {
				item.id = item[valuePropertyName];
			}
			if (!!textPropertyName) {
				item.text = item[textPropertyName];
			}
			return item;
		};

		$element.select2(
			$.extend({
				placeholder: optionsPlaceholder,
				dropdownAutoWidth: false,
				dropdownCssClass: "filter-select",
				containerCssClass: "filter-container",
				ajax: {
					url: optionsUrl,
					dataType: 'json',
					quietMillis: 150,
					cache: true,
					data: function (term, page) {
						return {
							search: term,
							page: page || 1
						}
					},
					results: function (data) {
						var select2Data = $.map(data.results, mapItemData);
						return {
							results: select2Data,
							more: data.hasMoreResults
						};
					}
				},
				initSelection: function (element, callback) {
					var id = $(element).val();
					if (id !== "") {
						var dataRequest = {};
						dataRequest[itemDetailsIdParameterName] = id;

						$.ajax({
							url: optionsUrl,
							dataType: "json",
							data: dataRequest
						}).done(function(data) {
							callback(mapItemData(data));
						});
					}
				},
			}, valueAccessor())
		);

		if (viewModel.disable) {
			$element.select2('disable');
		}

		$element.parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon legal-hold icon-chevron-down");
	},
	update: function (el, valueAccessor, allBindingsAccessor, viewModel) {
		var allBindings = allBindingsAccessor();
		if ("value" in allBindings) {
			var value = allBindings.value();
			if (value) {
				$(el).val(value).trigger('change');
			}
		}
	}
};

ko.bindingHandlers.select2lazySearchableView = {
	init: function (el, valueAccessor, allBindingsAccessor, viewModel) {
		ko.utils.domNodeDisposal.addDisposeCallback(el, function () {
			$(el).select2('destroy');
		});

		var allBindings = allBindingsAccessor(),
			select2 = ko.utils.unwrapObservable(allBindings.select2lazySearchable),
			$element = $(el);

		var itemDetailsIdParameterName = allBindings.itemDetailsIdParameterName;

		var optionsPlaceholder = 'Select ...';
		if (!!allBindings.optionsCaption) {
			optionsPlaceholder = allBindings.optionsCaption;
		}


		var valuePropertyName = allBindings.valuePropertyName;
		var textPropertyName = allBindings.textPropertyName;
		var mapItemData = function(item) {
			if (!!valuePropertyName) {
				item.id = item[valuePropertyName];
			}
			if (!!textPropertyName) {
				item.text = item[textPropertyName];
			}
			return item;
		};

		$element.select2(
			$.extend({
				placeholder: optionsPlaceholder,
				dropdownAutoWidth: false,
				dropdownCssClass: "filter-select",
				containerCssClass: "filter-container",
				ajax: {
					url: IP.data.cacheBustUrl(allBindings.optionsUrl),
					dataType: 'json',
					quietMillis: 150,
					cache: true,
					data: function (term, page) {
						return {
							search: term,
							page: page || 1
						}
					},
					results: function (data) {
						var select2Data = $.map(data.results, mapItemData);
						return {
							results: select2Data,
							more: data.hasMoreResults
						};
					}
				},
				initSelection: function (element, callback) {
					var id = $(element).val();
					if (id !== "") {
						var dataRequest = {};
						dataRequest[itemDetailsIdParameterName] = id;

						var optionsUrl = allBindings.optionsUrl.trim('/') + '/' + id;

						$.ajax({
							url: IP.data.cacheBustUrl(optionsUrl),
							dataType: "json",
							data: dataRequest
						}).done(function(data) {
							callback(mapItemData(data));
						});
					}
				},
			}, valueAccessor())
		);

		if (viewModel.disable) {
			$element.select2('disable');
		}

		$element.parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon legal-hold icon-chevron-down");
	},
	update: function (el, valueAccessor, allBindingsAccessor, viewModel) {
		var allBindings = allBindingsAccessor();
		if ("value" in allBindings) {
			var value = allBindings.value();
			if (value) {
				$(el).val(value).trigger('change');
			}
		}
	}
};

ko.bindingHandlers.datepicker = {
	init: function (element, valueAccessor, allBindingsAccessor) {
		//initialize datepicker with some optional options
		var options = allBindingsAccessor().datepickerOptions || {},
			$el = $(element);

		options.onSelect = function (date) {
			var observable = valueAccessor();
			observable(date);
		};

		//$el.on('change', function (e) {
		//	var date = $(e.srcElement).val();
		//	var observable = valueAccessor();
		//	observable(date);
		//});

		$el.datepicker(options);

		//handle disposal (if KO removes by the template binding)
		ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
			$el.datepicker("destroy");
		});

	},
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor()),
			$el = $(element);

		//handle date data coming via json from Microsoft
		if (String(value).indexOf('/Date(') == 0) {
			value = new Date(parseInt(value.replace(/\/Date\((.*?)\)\//gi, "$1")));
		}

		var current = $el.datepicker("getDate");
		if (value - current !== 0 && !/Invalid|NaN/.test(new Date(value))) {
			$el.datepicker("setDate", value);
		}
	}
};

ko.bindingHandlers.doubleClick = {
	init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
		var handler = valueAccessor(),
			delay = 200,
			clickTimeout = false;

		$(element).on('dblclick', function () {
			handler.call(viewModel);
		});
	}
};

ko.bindingHandlers.hidden = (function () {
	function setVisibility(element, valueAccessor) {
		var hidden = ko.unwrap(valueAccessor());
		$(element).css('visibility', hidden ? 'hidden' : 'visible');
	}
	return { init: setVisibility, update: setVisibility };
})();