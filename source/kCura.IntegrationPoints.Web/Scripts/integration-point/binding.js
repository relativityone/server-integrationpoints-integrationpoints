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

ko.bindingHandlers.datePicker = {
	init: function (element) {
		$(element).datepicker({
			beforeShow: function (el, inst) {
				if ($(el).attr('readonly')) {
					return false;
				}
				inst.dpDiv.css({ marginTop: -el.offsetHeight + 'px', marginLeft: el.offsetWidth + 5 + 'px' });
				return true;
			},
			onSelect: function () {
				//get the shim to work properly
				$(this).blur();
			}
		});
	},
	update: function (element) {
		
	}
};