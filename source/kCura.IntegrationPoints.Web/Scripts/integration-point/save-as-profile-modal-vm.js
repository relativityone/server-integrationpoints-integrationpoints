var SaveAsProfileModalViewModel = function (okCallback) {
	var self = this;

	self.profileName = ko.observable();
	self.okCallback = okCallback;
	self.data = {};

	self.view = null;

	this.construct = function (view) {
		self.view = view;
	}

	this.open = function (name) {
		self.profileName(name);
		self.view.dialog("open");
		self.view.keypress(function (e) {
			if (e.which === 13) {
				self.ok();
			}
		});
	}

	this.ok = function () {
		var canClose = !!self.profileName() && self.profileName().length > 0;
		

		if (canClose) {
			self.okCallback(self.profileName());
			self.view.dialog("close");
		}
	}

	this.cancel = function () {
		self.view.dialog("close");
	}
}
