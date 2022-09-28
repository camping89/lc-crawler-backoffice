$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var crawlerAccountService = window.lC.crawler.backOffice.crawlerAccounts.crawlerAccounts;
	
	
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "CrawlerAccounts/CreateModal",
        scriptUrl: "/Pages/CrawlerAccounts/createModal.js",
        modalClass: "crawlerAccountCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "CrawlerAccounts/EditModal",
        scriptUrl: "/Pages/CrawlerAccounts/editModal.js",
        modalClass: "crawlerAccountEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            username: $("#UsernameFilter").val(),
			password: $("#PasswordFilter").val(),
			twoFactorCode: $("#TwoFactorCodeFilter").val(),
			accountType: $("#AccountTypeFilter").val(),
			accountStatus: $("#AccountStatusFilter").val(),
			email: $("#EmailFilter").val(),
			emailPassword: $("#EmailPasswordFilter").val(),
            isActive: (function () {
                var value = $("#IsActiveFilter").val();
                if (value === undefined || value === null || value === '') {
                    return '';
                }
                return value === 'true';
            })()
        };
    };

    var dataTable = $("#CrawlerAccountsTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(crawlerAccountService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.CrawlerAccounts.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.CrawlerAccounts.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    crawlerAccountService.delete(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "username" },
			{ data: "password" },
			{ data: "twoFactorCode" },
            {
                data: "accountType",
                render: function (accountType) {
                    if (accountType === undefined ||
                        accountType === null) {
                        return "";
                    }

                    var localizationKey = "Enum:AccountType." + accountType;
                    var localized = l(localizationKey);

                    if (localized === localizationKey) {
                        abp.log.warn("No localization found for " + localizationKey);
                        return "";
                    }

                    return localized;
                }
            },
            {
                data: "accountStatus",
                render: function (accountStatus) {
                    if (accountStatus === undefined ||
                        accountStatus === null) {
                        return "";
                    }

                    var localizationKey = "Enum:AccountStatus." + accountStatus;
                    var localized = l(localizationKey);

                    if (localized === localizationKey) {
                        abp.log.warn("No localization found for " + localizationKey);
                        return "";
                    }

                    return localized;
                }
            },
			{ data: "email" },
			{ data: "emailPassword" },
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

    $("#NewCrawlerAccountButton").click(function (e) {
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
