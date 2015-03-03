(function (ko, helper) {
	IP.points = IP.points || {};
	IP.points.steps = {
		steps: [],
		push: function (step, order) {
			this.steps.push(step);
		}
	};

	var viewModel = function () {
		this.steps = ko.observableArray(IP.points.steps.steps);
		this.currentStep = ko.observable();

		this.currentStep.subscribe(function () {
			this.currentStep().getTemplate();
		}, this);


		this.goToStep = function (step, model) {
			var totalSteps = this.steps().length - 1;
			if (step > totalSteps) {
				return totalSteps;
			}
			if (step < 0) {
				return 0;
			}
			var nextStep = this.steps()[step];
			if (nextStep.loadModel) {
				nextStep.loadModel($.extend({}, model));
			}
			this.currentStep(nextStep);
			return step;
		};

	};

	$(function () {

		var vm = new viewModel();
		var step = 0;
		var model = {};
		var artifactID = 0;
		IP.data.ajax({
			url: IP.utils.generateWebAPIURL('IntegrationPointsAPI', IP.data.params['artifactID']),
			type: 'Get'
		}).then(function (result) {
			vm = new viewModel();
			if (result.scheduler && result.scheduler.scheduledTime) {
				
				var time = helper.utcToLocal(result.scheduler.scheduledTime.split(':'), "HH:mm");
				var timeSplit = time.split(':');
				
				if ((timeSplit[0] - 12) >= 1) {
					result.scheduler.scheduledTime = timeSplit[0] - 11 + ":" + timeSplit[1];
					result.scheduler.selectedTimeFormat='PM';
				} else {
					result.scheduler.scheduledTime = timeSplit[0] + ":" + timeSplit[1];
					result.scheduler.selectedTimeFormat = 'AM';
				}
				
			}
			vm.goToStep(0, result);
			artifactID = result.artifactID;
			ko.applyBindings(vm, document.getElementById('pointBody'));
		});

		var _next = function () {
			var d = IP.data.deferred().defer();
			vm.currentStep().submit().then(function (result) {
				result.artifactID = artifactID;
				step = vm.goToStep(++step, result);
				model = result;
				IP.message.error.clear();
				IP.messaging.publish('goToStep', step);
				d.resolve(result);
			}).fail(function (err) {
				if (err.message) {
					err = err.message;
				}
				IP.message.error.raise(err);
				d.reject(err);
			});
			return d.promise;
		};

		IP.messaging.subscribe('next', function () {
			_next();
		});

		IP.messaging.subscribe('save', function () {
			_next().then(function (result) {
				if (result.scheduler && result.scheduler.scheduledTime) {
					var timeSplit = result.scheduler.scheduledTime.split(':');
					var time = result.scheduler.scheduledTime; 
					if (result.scheduler.selectedTimeFormat == "AM") {
						result.scheduler.scheduledTime = timeSplit[0] == 12 ? helper.timeLocalToUtc(0+':'+timeSplit[1]) : helper.timeLocalToUtc(time);
					} else {
						
						var hour = parseInt(timeSplit[0]) + 12;
						result.scheduler.scheduledTime = helper.timeLocalToUtc(hour+':'+timeSplit[1]);
					}
					
				}
				IP.messaging.publish('saveComplete', result);
			}, function (error) {
				IP.message.error.raise(error);
			});

		});

		IP.messaging.subscribe('saveComplete', function (model) {
			IP.data.ajax({ type: 'POST', url: IP.utils.generateWebAPIURL('IntegrationPointsAPI'), data: JSON.stringify(model) }).then(function (result) {
				//redirect to page!!
				var prefix = window.top.location.protocol + "//" + window.top.location.host;
				window.top.location = prefix + result.returnURL;
			}, function (error) {
				IP.message.error.raise(error);
			});
		});
		
		IP.messaging.subscribe('back', function () {
			if (typeof (vm.currentStep().back) !== "function") {
				vm.currentStep().back = function () {
					var d = IP.data.deferred().defer();
					d.resolve();
					return d.promise;
				};
			}

			vm.currentStep().back().then(function () {
				step = vm.goToStep(--step, model);
				IP.message.error.clear();
				IP.messaging.publish('goToStep', step);
			});
		});


	});
})(ko, IP.timeUtil);
