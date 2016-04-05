(function () {
	$(document).ready(function () {
		disableEditOnJobHistory();
	});

	function disableEditOnJobHistory() {
		var $jobHistoryFieldName = $(createFieldNameFindString("JobHistory:"));
		$jobHistoryFieldName.removeClass("dynamicEditFieldName dynamicEditFieldNameRequired dynamicViewFieldName").addClass("dynamicEditFieldName");
		$jobHistoryFieldName.css("vertical-align", "middle");

		var $jobHistoryFieldRow = $jobHistoryFieldName.closest("tr");
		$jobHistoryFieldRow.find("a.JavascriptLink").hide();
		$jobHistoryFieldRow.find("a.clearButton").hide();
		$jobHistoryFieldRow.find("input").hide();

		var $editFieldValueCell = $jobHistoryFieldRow.find(".dynamicEditFieldValue");
		$editFieldValueCell.attr("class", "dynamicViewFieldValue");

		var $jobHistorySpan = $jobHistoryFieldRow.find("span[id*=\"description\"]");
		$jobHistorySpan.removeAttr("style");
	}

	function createFieldNameFindString(titleToFind) {
		/// <summary>Creates a string to find the title element for a field
		/// based on the title provided.</summary>
		/// <param name="titleToFind" type="Object">The title to find in the
		/// field title element</param>
		return ".editTable .dynamicEditFieldName:contains(\"" + titleToFind + "\")," +
			".editTable .dynamicEditFieldNameRequired:contains(\"" + titleToFind + "\"), " +
			".editTableColumn .dynamicViewFieldName:contains(\"" + titleToFind + "\"), ";
	}
})();