$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var crawlerProxyService = window.lC.crawler.backOffice.crawlerProxies.crawlerProxies;
	
	
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "CrawlerProxies/CreateModal",
        scriptUrl: "/Pages/CrawlerProxies/createModal.js",
        modalClass: "crawlerProxyCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "CrawlerProxies/EditModal",
        scriptUrl: "/Pages/CrawlerProxies/editModal.js",
        modalClass: "crawlerProxyEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            ip: $("#IpFilter").val(),
			port: $("#PortFilter").val(),
			protocol: $("#ProtocolFilter").val(),
			username: $("#UsernameFilter").val(),
			password: $("#PasswordFilter").val(),
			pingedAtMin: $("#PingedAtFilterMin").data().datepicker.getFormattedDate('yyyy-mm-dd'),
			pingedAtMax: $("#PingedAtFilterMax").data().datepicker.getFormattedDate('yyyy-mm-dd'),
            isActive: (function () {
                var value = $("#IsActiveFilter").val();
                if (value === undefined || value === null || value === '') {
                    return '';
                }
                return value === 'true';
            })()
        };
    };

    var dataTable = $("#CrawlerProxiesTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(crawlerProxyService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.CrawlerProxies.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.CrawlerProxies.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    crawlerProxyService.delete(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "ip" },
			{ data: "port" },
			{ data: "protocol" },
			{ data: "username" },
			{ data: "password" },
            {
                data: "pingedAt",
                render: function (pingedAt) {
                    if (!pingedAt) {
                        return "";
                    }
                    
					var date = Date.parse(pingedAt);
                    return (new Date(date)).toLocaleDateString(abp.localization.currentCulture.name);
                }
            },
            {
                data: "isActive",
                render: function (isActive) {
                    return isActive ? l("Yes") : l("No");
                }
            }
        ]
    }));

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $("#NewCrawlerProxyButton").click(function (e) {
        e.preventDefault();
        createModal.open();
    });

	$("#SearchForm").submit(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $('#AdvancedFilterSectionToggler').on('click', function (e) {
        $('#AdvancedFilterSection').toggle();
    });

    $('#AdvancedFilterSection').on('keypress', function (e) {
        if (e.which === 13) {
            dataTable.ajax.reload();
        }
    });

    $('#AdvancedFilterSection select').change(function() {
        dataTable.ajax.reload();
    });
    
    
});
