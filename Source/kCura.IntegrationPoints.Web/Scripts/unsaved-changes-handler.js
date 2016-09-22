var UnsavedChangesHandler = function () {
	var self = this;

	self._onUnloadConfirm = function (e) {
		// If we haven't been passed the event get the window.event
		e = e || window.event;

		var message = "If you leave this page, you'll lose all unsaved changes.  Continue?";

		// For IE6-8 and Firefox prior to version 4
		if (e) {
			e.returnValue = message;
		}

		// For Chrome, Safari, IE8+ and Opera 12+
		return message;
	};

	self.isDirty = ko.observable(false);

	self.register = function () {
		self.isDirty(true);
	};

	self.unregister = function () {
		self.isDirty(false);
	};

	self.isDirty.subscribe(function (newValue) {
		if (newValue) {
			$(window).on("beforeunload", self._onUnloadConfirm);
		} else {
			$(window).off("beforeunload");
		}
	});
};