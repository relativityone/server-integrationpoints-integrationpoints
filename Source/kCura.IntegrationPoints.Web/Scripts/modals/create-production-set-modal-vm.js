var CreateProductionSetViewModel = function (successCallback, failCallback) {
	var self = this;

	self.newProductionSetName = ko.observable();
	self.newProductionSetId = ko.observable();
    self.workspaceArtifactId = ko.observable();
    self.credentials = ko.observable();
    self.federatedInstanceArtifactId = ko.observable();
    self.successCallback = successCallback;
    self.failCallback = failCallback;
	self.model = {
		propName: "New Production Name:"
	}

	self.view = null;

	this.construct = function (view) {
		self.view = view;
	}

	this.updateModel = function (model) {
		self.model = model;
	}

	this.open = function (workspaceArtifactId, secretCatalog, federatedInstanceArtifactId) {
	    self.workspaceArtifactId(workspaceArtifactId);
	    self.credentials(secretCatalog);
	    self.federatedInstanceArtifactId(federatedInstanceArtifactId);
	    self.newProductionSetName("");

		self.view.dialog("open");
		self.view.keypress(function (e) {
			if (e.which === 13) {
			    self.create();
			}
		});
	}

	this.create = function () {
	    var canCreate = !!self.newProductionSetName() && self.newProductionSetName().length > 0
	        && !!self.workspaceArtifactId() && self.workspaceArtifactId() > 0;

	    if (canCreate) {
	        var productionSetsPromise = IP.data.ajax({
	            type: "POST",
	            url: IP.utils.generateWebAPIURL("Production/CreateProductionSet", self.newProductionSetName(), self.workspaceArtifactId(), self.federatedInstanceArtifactId()),
	            data: self.credentials()
	        }).fail(function (error) {
	            self.view.dialog("close");
	            self.failCallback();
	        });
            
	        IP.data.deferred().all(productionSetsPromise).then(function (result) {
	            self.newProductionSetId(result);

	            var productionCreated = !!self.newProductionSetId() && self.newProductionSetId() > 0;

	            self.view.dialog("close");
	            if (productionCreated) {
	                self.successCallback(self.newProductionSetId());
	            } else {
	                self.failCallback();
	            }
	        });
	    }
	}

	this.cancel = function () {
		self.view.dialog("close");
	}
}
