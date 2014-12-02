ko.bindingHandlers.select2 = {
	init: function (element) { },
	update: function (element) {
		//do this on update so the value get's persisted
		var $element = $(element);
		$element.select2({
			dropdownAutoWidth: false,
			dropdownCssClass: "filter-select",
			containerCssClass: "filter-container",
		});
		$element.parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon legal-hold icon-chevron-down");
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

		$el.datepicker(options);
		ko.bindingHandlers['value']['init'](element, valueAccessor, allBindingsAccessor);
		
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

		if (value - current !== 0) {
			$el.datepicker("setDate", value);
			ko.bindingHandlers['value']['update'](element, valueAccessor);
		}		
	}
};