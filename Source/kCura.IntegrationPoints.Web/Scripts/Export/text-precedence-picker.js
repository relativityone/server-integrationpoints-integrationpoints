var TextPrecedencePickerViewModel = function(okCallback, artifactTypeId) {
	var self = this;
	self.PopupTitle = ko.observable("Select Text Precedence");
	self.PickerName = ko.observable("Long Text Fields");

	self.artifactTypeId = artifactTypeId;

	this.okCallback = okCallback;

	this.construct = function(view) {
		self.view = view;
	};
	this.open = function(selectedFields) {
		self.loadAvailableFields(selectedFields);
		self.view.dialog("open");
	};
	this.loadAvailableFields = function(selectedFields) {
		IP.data.ajax({
				type: "get",
				url: IP.utils.generateWebAPIURL("ExportFields/LongTextFields"),
				data: {
					sourceWorkspaceArtifactId: IP.utils.getParameterByName("AppID", window.top),
					artifactTypeId : self.artifactTypeId
				}
			})
			.then(function(result) {
				self.loadFields(result, selectedFields);
			})
			.fail(function(error) {
				IP.message.error.raise("No attributes were returned from the source provider.");
			});
	};
	this.loadFields = function(fields, selectedFields) {
		self.model.availableValues(fields);
		self.model.selectValues(selectedFields,
			function(item1, item2) {
				return item1.fieldIdentifier == item2.fieldIdentifier;
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
	self.pickerId = ko.observable("textPrecedencePicker");
}
