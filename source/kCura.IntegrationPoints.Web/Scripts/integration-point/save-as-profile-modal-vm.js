var SaveAsProfileModalViewModel = function (okCallback, validateCallback) {
	var self = this;

	self.profileName = ko.observable();
	self.okCallback = okCallback;
	self.data = {};

	self.view = null;

	this.construct = function (view) {
		self.view = view;
	}

	this.open = function () {
		self.view.dialog('open');
	}

	this.ok = function () {
		var canClose = !!self.profileName() && self.profileName().length > 0;
		

		if (canClose) {
			self.okCallback(self.profileName());
			self.view.dialog('close');
		}
	}

	this.cancel = function () {
		self.view.dialog('close');
	}
}
