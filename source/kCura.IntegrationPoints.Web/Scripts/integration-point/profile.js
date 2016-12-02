var Profile = function (p, parentModel) {
	try {
		p = JSON.parse(p);
	} catch (e) {
		p = p;
	}
	this.settings = $.extend({}, p);
	var self = this;
	self.disable = parentModel.hasBeenRun();

	this.templateID = 'profileConfig';

	this.selectedProfile = ko.observable();

	this.profileTypes = ko.observableArray();
	this.profiles = [];

	this.getSelectedProfilePromise = function (artifactId) {
		var validatedProfileModelPromise = IP.data.ajax({
			url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/GetValidatedProfileModel', artifactId),
			type: 'get'
		}).fail(function (error) {
			IP.message.error.raise("No exportable fields were returned from the source provider.");
		});
		return validatedProfileModelPromise;
	};
	self.currentFilter = ko.observable();

	self.subscription = IP.messaging.subscribe('ProviderTypeChanged', function (type) {
		if (!!parentModel.source.sourceProvider && !!parentModel.destination.selectedDestinationType) {
			self.currentFilter({ source: parentModel.source.sourceProvider, destination: parentModel.destination.selectedDestinationType() });
		}
	});

	this.getProfiles = function (ipType) {
		var profilePromise = IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('IntegrationPointProfilesAPI/GetByType', ipType) });
		profilePromise.then(function (result) {
			var profileTypes = $.map(result, function (entry) {
				return new Choice(entry.name, entry.artifactID, "", { source: entry.sourceProvider, destination: entry.destinationProvider });
			});
			self.profileTypes(profileTypes);
			self.profiles = result;
		});
	};

	self.filterProfiles = ko.computed(function () {
		if (!self.currentFilter() || !self.currentFilter().source || !self.currentFilter().destination) {
			return self.profileTypes();
		} else {
			return ko.utils.arrayFilter(self.profileTypes(), function (profile) {
				return profile.model.source === self.currentFilter().source && profile.model.destination === self.currentFilter().destination;
			});
		}
	});

	this.publishUpdateProfile = function () {
		var profileId = self.selectedProfile();
		if (!!profileId) {
			var promise = self.getSelectedProfilePromise(profileId);
			promise.then(function (profile) {
				IP.messaging.publish("loadProfile", profile);
			});
		}
	};

};