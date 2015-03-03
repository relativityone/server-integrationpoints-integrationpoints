var IP = IP || {};
(function (root) {
	root.importNow = function (artifactId, appid) {

		window.Dragon.dialogs.showConfirm({
			message: 'Are you sure you want to import data now?',
			title: 'Import Now',
			showCancel: true,
			width: 450,
			success: function (calls) {
				calls.close();
				var ajax = IP.data.ajax({
					type: 'post',
					url: root.utils.generateWebAPIURL('ImportNow'),
					data: JSON.stringify({
						"appId": appid,
						"artifactId": artifactId
					})
				});
				ajax.then(function () {
					IP.message.info.raise("Data will now be imported from the source provider.", $(".cardContainer"));
				});
			}
		});
	};

	var _convertUTCToLocal = function () {

	};

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
		var v = IP.utils.stringNullOrEmpty(this.value) ? '' : this.value;
		IP.utils.updateField($newTr, this.key, v);
		$newTr.find('input').attr('id', IP.utils.toCamelCase(this.key)).removeAttr('faartifactid').removeAttr('fafriendlyname');
		$tr.after($newTr);
		$tr = $newTr;
	});
	$root.parent('tr').hide();
};

$(function () {
	//Scheduler
	var ruleFieldId = IP.params['scheduleRuleId'];
	var $field = IP.utils.getViewField(ruleFieldId).siblings('.dynamicViewFieldValue');
	$field.text('');
	IP.data.ajax({
		url: IP.utils.generateWebAPIURL('IntegrationPointsAPI', IP.artifactid),
		type: 'Get'
	}).then(function (result) {
		var result = result.scheduler;
		var obj = [
			{ key: 'Frequency', value: result.selectedFrequency },
			{ key: 'Start Date', value: result.startDate },
			{ key: 'End Date', value: result.endDate },
			{ key: 'Scheduled Time (UTC)', value: IP.timeUtil.utcToAmPm(result.scheduledTime) }
		];
		IP.utils.createFields($field, obj);
	}, function () {
		//TODO: what!?
		debugger;
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
		IP.data.ajax({
			url: url,
			data: str,
			type: 'post'
		}).then(function (result) {
			d.resolve(result);
		});

		return d.promise;
	};
	var $field = IP.utils.getViewField(IP.sourceConfiguration).siblings('.dynamicViewFieldValue');
	var settings = $field.text();
	$field.text('');
	_getSource(settings).then(function (result) {
		IP.utils.createFields($field, result);
	}, function () {
		//TODO: what!?
		debugger;
	});

});