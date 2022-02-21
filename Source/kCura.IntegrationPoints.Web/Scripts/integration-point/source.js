var Source = function (s, parentModel) {
	this.settings = $.extend({}, s);
	this.templateID = 'ldapSourceConfig';
	var self = this;
	self.disable = parentModel.hasBeenRun();

	var loadFileProviderGuid = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18";

	this.sourceTypes = ko.observableArray();
	this.selectedType = ko.observable().extend({ required: true });
	this.isSourceProviderDisabled = ko.observable(false);

	this.SourceProviderConfiguration = ko.observable();

	this.tmpRelativitySourceTypeObject = null;

	this.sourceProvider = self.settings.sourceProvider || 0;

	this.loadSettings = function (settings) {
		self.settings = settings;
		self.sourceProvider = settings.sourceProvider;
		self.updateSelectedType();
	};


	this.displayRelativityInSourceTypes = function (value) {
		if (self.tmpRelativitySourceTypeObject === null) return;

		if (value === true) {
			var containsRelativityObj = false;
			$.each(self.sourceTypes(),
				function () {
					if (this.displayName === "Relativity") {
						containsRelativityObj = true;
					}
				});
			if (containsRelativityObj === false) {
				self.sourceTypes.push(self.tmpRelativitySourceTypeObject);
			}
		} else {
			$.each(self.sourceTypes(),
				function () {
					if (this === self.tmpRelativitySourceTypeObject) {
						self.sourceTypes.remove(this);
					}
				});
		}
	};

	this.updateSelectedType = function () {

		$.each(self.sourceTypes(), function () {
			if (!!self.settings && this.value === self.settings.selectedType || this.artifactID === self.sourceProvider) {
				self.selectedType(this.value);
			}

			if (this.displayName == "Relativity") {
				self.tmpRelativitySourceTypeObject = this;
			}
		});
	};

	this.selectedType.subscribe(function (selectedValue) {

		var isLoadFileDestinationProvider = parentModel.destination.selectedDestinationTypeGuid() === loadFileProviderGuid;
		var enableSyncNonDocumentFlow = IP.data.params['EnableSyncNonDocumentFlowToggleValue'];

		$.each(self.sourceTypes(), function () {
			if (this.value === selectedValue) {
				self.sourceProvider = this.artifactID;
				if (typeof this.model.config.compatibleRdoTypes === 'undefined' ||
					this.model.config.compatibleRdoTypes === null ||
					isLoadFileDestinationProvider ||
					enableSyncNonDocumentFlow === true)
					{
						var rdoTypesToDisplay = parentModel.destination.allRdoTypes();

						if (enableSyncNonDocumentFlow && parentModel.isSyncFlow()) {
							rdoTypesToDisplay = rdoTypesToDisplay.filter(x => x.belongsToApplication === true);
						}

						parentModel.destination.rdoTypes(rdoTypesToDisplay);
				}
				else {
					var compatibleRdos = this.model.config.compatibleRdoTypes;
					var rdosToDisplay = [];
					$.each(parentModel.destination.allRdoTypes(), function () {
						if (compatibleRdos.indexOf(this.value) > -1) {
							rdosToDisplay.push(this);
						}
					});
					parentModel.destination.rdoTypes(rdosToDisplay);
				}
				self.SourceProviderConfiguration = this.model.config;
				parentModel.destination.UpdateSelectedItem();
			}
		});
		self.selectedType.isModified(false);
		IP.messaging.publish("SourceProviderTypeChanged", !selectedValue ? undefined : self.sourceProvider);
	});
};
