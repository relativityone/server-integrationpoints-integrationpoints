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

		this.ipModel = {};

		this.goToStep = function (step) {
			var totalSteps = this.steps().length - 1;
			if (step > totalSteps) {
				step = totalSteps;
			}
			if (step < 0) {
				step = 0;
			}
			var nextStep = this.steps()[step];
			if (nextStep.loadModel) {
				nextStep.loadModel($.extend({}, this.ipModel));
			}
			this.currentStep(nextStep);
			return step;
		};

		var nextStep = this.steps()[0];
		if (nextStep.loadModel) {
			nextStep.loadModel($.extend({}, this.ipModel));
		}
		this.currentStep(nextStep);
	};

	$(function () {
		var vm = new viewModel();
		ko.applyBindings(vm, document.getElementById('pointBody'));
		var step = 0;

		IP.messaging.subscribe('next', function () {
			vm.currentStep().submit().then(function () {
				step = vm.goToStep(++step);
				IP.messaging.publish('goToStep', step);
			}).fail(function (err) {
				alert(err);
				throw err;
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
				step = vm.goToStep(--step);
				IP.messaging.publish('goToStep', step);
			});
		});

	});
})(ko);
