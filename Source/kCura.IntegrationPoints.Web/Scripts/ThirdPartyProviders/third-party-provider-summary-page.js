var IP = IP || {};

var loadData = function(ko, dataContainer) {
	var Model = function (dataContainer) {
		this.hasErrors = dataContainer.hasErrors;
		this.logErrors = dataContainer.logErrors;
		this.emailNotification = dataContainer.emailNotification;
		this.name = dataContainer.name;
		this.overwriteMode = dataContainer.overwriteMode;
		this.sourceProviderName = dataContainer.sourceProviderName;
		this.destinationRdoName = dataContainer.destinationRdoName;

		var createFields = function($container, sourceConfigurationFields) {
			var $tr = $container.parent('tr');
			$.each(sourceConfigurationFields || [],
				function() {
					var $newTr = $tr.clone();
					var value = this.value || this.Value;
					var key = this.key || this.Key;
					var v = IP.utils.stringNullOrEmpty(value) ? '' : value;
					IP.utils.updateField($newTr, key, v);
					$newTr.find('input').attr('id', IP.utils.toCamelCase(key)).removeAttr('faartifactid').removeAttr('fafriendlyname');
					$tr.after($newTr);
					$tr = $newTr;
				});
			$container.parent('tr').hide();
		};

		var getAppPath = function() {
			var newPath = window.location.pathname.split('/')[1];
			var url = window.location.protocol + '//' + window.location.host + '/' + newPath;
			return url;
		};

		var getSourceConfigurationFields = function(str) {
			var d = IP.data.deferred().defer();
			var appID = IP.utils.getParameterByName('AppID', window.top);
			var artifactID = IP.artifactid;
			var obj = {
				applicationPath: getAppPath(),
				appID: appID,
				artifactID: artifactID
			};
			var url = IP.utils.format(IP.params['sourceUrl'], obj);
			if (url) {
				IP.data.ajax({
					url: url,
					data: JSON.stringify(str),
					type: 'post'
				}).then(function(result) {
						d.resolve(result);
					},
					function(r) {
						d.reject(r);
					});
			} else {
				d.reject();
			}

			return d.promise;
		};

		this.populateSourceConfigurationContainer = function() {
			var $field = $("#sourceConfigurationContainer").siblings('.dynamicViewFieldValue');

			getSourceConfigurationFields(dataContainer.sourceConfiguration).then(function(result) {
					createFields($field, result);
				},
				function() {
					$field.text('There was an error retrieving the source configuration.');
				});
		}
	};

	var viewModel = new Model(dataContainer);
	viewModel.populateSourceConfigurationContainer();
	ko.applyBindings(viewModel, document.getElementById('summaryPage'));
};