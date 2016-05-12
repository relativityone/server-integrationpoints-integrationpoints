var IP = IP || {};
(function (root) {
	root.importNow = function (artifactId, appid) {

		var appendOnlyMessageStart = "You are about to create ";
		var appendOnlyMessageEnd = " folders in the destination workspace, would you still like to proceed?";
		var overlayOnlyMessage = "Only documents and their metadata with the same identifier will be overwritten, would you still like to proceed?";
		var appendOverlayMesssage = "All existing documents and their metadata in the target workspace that have the same identifier will be overwritten, would you still like to proceed?";

		var overwriteOption = $("[fafriendlyname=\"Overwrite Fields\"]").closest("tr").find(".dynamicViewFieldValue").text();
		var selectedMessage = "";
		if (overwriteOption === "Append Only") {
			var folderCount = 0;
			IP.data.ajax({
				type: "get",
				url: IP.utils.generateWebAPIURL("FolderPath", "GetFolderCount", artifactId),
				async: false,
				success: function(result) {
					folderCount = result;
				}
			});
			selectedMessage = appendOnlyMessageStart + folderCount + appendOnlyMessageEnd;
		} else if (overwriteOption === "Overlay Only") {
			selectedMessage = overlayOnlyMessage;
		} else if (overwriteOption === "Append/Overlay") {
			selectedMessage = appendOverlayMesssage;
		}

		window.Dragon.dialogs.showConfirm({
			message: selectedMessage,
			title: 'Run Now',
			showCancel: true,
			width: 450,
			success: function (calls) {
				calls.close();
				var ajax = IP.data.ajax({
					type: 'post',
					url: root.utils.generateWebAPIURL('Job'),
					data: JSON.stringify({
						"appId": appid,
						"artifactId": artifactId
					})
				});
				ajax.fail(function(value) {
					IP.message.error.raise("Failed to submit integration job. " + value.responseText, $(".cardContainer"));
				});
				ajax.done(function () {
					IP.message.info.raise("Data will now be imported from the source provider.", $(".cardContainer"));
				});
			}
		});
	};

	root.retryJob = function(artifactId, appId) {
		var ajax = IP.data.ajax({
			type: "POST",
			url: root.utils.generateWebAPIURL('Job/Retry'),
			async: true,
			data: JSON.stringify({
				"appId": appId,
				"artifactId": artifactId
			})
		});

		ajax.fail(function(value) {
			IP.message.error.raise("Failed to submit the retry job. " + value.responseText, $(".cardContainer"));
		});
		ajax.done(function() {
			IP.message.info.raise("Retry job submitted. Data will now be imported from the source provider.", $(".cardContainer"));
		});
	}

	var config = {
		time: {
			longDate: Date.CultureInfo.formatPatterns.shortDate + ' ' + Date.CultureInfo.formatPatterns.shortTime
		}
	};
	IP.redirect.set(document.URL);

	IP.config = IP.config || {};
	IP.config.time = config.time;

	$(function () {
		var $editButtons = $(":contains('Edit')").closest('a');
		for (var i = 0; i < $editButtons.length; i++) {
			if ($editButtons[i].text === "Edit") {
				$($editButtons[i]).on("click", function () {
					IP.isEdit = "Edit";
				});
			}
		}
	});
	$(function () {
		$.each(IP.nextTimeid || [], function () {
			var $el = $('input[faartifactid="' + this + '"]').siblings('.dynamicViewFieldValue');
			var text = $el.text();
			var result = IP.timeUtil.utcDateToLocal(text, config.time.longDate);
			$el.text(result);
		});
	});

})(IP);
$(window).load(function () {
	$(".consoleContainer .consoleButtonDisabled").attr("title", "You do not have permission to import.");
});

$(window).unload(function () {

	if (IP.isEdit === "Edit") {
		IP.redirect.reset(true);
	} else {
		IP.redirect.reset(false);
	}
});

IP.utils.createFields = function ($root, fields) {
	var $tr = $root.parent('tr');
	$.each(fields || [], function () {
		var $newTr = $tr.clone();
		var value = this.value || this.Value;
		var key = this.key || this.Key;
		var v = IP.utils.stringNullOrEmpty(value) ? '' : value;
		IP.utils.updateField($newTr, key, v);
		$newTr.find('input').attr('id', IP.utils.toCamelCase(key)).removeAttr('faartifactid').removeAttr('fafriendlyname');
		$tr.after($newTr);
		$tr = $newTr;
	});
	$root.parent('tr').hide();
};

$(function () {
	//Scheduler
	var dayLookUp = {
		'1': 'first',
		'2': 'second',
		'3': 'third',
		'4': 'fourth',
		'5': 'last'
	};
	var dayOfMonthLookup = {
		'1': 'Monday',
		'2': 'Tuesday',
		'4': 'Wednesday',
		'8': 'Thursday',
		'16': 'Friday',
		'32': 'Saturday',
		'64': 'Sunday',
		'128': 'day'

	};

	var ruleFieldId = IP.params['scheduleRuleId'];
	var $field = IP.utils.getViewField(ruleFieldId).siblings('.dynamicViewFieldValue');
	$field.text('');
	IP.data.ajax({
		url: IP.utils.generateWebAPIURL('IntegrationPointsAPI', IP.artifactid),
		type: 'Get'
	}).then(function (result) {
		var result = result.scheduler || {};
		var sendOn;
		var obj = [
			{ key: 'Frequency', value: result.selectedFrequency || '' }
		];

		if (result.selectedFrequency === "Weekly") {
			obj.push({ 'key': 'Reoccur', value: 'Every ' + result.reoccur + ' week(s).' });
			sendOn = '<ul style="list-style-type: none; padding: 0; margin: 0;">';
			$.each(JSON.parse(result.sendOn).selectedDays || [], function () {
				sendOn += '<li>' + this + '</li>';
			});
			sendOn += '</ul>';
			obj.push({ 'key': 'Send On', value: sendOn });
		}
		else if (result.selectedFrequency === "Monthly") {
			sendOn = JSON.parse(result.sendOn);
			obj.push({ 'key': 'Reoccur', value: 'Every ' + result.reoccur + ' month(s).' });

			if (sendOn.monthChoice === 1) {
				obj.push({ key: 'Send On', value: 'The ' + dayLookUp[sendOn.selectedType] + ' ' + dayOfMonthLookup[sendOn.selectedDayOfTheMonth] + ' of the Month.' });
			} else if (sendOn.monthChoice == 2) {
				obj.push({ key: 'Send On', value: 'Day ' + sendOn.selectedDay + ' of the Month.' });
			}
		}
		obj.push({ key: 'Start Date', value: result.startDate || '' });
		obj.push({ key: 'End Date', value: result.endDate || '' });
		obj.push({ key: 'Scheduled Time (UTC)', value: IP.timeUtil.utcToAmPm(result.scheduledTime || '') });
		IP.utils.createFields($field, obj);
	}, function () {
		$field.text('There was an error retreving the scheduling information.');
	});
});

$(function () {
	var _getAppPath = function () {
		var newPath = window.location.pathname.split('/')[1];
		var url = window.location.protocol + '//' + window.location.host + '/' + newPath;
		return url;
	};

	//source settings
	var _getSource = function (str) {
		var d = IP.data.deferred().defer();
		var appID = IP.utils.getParameterByName('AppID', window.top);
		var artifactID = IP.artifactid;
		var obj = {
			applicationPath: _getAppPath(),
			appID: appID,
			artifactID: artifactID
		};
		var url = IP.utils.format(IP.params['sourceUrl'], obj);
		if (url) {
			IP.data.ajax({
				url: url,
				data: JSON.stringify(str),
				type: 'post'
			}).then(function (result) {
				d.resolve(result);
			}, function (r) {
				d.reject(r);
			});
		} else {
			d.reject();
		}

		return d.promise;
	};

	var $field = IP.utils.getViewField(IP.sourceConfiguration).siblings('.dynamicViewFieldValue');
	var settings = $field.text();
	$field.text('');
	_getSource(settings).then(function (result) {
		IP.utils.createFields($field, result);
	}, function () {
		$field.text('There was an error retrieving the source configuration.');
	});

});