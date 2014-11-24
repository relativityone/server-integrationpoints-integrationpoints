var IP = IP || {};
(function (root, ko) {

	var Choice = function (name, value) {
		this.displayName = name;
		this.value = value;
	};

	var Source = function () {
		this.templateID = 'ldapSourceConfig';
	};

	var Destination = function () {
		var self = this;

		IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('Test') }).then(function (result) {
			var types = $.map(result, function(entry){
				return new Choice(entry.m_Item1, entry.m_Item2);
			});
			self.rdoTypes(types);
		}, function () {
			
		})

		this.templateID = 'ldapDestinationConfig';
		this.rdoTypes = ko.observableArray();
		this.selectedRdo = ko.observable();
	};

	var Scheduler = function () {
		this.templateID = 'schedulingConfig';
	};
	
	var Model = function () {
		
		var self = this;
		this.name = ko.observable('name');

		this.source = new Source();
		this.destination = new Destination();
		this.overwrite = ko.observableArray([
			new Choice('test1', 1234),
			new Choice('test2', 5678)
		]);
		this.selectedOverwrite = ko.observable();
		this.scheduler = new Scheduler();
	};

	var Step = function (settings) {
		var self = this;
		self.settings = settings;
		this.template = ko.observable();
		this.hasTemplate = false;
		this.model = new Model();

		this.getTemplate = function () {

			IP.data.ajax({ dataType: 'html', cache: true, type: 'get', url: self.settings.url }).then(function (result) {
				$('body').append(result);
				self.template(self.settings.templateID);
				self.hasTemplate = true;
			});
		};

		this.submit = function () {
			var d = root.data.deferred().defer();
			var url = IP.utils.generateWebURL('IntegrationPoints', 'ValidationConnection');
			root.data.ajax({ type: 'get', url: url }).then(function (result) {
				if (result.success) {
					d.resolve();
				} else {
					d.reject();
				}
			});
			return d.promise;
		};
	};

	var step = new Step({
		url: IP.utils.generateWebURL('IntegrationPoints', 'StepDetails'),
		templateID: 'step1'
	});
	root.points.steps.push(step);

})(IP, ko);
