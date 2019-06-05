var CreateProductionSetViewModel = function (createCallback) {
	var self = this;

	self.newProductionSetName = ko.observable();
	self.createCallback = createCallback;
	self.model = {
		propName: "New Production Set Name:"
	}

	self.view = null;

	this.newProductionSetName.extend({
		required: true
	});

	this.construct = function (view) {
		self.view = view;
	}

	this.updateModel = function (model) {
		self.model = model;
	}

	this.open = function () {
		self.newProductionSetName("");
		self.newProductionSetName.isModified(false);

		self.view.dialog("open");
		self.view.keyup(function (e) {
			if (e.which == 13) {
				if(e.handled !== true){
					document.activeElement.blur();
					self.create();
					e.handled = true;
				}
			}
		});
	}

	this.create = function () {
		if (!self.newProductionSetName.isValid()) {
			this.newProductionSetName.notifySubscribers();
			return;
		}

		var positionLeft = document.querySelector('#create-production-set-modal').parentNode.style.left;
		self.view.dialog("close");

		self.createCallback(self.newProductionSetName(), positionLeft);
	}

	this.cancel = function () {
		self.view.dialog("close");
	}
}
