(function ($, window) {
	var settings = {
		name: '_blank',
		specs: 'width=980,height=800,toolbar=0,menubar=1,resizable=1'
	};

	function getHelp() {
		// NOTE: the 'helpURL' variable must be set in the javascript context of the page using the method
		window.open(helpURL, settings.name, settings.specs);
	};
	$(document).on('click', '.legal-hold.icon-help', getHelp);

})(jQuery, window);