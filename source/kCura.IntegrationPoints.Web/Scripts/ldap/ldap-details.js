
$(function () {
	var isSchedulerEnabled = $("td").filter(function() {
		return $(this).text() == 'Enable Scheduler:';
	}).siblings("td")[0].lastElementChild.value === "checked";
	var status;

	if (isSchedulerEnabled === true) {
		status = $("td").filter(function () {
			var text = $(this).text();
			return $(this).text() == 'Enable Scheduler:';
		}).siblings("td")[0].lastElementChild.value;
	} else {
		status = $("span").filter(function() {
			var text = $(this).text();
			return $(this).text().trim() == 'Status';
		}).parents("table")[0];
		$(status).css("display", "none");
	}
});
