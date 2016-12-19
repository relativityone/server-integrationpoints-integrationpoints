﻿(function (ko, helper) {
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
			var totalSteps = 2; // hardcoded to hide additional steps //this.steps().length - 1;
			if (step > totalSteps) {
				return totalSteps;
			}
			if (step < 0) {
				return 0;
			}

			if (step === 0) {
				IP.stepDefinitionProvider.init();
			} else if (step === 1) {
				if (model.destinationProviderGuid === "1D3AD995-32C5-48FE-BAA5-5D97089C8F18") {
					IP.stepDefinitionProvider.loadOverride([
							{
								text: 'Setup'
							},
							{
								text: 'Source Information'
							},
							{
								text: 'Destination Information'
							}
					]);
				} else {
					IP.stepDefinitionProvider.loadDefaults();
				}
			}

			var nextStep;

			if (model.destinationProviderGuid === "1D3AD995-32C5-48FE-BAA5-5D97089C8F18") {
				_steps = ko.utils.arrayFilter(this.steps(), function (_step) {
					return _step.settings.isForRelativityExport;
				});

				nextStep = _steps[step - 1];
			}

			if (!nextStep) {
				nextStep = this.steps()[step];
			}

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
			url: IP.utils.generateWebAPIURL(IP.data.params['apiControllerName'], IP.data.params['artifactID']),
			type: 'Get'
		}).then(function (result) {
			vm = new viewModel();
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

		var _save = function () {
			if (step === 0) {
				var d = IP.data.deferred().defer();

				vm.currentStep().submit().then(function (result) {
					result.artifactID = artifactID;
					model = result;
					IP.message.error.clear();
					d.resolve(result);
				}).fail(function (err) {
					if (err.message) {
						err = err.message;
					}
					IP.message.error.raise(err);
					d.reject(err);
				});
				return d.promise;
			} else {
				return _next();
			}
		};

		IP.messaging.subscribe('next', function () {
			_next();
		});

		var proceedToSaveComplete = function (result) {
			if (result.scheduler && result.scheduler.scheduledTime) {
				var timeSplit = result.scheduler.scheduledTime.split(':');
				var time = result.scheduler.scheduledTime;
				if (result.scheduler.selectedTimeFormat == "AM") {
					result.scheduler.scheduledTime = timeSplit[0] == 12 ? (0 + ':' + timeSplit[1]) : time;
				} else {
					var hour = 12;
					if (parseInt(timeSplit[0], 10) < 12) {
						hour = parseInt(timeSplit[0], 10) + 12;
					}

					result.scheduler.scheduledTime = hour + ':' + timeSplit[1];
				}
			}
			IP.messaging.publish('saveComplete', result);
		}

		IP.messaging.subscribe('save', function () {
			_save().then(function (result) {

				if (typeof (vm.currentStep().validate) !== "function") {
					vm.currentStep().validate = function () {
						var d = IP.data.deferred().defer();
						d.resolve(true);
						return d.promise;
					};
				}

				vm.currentStep().validate(result).then(function (validationResult) {
					if (validationResult === true) {
						proceedToSaveComplete(result);
					}
				}).fail(function (err) {
					IP.message.error.raise(err);
				});
			}, function (error) {
				IP.message.error.raise(error);
			});
		});

		IP.messaging.subscribe('saveComplete', function (model) {
			var save = $('#save').attr('save');
			if (typeof (save) != 'undefined') {
				return;
			}

			//If we are the import provider we need to pass along Extracted Text/ Native path fields
			//This will allow the provider to convert any relative paths to absolute.
			if (model.source.selectedType === "548f0873-8e5e-4da6-9f27-5f9cda764636") {
				var importConfig = JSON.parse(model.sourceConfiguration);
				var destinationConfig = JSON.parse(model.destination);
				var fieldMap = JSON.parse(model.map);
				if (destinationConfig.ExtractedTextFieldContainsFilePath === 'true') {
					//get the field identifier for the source field that contians the extracted text path
					for (var i = 0; i < fieldMap.length; i++) {
						if (fieldMap[i].destinationField.displayName === destinationConfig.LongTextColumnThatContainsPathToFullText) {
							$.extend(importConfig, { ExtractedTextPathFieldIdentifier: fieldMap[i].sourceField.fieldIdentifier });
							break;
						}
					}
				}
				if (destinationConfig.importNativeFile === 'true') {
					//get the field identifier for the source field that contians the native file path
					for (var j = 0; j < fieldMap.length; j++) {
						if (fieldMap[j].fieldMapType === 'NativeFilePath') {
							$.extend(importConfig, { NativeFilePathFieldIdentifier: fieldMap[j].sourceField.fieldIdentifier });
							break;
						}
					}
				}

				model.sourceConfiguration = JSON.stringify(importConfig);
			}

			IP.data.ajax({ type: 'POST', url: IP.utils.generateWebAPIURL(IP.data.params['apiControllerName']), data: JSON.stringify(model) }).then(function (result) {
				//redirect to page!!
				IP.unsavedChangesHandler.unregister();
				$('#save').attr('save', 'true');
				IP.modal.open(200, $('body'));
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

			vm.currentStep().back().then(function (result) {
				$.extend(model, result);
				step = vm.goToStep(--step, model);
				IP.message.error.clear();
				IP.messaging.publish('goToStep', step);
			});
		});
	});
})(ko, IP.timeUtil);