var IP = IP || {};

(function (root) {
    var previewJobId = -1;
    var intervalId = -1;
    var percent = 0;

    $("#tableData").hide();

    $.get(root.utils.getBaseURL() + "/api/ImportPreview/CreatePreviewJob/" + $("#workspaceId").text())
    .done(function (data) {
        previewJobId = data;
        $("#progressBar").css("width", percent);
        intervalId = setInterval(
            function () {
                $.get(root.utils.getBaseURL() + "/api/ImportPreview/CheckProgress/" + previewJobId)
                .done(function (data) {
                    var percent = (data.BytesRead / data.TotalBytes) * 100;
                    $("#statusMessage").html("In Process");
                    $("#progressBar").css("width", percent + "%");
                    $("#total-bytes-read").html(data.BytesRead);
                    $("#total-bytes").html(data.TotalBytes);

                    //TODO: bind data to summary section here
                    //console.log(data);
                    //check if the Preview is complete
                    if (data.IsComplete) {
                        $("#statusMessage").html("Completed");
                        $("#statusMessage").attr("class", "active-transfer-status-success");
                        $("#progressBar").attr("class", "progress-bar-indicator progress-complete");
                        $("#tableData").show();
                        clearInterval(intervalId);
                        GetPreviewTableData(previewJobId);
                    }
                });
            }, 2000);
    });

    var GetPreviewTableData = function (jobId) {
        $.get(root.utils.getBaseURL() + "/api/ImportPreview/GetImportPreviewTable/" + jobId)
        .done(function (data) {
            var formattedHeaders = [];
            $.each(data.Header, function (e, f) {
                formattedHeaders.push({ "sTitle": data.Header[e] });
            });

            var csvTable = $("#csvData-table").DataTable({
                "bFilter": false,
                "bInfo": false,
                "destroy": false,
                "paging": true,
                "lengthChange": false,
                "responsive": true,
                "info": true,
                "columns": formattedHeaders,
                "columnDefs": [{
                    "searchable": false,
                    "orderable": false,
                    "targets": 0
                }]

            });
            //todo: find a way to insert a column for the row index on the controller or on client -side
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

            function populateTotalRecords() {
                var info = csvTable.page.info();
                var totalRecords = $("#totalRecord");

                totalRecords.text(info.recordsTotal);
            };

            function updateMoveNextBtn() {
                var info = csvTable.page.info();
                var totalPages = info.pages;

                if (info.end == info.recordsDisplay) {
                    $("#pag-nav-move-next").attr("class", "pag-nav-button pag-nav-next-dis");
                } else if (info.recordsTotal <= (info.length * totalPages)) {
                    $("#pag-nav-move-next").attr("class", "pag-nav-button pag-nav-next");
                };
            };

            function updateMovePreviousBtn() {
                var info = csvTable.page.info();
                var currentPage = info.page;

                if (currentPage > 0) {
                    $("#pag-nav-move-previous").attr("class", "pag-nav-button pag-nav-previous");
                } else if (currentPage == 0) {
                    $("#pag-nav-move-previous").attr("class", "pag-nav-button pag-nav-previous-dis");
                };
            };

            function updateMoveLastBtn() {
                var info = csvTable.page.info();
                var currentPage = info.page;
                var lastPage = (info.pages - 1);

                if (currentPage != lastPage) {
                    $("#pag-nav-move-last").attr("class", "pag-nav-button pag-nav-last");
                } else {
                    $("#pag-nav-move-last").attr("class", "pag-nav-button pag-nav-last-dis");
                };
            };

            function updateMoveFirstBtn() {
                var info = csvTable.page.info();
                var currentpage = info.page;

                if (currentpage != 0) {
                    $("#pag-nav-move-first").attr("class", "pag-nav-button pag-nav-first");
                } else {
                    $("#pag-nav-move-first").attr("class", "pag-nav-button pag-nav-first-dis");
                }
            };

            function updateItemNumber() {
                var info = csvTable.page.info();
                var totalItems = info.recordsTotal;

                $(".pag-items").text("- " + info.end + " ( of " + totalItems + ")");
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
                var rowNumber = $("#row-number");
                var info = csvTable.page.info();
                var totalItems = info.recordsTotal;

                $(".pag-items").text("- " + info.end + " ( of " + totalItems + ")");

                rowNumber.val(info.start + 1);
            };

            function updatePaging() {
                $("#csvData-table_paginate").hide();

                updateMoveFirstBtn();
                updateMoveLastBtn();
                updateMovePreviousBtn();
                updateMoveNextBtn();
                updateItemNumber();
            };

            updatePaging();
            updateItemUi();

            $("#import-load-file-summary-toggle").on("click", function () {
                if ($(this).attr("class") == "arrow-collapsed-summary") {
                    $(this).attr("class", "arrow-expanded-summary");
                } else if ($(this).attr("class") == "arrow-expanded-summary") {
                    $(this).attr("class", "arrow-collapsed-summary");
                }
                $("#transfer-progress-div").toggle();
            });

            $("#pag-nav-move-next").on("click", function () {
                csvTable.page('next').draw('page');

                updateItemUi();
                updatePaging();
            });

            $("#pag-nav-move-previous").on("click", function () {
                csvTable.page('previous').draw('page');
                updatePaging();
            });

            $("#pag-nav-move-first").on("click", function () {
                csvTable.page('first').draw('page');

                updateItemUi();
                updatePaging();
            });

            $("#pag-nav-move-last").on("click", function () {
                csvTable.page('last').draw('page');

                updateItemUi();
                updatePaging();
            });

            $("#itemsPerPageSelect").change(function () {
                var pageLength = $("#itemsPerPageSelect option:selected").val();

                csvTable.page.len(pageLength);
                csvTable.page('first').draw('page');

                updateItemUi();
                updatePaging();
            });

            $("#row-number").change(function () {
                var info = csvTable.page.info();
                var totalItems = info.recordsTotal;
                var itemsPerPage = info.length;
                var convertedInput = Math.floor(parseInt($(this).val()) - 1);

                csvTable.page(getPageNumber(itemsPerPage, totalItems, convertedInput)).draw('page');

                updateItemUi();
                updatePaging();
            });


        });
    };
})(IP);