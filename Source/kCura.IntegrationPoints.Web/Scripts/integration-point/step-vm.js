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
				} else if (model.source.selectedType === "548f0873-8e5e-4da6-9f27-5f9cda764636") {
					IP.stepDefinitionProvider.loadDefaults();
					IP.frameMessaging().subscribe('importType', function (data) {
						if (data === 0) {
							IP.stepDefinitionProvider.loadDefaults(2);
							$.stepProgress.showButtons(true, true, false);
						}
						else {
							IP.stepDefinitionProvider.loadOverride([
								{
									text: 'Setup'
								},
								{
									text: 'Source Information'
								},
							],
								2);
							$.stepProgress.showButtons(true, false, true);
						}
					});
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
		var validStepNo = 0;
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
			let $select2elements =  $('.select2-offscreen').filter(function () {
					return !this.id.match('s2id_autogen') && this.tagName === 'SELECT';
				});
			vm.currentStep().submit().then(function (result) {
				result.artifactID = artifactID;
				step = vm.goToStep(++step, result);
				validStepNo = ++validStepNo;
				model = result;
				IP.message.error.clear();
				IP.messaging.publish('goToStep', step);
				d.resolve(result);
			}).fail(function (err) {
				if (err === undefined) {
					return;
				}

				if (err.message) {
					err = err.message;
				}
				IP.message.error.raise(err);
				d.reject(err);
			}).done(function () {
				let heap = window.heap;
				if (heap) {
					let heapEventParameters = {};
                    heapEventParameters['stepIndex'] = validStepNo;
                    $select2elements.each(function (i, element) {
                        try {
                            let optiontext = element.options[element.selectedIndex].text;
                            heapEventParameters["select-" + element.id] = optiontext;
                        } catch (error) {
                            // empty intentionally
                        }
                    });
                    heap.track('NextStepEvent', heapEventParameters);
                }
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
				result.scheduler.scheduledTime = IP.timeUtil.formatMilitaryTimeTo24HourTime(result.scheduler.scheduledTime, result.scheduler.selectedTimeMeridiem);
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

			var mappingHasWarnings = false;			 

			if (model.mappingHasWarnings){
				mappingHasWarnings = true;
			}

			var destinationWorkspaceChanged = false;
			if (model.destinationWorkspaceChanged) {
				destinationWorkspaceChanged = true;
			}

			var clearAndProceedSelected = false;
			if (model.clearAndProceedSelected) {
				clearAndProceedSelected = true;
			}

			var apiUrl = IP.utils.generateWebAPIURL(IP.data.params['apiControllerName'])
				+ '?mappingHasWarnings=' + mappingHasWarnings
				+ '&destinationWorkspaceChanged=' + destinationWorkspaceChanged
				+ '&clearAndProceed=' + clearAndProceedSelected
				+ '&mappingType=' + IP.mappingType;

			IP.data.ajax({ type: 'POST', url: apiUrl, data: JSON.stringify(model) })
				.then(function (result) {
					//redirect to page!!
					IP.unsavedChangesHandler.unregister();
					$('#save').attr('save', 'true');
					IP.modal.open(200, $('body'));
					var prefix = window.top.location.protocol + "//" + window.top.location.host;
					window.top.location = prefix + result.returnURL;
				}, function (error) {
					try {
						const errPrefix = "Failed to save Integration Point.";
						const validationResultDto = JSON.parse(error.responseText);
						IP.message.errorFormatted.raise(validationResultDto.errors, null, errPrefix);
					} catch (e) {
						IP.message.error.raise(error);
					}
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
				validStepNo = --validStepNo;
				IP.message.error.clear();
				IP.messaging.publish('goToStep', step);
			});
		});
	});
})(ko, IP.timeUtil);