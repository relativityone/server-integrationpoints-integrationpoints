var Destination = function (d, parentModel) {
	try {
		d = JSON.parse(d);
	} catch (e) {
	}
	this.settings = $.extend({}, d);
	var self = this;
	self.disable = parentModel.hasBeenRun();
	var relativityDestinationProviderGuid = "74A863B9-00EC-4BB7-9B3E-1E22323010C6";
	var loadFileProviderGuid = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18";

	this.loadSettings = function (settings) {
		self.settings = settings;
		self.updateDestinationProvider();
		self.UpdateSelectedItem();//TODO: refactor RDO update dependency on source
		self.profile = settings;
	};

	this.templateID = 'ldapDestinationConfig';
	this.allRdoTypes = ko.observableArray();
	this.rdoTypes = ko.observableArray();

	this.destinationTypes = ko.observableArray();
	this.selectedDestinationType = ko.observable().extend({ required: true });

	this.selectedDestinationType.subscribe(function (selectedValue) {

		var destType = self.selectedDestinationTypeGuid();
		var enableSyncNonDocumentFlow = IP.data.params['EnableSyncNonDocumentFlowToggleValue'];

		var disableRdoSelection = destType === relativityDestinationProviderGuid && enableSyncNonDocumentFlow === false;
		self.isDestinationObjectDisabled(disableRdoSelection);
		
		if (destType === loadFileProviderGuid ||
			(typeof parentModel.source.SourceProviderConfiguration.compatibleRdoTypes === 'undefined' ||
			parentModel.source.SourceProviderConfiguration.compatibleRdoTypes === null ||
			enableSyncNonDocumentFlow === true)) {

				var rdoTypesToDisplay = self.allRdoTypes();

				if (enableSyncNonDocumentFlow && parentModel.isSyncFlow()) {
					rdoTypesToDisplay = rdoTypesToDisplay.filter(x => x.belongsToApplication === true);
				}

				self.rdoTypes(rdoTypesToDisplay);

				if (typeof self.artifactTypeID() === 'undefined') {
					self.artifactTypeID(parentModel.DefaultRdoTypeId);
				}
		}
		else {
			if ($.isArray(parentModel.source.SourceProviderConfiguration.compatibleRdoTypes)) {
				var rdosToDisplay = [];
				$.each(self.allRdoTypes(), function () {
					if (parentModel.source.SourceProviderConfiguration.compatibleRdoTypes.indexOf(this.value) > -1) {
						rdosToDisplay.push(this);
					}
				});
				self.rdoTypes(rdosToDisplay);
			}
		}
		IP.messaging.publish("DestinationProviderTypeChanged", self.selectedDestinationType());
	});

	this.destinationProviderVisible = ko.observable(false);
	this.isDestinationProviderDisabled = ko.observable(false);

	var withArtifactId = function (artifacId) {
		return function (element) {
			return element.artifactID === artifacId;
		}
	}
	this.selectedDestinationTypeGuid = function () {
		var results = self.destinationTypes().filter(withArtifactId(self.selectedDestinationType()));
		return results.length > 0 ? results[0].value : "";
	}

	this.setRelativityAsDestinationProvider = function () {
		var defaultRelativityProvider = self.destinationTypes().filter(function (obj) {
			return obj.value === relativityDestinationProviderGuid;
		});
		if (defaultRelativityProvider.length === 1) {
			self.selectedDestinationType(defaultRelativityProvider[0].artifactID);
		}
	}

	this.updateDestinationProvider = function () {
		$.each(self.destinationTypes(), function () {
			if (!!self.settings && !!self.settings.destinationProviderType && this.value === self.settings.destinationProviderType && self.settings.destinationProviderType !== undefined) {
				self.selectedDestinationType(this.artifactID);
			}
		})
	};
	this.artifactTypeID = ko.observable().extend({ required: true });
	this.artifactTypeID.subscribe(function(value) {
		IP.messaging.publish("TransferedObjectChanged", value);
		IP.data.params['TransferredRDOArtifactTypeID'] = value;
	});
	this.UpdateSelectedItem = function () {

		if (self.settings.artifactTypeID === undefined) {
			self.artifactTypeID(parentModel.DefaultRdoTypeId);
		} else {
			self.artifactTypeID(self.settings.artifactTypeID);
		}
	}

	this.isDestinationObjectDisabled = ko.observable(false);
};