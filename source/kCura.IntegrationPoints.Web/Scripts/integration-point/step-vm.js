(function (ko) {
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
				step = totalSteps;
			}
			if (step < 0) {
				step = 0;
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
			vm.goToStep(0, result);
			artifactID = result.artifactID;
			ko.applyBindings(vm, document.getElementById('pointBody'));
		});

		IP.messaging.subscribe('next', function () {
			vm.currentStep().submit().then(function (result) {
				debugger;
				result.artifactID = artifactID;
				step = vm.goToStep(++step, result);
				model = result;
				IP.messaging.publish('goToStep', step);
			}).fail(function (err) {
				IP.message.error.raise(err);
			});
		});
				
		IP.messaging.subscribe('back', function () {
			if (typeof (vm.currentStep().back) !== "function") {
				vm.currentStep().back = function () {
					var d = root.data.deferred().defer();
					d.resolve();
					return d.promise();
				};

			}
			vm.currentStep().back().then(function () {
				step = vm.goToStep(--step, model);
				IP.messaging.publish('goToStep', step);
			});
		});

	});
})(ko);
