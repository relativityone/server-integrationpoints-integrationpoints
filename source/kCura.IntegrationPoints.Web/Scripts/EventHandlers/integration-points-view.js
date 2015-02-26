var IP = IP || {};
(function(root) {
	root.importNow = function(artifactId, appid) {
		
		window.Dragon.dialogs.showConfirm({
			message: 'Are you sure you want to import data now?',
			title: 'Import Now',
			showCancel: true,
			width: 450,
			success: function(calls) {
				calls.close();
				var ajax = IP.data.ajax({
					type: 'post',
					url: root.utils.generateWebAPIURL('ImportNow'),
					data: JSON.stringify({
						"appId": appid,
						"artifactId": artifactId
					})
				});
				ajax.then(function() {
					IP.message.info.raise("Data will now be imported from the source provider.", $(".cardContainer"));
				});
			}
		});
	};
	
	var _convertUTCToLocal = function() {

	};

	var config = {
		time: {
			longDate: Date.CultureInfo.formatPatterns.shortDate + ' ' + Date.CultureInfo.formatPatterns.shortTime
		}
	};
	IP.redirect.set(document.URL);
	

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
$(window).load(function() {
	$(".consoleContainer .consoleButtonDisabled").attr("title", "You do not have permission to import.");
});

$(window).unload(function () {
	
	if (IP.isEdit === "Edit") {
		IP.redirect.reset(true);
	} else {
		IP.redirect.reset(false);
	}
});
