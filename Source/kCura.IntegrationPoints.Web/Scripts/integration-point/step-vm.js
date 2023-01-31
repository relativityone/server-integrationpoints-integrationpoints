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
				model.rdoTypeName = $('#destinationRdo option:selected').text();
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
		var saveRequested = false;
		var heapData = {};
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
			if (validStepNo === 0)
			{
				heapData["NotificationEmailsAdded"] = $('#notificationEmails').val() !== "";
			}
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
				if (validStepNo === 2)
				{
					if (IsSyncJob())
					{
						heapData["CreateSavedSearch"] = $('#create-saved-search-0:checked').val() === "true";
					}
				}
				if (saveRequested) 
				{
					SendHeapMetrics();
					saveRequested = false;
				}
			});
			return d.promise;
		};

		var _save = function () {
			saveRequested = true;
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
				})
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

		var SendHeapMetrics = function()
		{
			try
			{
				let heap = window.heap;
				if (heap)
				{
					let source = GetSourceType();
					heapData["SourceProvider"] = source;
					heapData["DestinationProvider"] = model.IPDestinationSettings.Provider;
					heapData["ImportExport"] = model.isExportType ? "Export" : "Import";
					heapData["LogErrors"] = model.logErrors;
					heapData["RdoTypeName"] = model.rdoTypeName;
					heapData["SelectedProfile"] = model.profile.selectedProfile !== undefined;
					heapData["EnableTagging"] = model.EnableTagging;
					heapData["SchedulerIsEnabled"] = model.scheduler.enableScheduler;
					if (model.scheduler.enableScheduler)
					{
						heapData["Scheduler-Frequency"] = model.scheduler.selectedFrequency;
						heapData["Scheduler-Time"] = model.scheduler.scheduledTime;
						
						let startDate = Date.parse(model.scheduler.startDate);
						let endDate = Date.parse(model.scheduler.endDate);
						let scheduleTimePeriod = (endDate - startDate)/1000/3600/24 // days calculation
						heapData["Scheduler-TimePeriod_days"] = scheduleTimePeriod;
					}

					if (IsSyncJob())
					{
						heapData["SourceFieldsCount"] = $('#source-fields option').length;
						heapData["MappedSourceFieldsCount"] = $('#selected-source-fields option').length;
						heapData["DestinationFieldsCount"] = $('#workspace-fields option').length;
						heapData["MappedDestinationFieldsCount"] = $('#selected-workspace-fields option').length;
						let sourceConfiguration = JSON.parse(model.sourceConfiguration);
						heapData["SyncSourceType"] = sourceConfiguration.ProductionImport ? "Production" : "SavedSearch";
						heapData["OverwriteMode"] = model.SelectedOverwrite;
						if (model.SelectedOverwrite !== "Append Only")
						{
							heapData["FieldOverlayBehavior"] = model.FieldOverlayBehavior;
						}
						heapData["ImageImport"] = $('#exportImages-radio-0:checked').val() === "true"
						if (heapData["ImageImport"])
						{
							heapData["ImagePrecedence"] = $('#image-production-precedence option:selected').text();
							heapData["CopyFilesToRepository"] = $('#native-file-radio-0:checked').val() === "true";
							if (heapData["ImagePrecedence"] === "Produced Images")
							{
								heapData["IncludeOriginalImagesIfNotProduced"] = $('#image-include-original-images-checkbox:checked').val() !== undefined;
							}
						}
						else
						{
							let physicalFilesChecked = $('#native-file-mode-radio-0:checked').val() !== undefined;
							let copyLinksChecked = $('#native-file-mode-radio-1:checked').val() !== undefined;
							let importNativeFileCopyMode = physicalFilesChecked ? "Physical files" : copyLinksChecked ? "Links Only" : "No";
							heapData["ImportNativeFileCopyMode"] = importNativeFileCopyMode.trim();
							heapData["UseFolderPathInformation"] = $('#folderPathInformationSelect option:selected').text().trim();
							if (heapData["UseFolderPathInformation"] !== "No" && model.SelectedOverwrite !== "Append Only")
							{
								heapData["MoveExistingDocuments"] = $('#move-documents-radio-0:checked').val() === "true";
							}
							if (heapData["UseFolderPathInformation"] === "Read From Field" )
							{
								heapData["FolderPathInformation"] = $('#folderPath option:selected').text();
							}
						}
					}
					heap.track("IntegraionPoint-Edit", heapData);
				}
			} catch (error)
			{
				console.warn(error);
			}
		}

		var IsSyncJob = function()
		{
			let relativitySourceTypeGuid = "423b4d43-eae9-4e14-b767-17d629de4bb2";
			return relativitySourceTypeGuid === model.source.selectedType;
		}

		var GetSourceType = function()
		{
			let source = $.grep(model.source.sourceTypes, function(item)
					{				
						return item.value === model.source.selectedType;			
					})[0]
					.displayName;
			return source;
		}
	});
})(ko, IP.timeUtil);