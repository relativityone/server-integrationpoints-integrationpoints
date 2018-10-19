var CreatingProductionSetViewModel = function (successCallback) {
	var self = this;

	var statusEnum = {
		INPROGRESS: 0,
		SUCCESS: 1,
		FAIL: 2
	};

	self.creatingStatus = ko.observable();
	self.productionSetName = null;
	self.newProductionSetId = null;
	self.workspaceArtifactId = null;
	self.credentials = null;
	self.federatedInstanceArtifactId = null;
	self.successCallback = successCallback;

	self.view = null;

	this.construct = function (view) {
		self.view = view;
	}

	this.updateModel = function (model) {
		self.model = model;
	}

	this.open = function (newProductionSetName, workspaceArtifactId, secretCatalog, federatedInstanceArtifactId, positionLeft) {
		self.creatingStatus(statusEnum.INPROGRESS);
		self.productionSetName = newProductionSetName;
		self.workspaceArtifactId = workspaceArtifactId;
		self.credentials = secretCatalog;
		self.federatedInstanceArtifactId = federatedInstanceArtifactId;

		self.view.dialog("open");
		document.querySelector('#creating-production-set-modal').parentNode.style.left = positionLeft;
		self.view.keypress(function (e) {
			if (e.which === 13) {
				self.create();
			}
		});

		self.createNewProductionSet();
	}

	this.createNewProductionSet = function () {

		var productionSetsPromise = IP.data.ajax({
			type: "POST",
			url: IP.utils.generateWebAPIURL("Production/CreateProductionSet", self.productionSetName, self.workspaceArtifactId, self.federatedInstanceArtifactId),
			data: self.credentials
		},
		false).fail(function (error) {
			self.creatingStatus(statusEnum.FAIL);
		});

		IP.data.deferred().all(productionSetsPromise).then(function (result) {
			self.newProductionSetId = result;

			if (!!self.newProductionSetId && self.newProductionSetId > 0) {
				self.creatingStatus(statusEnum.SUCCESS);
			} else {
				self.creatingStatus(statusEnum.FAIL);
			}
		});
	}

	this.close = function () {
		if (self.creatingStatus() === statusEnum.SUCCESS) {
			self.successCallback(self.newProductionSetId);
		}

		self.view.dialog("close");
	}
}
