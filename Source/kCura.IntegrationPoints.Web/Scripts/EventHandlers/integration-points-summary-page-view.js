IP.isFileshareProvider = true;

$(function integrationPointsSummaryPageView() {

	var displaySummaryPage = function (fieldId, html, dataContainer) {
		console.log("ip summary page view - in integrationPointsSummaryPageView -> displaySummaryPage");
		IP.utils.getViewField(fieldId).closest('.innerTabTable').replaceWith(html);
		loadData(ko, dataContainer);
	}

	var SummaryPageGeneralTab = function () {
		var self = this;

		var DataContainer = function () {
			var self = this;

			this.hideContainer = function () {
				IP.utils.getViewField(IP.sourceConfiguration).parent().hide();
				IP.utils.getViewField(IP.overwriteFieldsId).parent().hide();
				IP.utils.getViewField(IP.sourceProviderId).parent().hide();
				IP.utils.getViewField(IP.destinationid).parent().hide();
				IP.utils.getViewField(IP.destinationProviderid).parent().hide();
			};

			var apiControllers = { "LDAP": "ldap/view", "FTP (CSV File)": "FtpProviderAPI/view" }

			var setSourceConfigurationField = function (sourceConfigurationString) {
				self.sourceConfiguration = JSON.parse(sourceConfigurationString);
			}

			var decryptConfiguration = function (configuration) {
				IP.data.ajax({
					data: JSON.stringify(configuration),
					url: IP.utils.generateWebAPIURL(apiControllers[self.sourceProviderName]),
					type: 'post',
					async: false
				}).then(function (result) {
					setSourceConfigurationField(result);
				});
			}

			var updateSourceConfigurationField = function () {
				var sourceConfigurationFieldText = IP.utils.getViewField(IP.sourceConfiguration).siblings('.dynamicViewFieldValue')
					.text();

				switch (self.sourceProviderName) {
					case "LDAP":
					case "FTP (CSV File)":
						decryptConfiguration(sourceConfigurationFieldText);
						break;
					default:
						setSourceConfigurationField(sourceConfigurationFieldText);
				}
			}
			this.destinationConfiguration = JSON.parse(IP.utils.getViewField(IP.destinationid).siblings('.dynamicViewFieldValue').text());
			this.transferredRdoTypeName = this.destinationConfiguration.ArtifactTypeName;
			this.hasErrors = IP.utils.getViewField(IP.hasErrorsId).siblings('.dynamicViewFieldValue').text();
			this.logErrors = IP.utils.getViewField(IP.logErrorsId).siblings('.dynamicViewFieldValue').text();
			this.emailNotification = IP.utils.getViewField(IP.emailNotificationId).siblings('.dynamicViewFieldValue').text();
			this.name = IP.utils.getViewField(IP.nameId).siblings('.dynamicViewFieldValue').text();
			this.overwriteMode = IP.utils.getViewField(IP.overwriteFieldsId).siblings('.dynamicViewFieldValue').text();
			this.destinationRdoName = this.destinationConfiguration.ArtifactTypeName;
			this.sourceProviderName = IP.utils.getViewField(IP.sourceProviderId).siblings('.dynamicViewFieldValue').text();
			this.sourceConfiguration = "";

			updateSourceConfigurationField();
		};

		var downloadSummaryPage = function (url, fieldId, dataContainer) {
			IP.data.ajax({
				url: url,
				type: 'POST',
				data: JSON.stringify({ id: IP.artifactid, controllerType: IP.apiControllerName }),
				dataType: 'html'
			}).then(function (result) {
				self.displaySummaryPage(fieldId, result, dataContainer);
			});
		}

		this.downloadAndDisplay = function () {
			var dataContainer = new DataContainer();
			dataContainer.hideContainer();

			var url = IP.utils.generateWebURL('SummaryPage', 'GetSummaryPage');
			downloadSummaryPage(url, IP.nameId, dataContainer);
		}
	};

	var SummaryPageSchedulingTab = function () {
		var self = this;

		var DataContainer = function () {
			var self = this;

			var getScheduler = function () {
				IP.data.ajax({
					url: IP.utils.generateWebAPIURL(IP.apiControllerName, IP.artifactid),
					type: 'Get',
					async: false
				}).then(function (result) {
					self.lastRun = result.lastRun;
					self.nextRun = result.nextRun;
					self.scheduler = result.scheduler;
				});
			};

			var getWindowsTimeZones = function () {
				IP.data.ajax({
					contentType: "application/json",
					dataType: "json",
					headers: { "X-CSRF-Header": "-" },
					type: "POST",
					url:
						"/Relativity.REST/api/Relativity.Services.TimeZone.ITimeZoneModule/Time%20Zone%20Service/GetWindowsTimeZonesAsync",
					async: false
				})
					.then(function (result) {
						self.windowsTimeZones = result;
					});
			};

			getScheduler();
			getWindowsTimeZones();

			this.windowsTimeZones = [];
			this.scheduler = null;
			this.lastRun = null;
			this.nextRun = null;
		};

		var downloadSummaryPage = function (url, fieldId, dataContainer) {
			IP.data.ajax({
				url: url,
				type: 'GET',
				dataType: 'html'
			}).then(function (result) {
				self.displaySummaryPage(fieldId, result, dataContainer);
			});
		}

		this.downloadAndDisplay = function () {
			var dataContainer = new DataContainer();
			var url = IP.utils.generateWebURL('SummaryPage', 'SchedulerSummaryPage');
			downloadSummaryPage(url, IP.params["scheduleRuleId"], dataContainer);
		}
	};

	SummaryPageGeneralTab.prototype.displaySummaryPage = displaySummaryPage;
	SummaryPageSchedulingTab.prototype.displaySummaryPage = displaySummaryPage;

	var generalTab = new SummaryPageGeneralTab();
	generalTab.downloadAndDisplay();
	
	var schedulingTab = new SummaryPageSchedulingTab();
	schedulingTab.downloadAndDisplay();
});