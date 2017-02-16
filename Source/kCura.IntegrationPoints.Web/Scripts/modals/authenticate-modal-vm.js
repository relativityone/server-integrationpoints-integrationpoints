var AuthenticateViewModel = function (okCallback, cancelCallback) {
	var self = this;

	self.id = ko.observable();
	self.secret = ko.observable();
	self.okCallback = okCallback;
	self.cancelCallback = cancelCallback;
	self.model = {
		idPropName: "Client Id:",
		secretPropName: "Client Secret:"
	}

	self.view = null;

	this.construct = function (view) {
		self.view = view;
	}

	this.updateModel = function (model) {
		self.model = model;
	}

	this.open = function (secretCatalog) {
		if (secretCatalog) {
			var secret = JSON.parse(secretCatalog);
			self.id(secret.ClientId);
			self.secret(secret.ClientSecret);
		}
		else {
			self.id("");
			self.secret("");
		}
		self.view.dialog("open");
		self.view.keypress(function (e) {
			if (e.which === 13) {
				self.ok();
			}
		});
	}

	this.ok = function () {
		var canClose = !!self.id() && self.id().length > 0 &&
		 !!self.secret() && self.secret().length > 0;


		if (canClose) {
			self.okCallback(self.id(), self.secret());
			self.view.dialog("close");
		}
	}

	this.cancel = function () {
		self.cancelCallback();
		self.view.dialog("close");
	}
}
