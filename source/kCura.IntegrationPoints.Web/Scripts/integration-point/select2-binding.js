ko.bindingHandlers.select2 = {
	init: function (element) { },
	update: function (element) {
		//do this on update so the value get's persisted
		var $element = $(element);
		debugger;
		$element.select2({
			dropdownAutoWidth: false,
			dropdownCssClass: "filter-select",
			containerCssClass: "filter-container",
		});
		$element.parent().find('.filter-container span.select2-arrow').removeClass("select2-arrow").addClass("icon icon-chevron-down");
	}
};