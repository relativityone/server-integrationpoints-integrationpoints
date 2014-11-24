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

		this.goToStep = function (step) {
			var totalSteps = this.steps().length - 1;
			if (step > totalSteps) {
				step = totalSteps;
			}
			if (step < 0) {
				step = 0;
			}
			this.currentStep(this.steps()[step]);
			return step;
		};

		this.currentStep(this.steps()[0]);
	};
	
	$(function () {
		var vm = new viewModel();
		ko.applyBindings(vm, document.getElementById('pointBody'));
		var step = 0;

		IP.messaging.subscribe('next', function () {
			vm.currentStep().submit().then(function () {
				step = vm.goToStep(++step);
				IP.messaging.publish('goToStep', step);
			}).fail(function(err){
				throw err;
			});
		});

		IP.messaging.subscribe('back', function () {
			step = vm.goToStep(--step);
			IP.messaging.publish('goToStep', step);
		});

	});
})(ko);
