var IP = IP || {};

(function (root, opener) {
    var idSelector = function (name) { return '#' + name; }
    var classSelector = function (name) { return '.' + name; }
    var previewJobId = -1;
    var intervalId = -1;
    var percent = 0;
    var timerHandle;
    var settings = opener.RelativityImportPreviewSettings;
    var workspaceId = ("/" + opener.RelativityImportPreviewSettings.WorkspaceId);
    var fieldMapping = opener.top.getCurrentIpFieldMapping();
    var choiceFieldsPromise = opener.top.getMappedChoiceFieldsPromise();
    var timerCount = 0;
    var timerRequest = true;
    var previewTypeEnum = opener.PreviewTypeEnum;

    //Element names
    var ROW = 'row';
    var ELAPSED_TIME = 'elapsed-time';
    var PROGRESS_BAR = 'progressBar';
    var STATUS_MESSAGE = 'statusMessage';
    var TOTAL_BYTE_READ = 'total-bytes-read';
    var TOTAL_RECORD = 'totalRecord';
    var ERRORS_DIV = 'errors-div';
    var PAG_NEXT = 'pag-nav-move-next';
    var PAG_PREVIOUS = 'pag-nav-move-previous';
    var PAG_LAST = 'pag-nav-move-last';
    var PAG_FIRST = 'pag-nav-move-first';
    var PAG_ITEMS = 'pag-items';
    var ROW_NUMBER = 'row-number';
    var CSV_DATA_TABLE_PAGINATE = 'csvData-table_paginate';
    var IMPORT_LOAD_FILE_TOGGLE = 'import-load-file-summary-toggle';
    var TRANSFER_PROGRESS = 'transfer-progress-div';
    var ITEMS_PER_PAGE_SELECT = 'itemsPerPageSelect';
    var CSV_TABLE = 'csvData-table';
    var TOTAL_BYTES = 'total-bytes';
    var PAG_NAV_BTN = ' pag-nav-button ';
    var BTN_DISABLE = ' btn-disable ';
    var BTN_ACTIVE = ' btn-active ';
    var MOVE_NEXT = ' icon-move-next ';
    var MOVE_PREVIOUS = 'icon-move-previous';
    var MOVE_LAST = 'icon-move-last';
    var MOVE_FIRST = 'icon-move-first';
    var ARROW_COLLAPSED = 'arrow-collapsed-summary';
    var ARROW_EXPANDED = 'arrow-expanded-summary';
    var PREVIEW_FILE_PERCENT = 'previewFilePercent';
    var CLOSE_BTN = 'preview-file-closeBtn';


    function showTableData() {
        var $el = $(classSelector(ROW));

        $el.show();
    };

    function timerRun() {
        if (timerRequest) {
            var format = (new Date).clearTime()
                .addSeconds(timerCount++)
                .toString('HH:mm:ss');
            $(classSelector(ELAPSED_TIME)).text(format);
        } else {
            clearInterval(timerHandle);
        }
    };
    timerHandle = setInterval(timerRun, 1000);

    choiceFieldsPromise.then(function (choiceFields) {

    	//take the settings we need to send to the CreatePreviewJob action
    	var previewSettingsData = {
    		WorkspaceId: settings.WorkspaceId,
    		PreviewType: settings.PreviewType,
    		LoadFile: settings.LoadFile,
    		EncodingType: settings.EncodingType,
    		AsciiColumn: settings.AsciiColumn,
    		AsciiQuote: settings.AsciiQuote,
    		AsciiNewLine: settings.AsciiNewLine,
    		AsciiMultiLine: settings.AsciiMultiLine,
    		AsciiNestedValue: settings.AsciiNestedValue,
    		FieldMapping: $.parseJSON(fieldMapping),
    		ChoiceFields: choiceFields
    	};

		root.data.ajax({
			type: "post",
			url: root.utils.getBaseURL() + workspaceId + "/api/ImportPreview/CreatePreviewJob",
			data: JSON.stringify(previewSettingsData),
			dataType: 'json'
		})
		.done(function (data) {
			previewJobId = data;
			$(idSelector(PROGRESS_BAR)).css("width", percent);
			intervalId = setInterval(
				function () {
					$.get(root.utils.getBaseURL() + workspaceId + "/api/ImportPreview/CheckProgress/" + previewJobId)
					.done(function (data) {
						var statusMsg = $(idSelector(STATUS_MESSAGE));
						var progBar = $(idSelector(PROGRESS_BAR));
						var totalByte = $(idSelector(TOTAL_BYTES));
						var preFilePercent = $(idSelector(PREVIEW_FILE_PERCENT));

						statusMsg.html("In Process");
						//if we're only reading the first 1000 rows of a large file, bytes read comes back as -1
						//we can just show the progress as 100% in this case
						var percent;

						if (data.BytesRead != -1) {
							$("#total-bytes-read").html(data.BytesRead);
							percent = (Math.floor((data.BytesRead / data.TotalBytes) * 100) + "%");
						} else {
							percent = "100%";
						}
						progBar.css("width", percent);
						preFilePercent.html(percent);
						totalByte.html(data.TotalBytes);

						if (data.BytesRead != -1) {
							$(idSelector(TOTAL_BYTE_READ)).html(data.BytesRead);
							percent = (data.BytesRead / data.TotalBytes) * 100;
						} else {
							percent = 100;
						}
						progBar.css("width", percent + "%");
						totalByte.html(data.TotalBytes);

						//check if the Preview is complete or failed
						if (data.IsFailed) {
							timerRequest = false;
							statusMsg.html("Error");
							statusMsg.attr("class", "active-transfer-status-failed");
							progBar.attr("class", "progress-bar-indicator progress-failed");
							progBar.css("width", "100%");
							clearInterval(intervalId);
							console.log(data.ErrorMessage);
						}
						else if (data.IsComplete) {
							timerRequest = false;
							statusMsg.html("Completed");
							statusMsg.attr("class", "active-transfer-status-success");
							progBar.attr("class", "progress-bar-indicator progress-complete");
							showTableData();
							progBar.css("width", percent + "%");
							clearInterval(intervalId);
							GetPreviewTableData(previewJobId);
						}
					});
				}, 2000);
		});
    });

    var GetPreviewTableData = function (jobId) {
        $.get(root.utils.getBaseURL() + workspaceId + "/api/ImportPreview/GetImportPreviewTable/" + jobId)
        .done(function (data) {
            var formattedHeaders = [];
            $.each(data.Header, function (e, f) {
                formattedHeaders.push({ "sTitle": data.Header[e] });
            });

            var csvTable = $(idSelector(CSV_TABLE)).DataTable({
                "bFilter": false,
                "bInfo": false,
                "destroy": false,
                "paging": true,
                "lengthChange": false,
                "responsive": true,
                "info": true,
                "columns": formattedHeaders,
                "pageLength": 15,
                "columnDefs": [{
                    "searchable": false,
                    "orderable": false,
                    "targets": 0
                }]

            });
            csvTable.on('order.dt search.dt', function () {
                csvTable.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                    cell.innerHTML = i + 1;
                });
            }).draw();

            csvTable.clear();
            csvTable.draw();

            csvTable.rows.add(
                data.Data
            );

            csvTable.draw();
            populateTotalRecords();

            function colorErrorRows() {
                $.each(data.ErrorRows, function (arrayIndex, rowIndex) {
                    csvTable.row(rowIndex - 1).nodes().to$().addClass('error-row');
                });
            }

            function populateTotalRecords() {
                var info = csvTable.page.info();
                var totalRecords = $(idSelector(TOTAL_RECORD));
                var $el = $(idSelector(ERRORS_DIV));

                totalRecords.text(info.recordsTotal);

                (settings.PreviewType != previewTypeEnum.Errors) ? $el.text(data.ErrorRows.length) : $el.text(info.recordsTotal);
            };

            function updateMoveNextBtn() {
                var info = csvTable.page.info();
                var totalPages = info.pages;
                var $el = $(idSelector(PAG_NEXT));
                var active = PAG_NAV_BTN.concat(MOVE_NEXT, BTN_ACTIVE);
                var disable = PAG_NAV_BTN.concat(MOVE_NEXT, BTN_DISABLE);

                if (info.end == info.recordsDisplay) {
                    $el.attr("class", disable);
                } else if (info.recordsTotal <= (info.length * totalPages)) {
                    $el.attr("class", active);
                };
            };

            function updateMovePreviousBtn() {
                var info = csvTable.page.info();
                var currentPage = info.page;
                var $el = $(idSelector(PAG_PREVIOUS));
                var active = PAG_NAV_BTN.concat(MOVE_PREVIOUS, BTN_ACTIVE);
                var disable = PAG_NAV_BTN.concat(MOVE_PREVIOUS, BTN_DISABLE);

                if (currentPage > 0) {
                    $el.attr("class", active);
                } else if (currentPage == 0) {
                    $el.attr("class", disable);
                };
            };

            function updateMoveLastBtn() {
                var info = csvTable.page.info();
                var currentPage = info.page;
                var lastPage = (info.pages - 1);
                var $el = $(idSelector(PAG_LAST));
                var active = PAG_NAV_BTN.concat(MOVE_LAST, BTN_ACTIVE);
                var disable = PAG_NAV_BTN.concat(MOVE_LAST, BTN_DISABLE);

                (currentPage != lastPage) ? $el.attr("class", active) : $el.attr("class", disable);
            };

            function updateMoveFirstBtn() {
                var info = csvTable.page.info();
                var currentpage = info.page;
                var $el = $(idSelector(PAG_FIRST));
                var active = PAG_NAV_BTN.concat(MOVE_FIRST, BTN_ACTIVE);
                var disable = PAG_NAV_BTN.concat(MOVE_FIRST, BTN_DISABLE);

                (currentpage != 0) ? $el.attr("class", active) : $el.attr("class", disable);
            };

            function updateItemNumber() {
                var info = csvTable.page.info();
                var totalItems = info.recordsTotal;

                $(classSelector(PAG_ITEMS)).text("- " + info.end + " ( of " + totalItems + ")");
            };

            function getPageNumber(itemsPerPage, totalItems, itemNumber) {
                if (itemNumber < 0) {
                    return 0;
                } else if (itemNumber > (totalItems - 1)) {
                    return Math.floor(totalItems / itemsPerPage);
                } else {
                    return Math.floor(itemNumber / itemsPerPage);
                };
            };

            function updateItemUi() {
                var rowNumber = $(idSelector(ROW_NUMBER));
                var info = csvTable.page.info();
                var totalItems = info.recordsTotal;

                $(classSelector(PAG_ITEMS)).text("- " + info.end + " ( of " + totalItems + ")");

                rowNumber.val(info.start + 1);
            };

            function updatePaging() {
                $(idSelector(CSV_DATA_TABLE_PAGINATE)).hide();

                updateMoveFirstBtn();
                updateMoveLastBtn();
                updateMovePreviousBtn();
                updateMoveNextBtn();
                updateItemNumber();
            };

            function closeWindow() {
                $(classSelector(CLOSE_BTN)).click(function () {
                    window.close();
                });
            };

            updatePaging();
            updateItemUi();
            colorErrorRows();
            closeWindow();

            $(idSelector(IMPORT_LOAD_FILE_TOGGLE)).on("click", function () {
                if ($(this).attr("class") == ARROW_COLLAPSED) {

                    $(this).attr("class", ARROW_EXPANDED);
                } else if ($(this).attr("class") == ARROW_EXPANDED) {
                    $(this).attr("class", ARROW_COLLAPSED);
                }

                $(idSelector(TRANSFER_PROGRESS)).toggle();
            });

            $(idSelector(PAG_NEXT)).on("click", function () {
                csvTable.page('next').draw('page');
                updateItemUi();
                updatePaging();
            });

            $(idSelector(PAG_PREVIOUS)).on("click", function () {
                csvTable.page('previous').draw('page');
                updatePaging();
            });

            $(idSelector(PAG_FIRST)).on("click", function () {
                csvTable.page('first').draw('page');
                updateItemUi();
                updatePaging();
            });

            $(idSelector(PAG_LAST)).on("click", function () {
                csvTable.page('last').draw('page');
                updateItemUi();
                updatePaging();
            });

            $(idSelector(ITEMS_PER_PAGE_SELECT)).change(function () {
                var pageLength = $(this).val();

                csvTable.page.len(pageLength);
                csvTable.page('first').draw('page');
                updateItemUi();
                updatePaging();
            });

            $(idSelector(ROW_NUMBER)).change(function () {
                var info = csvTable.page.info();
                var totalItems = info.recordsTotal;
                var itemsPerPage = info.length;
                var convertedInput = Math.floor(parseInt($(this).val()) - 1);

                csvTable.page(getPageNumber(itemsPerPage, totalItems, convertedInput)).draw('page');
                updateItemUi();
                updatePaging();
            });

            $("#csvData-table thead").on('click', 'th', function () {
                updateItemUi();
                updatePaging();
            });
        });
    };
})(IP, this.opener);
