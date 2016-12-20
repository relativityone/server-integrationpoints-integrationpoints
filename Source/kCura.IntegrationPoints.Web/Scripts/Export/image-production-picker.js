var ImageProductionPickerViewModel = function(okCallback, data) {
	var self = this;
	self.PopupTitle = ko.observable("Select Production Precedence");
	self.PickerName = ko.observable("Productions");

	self.data = data;

	this.okCallback = okCallback;

	this.construct = function(view) {
		self.view = view;
	};
	this.open = function(selectedProductions) {
		self.loadAvailableProductions(selectedProductions);
		self.view.dialog("open");
	};
	this.loadAvailableProductions = function(selectedProductions) {
		IP.data.ajax({
				type: "get",
				url: IP.utils.generateWebAPIURL("Production/GetProductionsForExport"),
				data: {
					sourceWorkspaceArtifactId: IP.utils.getParameterByName("AppID", window.top)
				}
			})
			.then(function(result) {
				self.loadProductions(result, selectedProductions);
			})
			.fail(function(error) {
				IP.message.error.raise("No attributes were returned from the source provider.");
			});
	};
	this.loadProductions = function(productions, selectedProductions) {
		self.model.availableValues(productions);
		self.model.selectValues(selectedProductions,
			function(item1, item2) {
				return item1.artifactID == item2.artifactID;
			});
	};
	this.ok = function() {
		self.okCallback(self.model.mappedValues());
		self.view.dialog("close");
	};
	this.cancel = function() {
		self.view.dialog("close");
	};
	self.model = new ListPickerViewModel();
	self.pickerId = ko.observable("productionPrecedencePicker");
}
