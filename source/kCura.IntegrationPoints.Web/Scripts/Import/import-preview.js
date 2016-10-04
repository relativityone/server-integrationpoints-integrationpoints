var IP = IP || {};

(function (root) {

    $.get(root.utils.getBaseURL() + "/api/ImportPreview/DummbyData/")
        .done(function (data) {
            // data = $.parseJSON(data);
            var formattedHeaders = [];
            $.each(data.Headers, function (e, f) {
                formattedHeaders.push({ "sTitle": data.Headers[e] });
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

            function updateMoveNext() {
                var info = csvTable.page.info();
                var totalPages = info.pages;

                if (info.end == info.recordsDisplay) {
                    $("#pag-nav-move-next").attr("class", "pag-nav-button pag-nav-next-dis");
                } else if (info.recordsTotal <= (info.length * totalPages)) {
                    $("#pag-nav-move-next").attr("class", "pag-nav-button pag-nav-next");
                };
            };

            function updateMovePrevious() {
                var info = csvTable.page.info();
                var currentPage = info.page;

                if (currentPage > 0) {
                    $("#pag-nav-move-previous").attr("class", "pag-nav-button pag-nav-previous");
                } else if (currentPage == 0) {
                    $("#pag-nav-move-previous").attr("class", "pag-nav-button pag-nav-previous-dis");
                };
            };

            function updateMoveLast() {
                var info = csvTable.page.info();
                var currentPage = info.page;
                var lastPage = (info.pages - 1);

                if (currentPage != lastPage) {
                    $("#pag-nav-move-last").attr("class", "pag-nav-button pag-nav-last");
                } else {
                    $("#pag-nav-move-last").attr("class", "pag-nav-button pag-nav-last-dis");
                };

            };

            function updateMoveFirst() {
                var info = csvTable.page.info();
                var currentpage = info.page;

                if (currentpage != 0) {
                    $("#pag-nav-move-first").attr("class", "pag-nav-button pag-nav-first");
                } else {
                    $("#pag-nav-move-first").attr("class", "pag-nav-button pag-nav-first-dis");
                }
            };
            //todo: update the mesage according to same behvior in relativity
            function updateItems() {
                var info = csvTable.page.info();
                var currentPage = info.page;
                var totalPage = info.pages;

                $(".pag-items").text("- " + currentPage + "( of " + totalPage + ")");

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

            function updatePaging() {
                /*Todo check to see if there is a way to enable "items for page" without setting to true*/
                $("#csvData-table_paginate").hide();

                updateMoveFirst();
                updateMoveLast();
                updateMovePrevious();
                updateMoveNext();
                updateItems();
            };

            updatePaging();

            $("#pag-nav-move-next").on("click", function () {
                csvTable.page('next').draw('page');
                updatePaging();

            });

            $("#pag-nav-move-previous").on("click", function () {
                csvTable.page('previous').draw('page');
                updatePaging();

            });

            $("#pag-nav-move-first").on("click", function () {
                csvTable.page('first').draw('page');
                updatePaging();
            });

            $("#pag-nav-move-last").on("click", function () {
                csvTable.page('last').draw('page');
                updatePaging();
            });

            $("#itemsPerPageSelect").live("click", function () {
                var pageLength = $("#itemsPerPageSelect option:selected").val();

                csvTable.page.len(pageLength).draw();
                updatePaging();

            });

            $("#pag-number").change(function () {
                var input = $(this).val();
                var info = csvTable.page.info();
                var totalItems = info.recordsTotal;
                var itemsPerPage = info.length;
                var convertedInput = parseInt(input);
                var result = getPageNumber(itemsPerPage, totalItems, convertedInput);

                csvTable.page(result).draw('page');
                updatePaging();
            });

        });
})(IP);