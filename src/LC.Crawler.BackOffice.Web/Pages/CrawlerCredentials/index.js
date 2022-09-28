$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var crawlerCredentialService = window.lC.crawler.backOffice.crawlerCredentials.crawlerCredentials;
	
        var lastNpIdId = '';
        var lastNpDisplayNameId = '';

        var _lookupModal = new abp.ModalManager({
            viewUrl: abp.appPath + "Shared/LookupModal",
            scriptUrl: "/Pages/Shared/lookupModal.js",
            modalClass: "navigationPropertyLookup"
        });

        $('.lookupCleanButton').on('click', '', function () {
            $(this).parent().find('input').val('');
        });

        _lookupModal.onClose(function () {
            var modal = $(_lookupModal.getModal());
            $('#' + lastNpIdId).val(modal.find('#CurrentLookupId').val());
            $('#' + lastNpDisplayNameId).val(modal.find('#CurrentLookupDisplayName').val());
        });
	    $('#CrawlerAccountFilterLookupOpenButton').on('click', '', function () {
        lastNpDisplayNameId = 'CrawlerAccount_Filter_Username';
        lastNpIdId = 'CrawlerAccountIdFilter';
        _lookupModal.open({
            currentId: $('#CrawlerAccountIdFilter').val(),
            currentDisplayName: $('#CrawlerAccount_Filter_Username').val(),
            serviceMethod: function () {
                            
                            return window.lC.crawler.backOffice.crawlerCredentials.crawlerCredentials.getCrawlerAccountLookup;
            }
        });
    });    $('#CrawlerProxyFilterLookupOpenButton').on('click', '', function () {
        lastNpDisplayNameId = 'CrawlerProxy_Filter_Ip';
        lastNpIdId = 'CrawlerProxyIdFilter';
        _lookupModal.open({
            currentId: $('#CrawlerProxyIdFilter').val(),
            currentDisplayName: $('#CrawlerProxy_Filter_Ip').val(),
            serviceMethod: function () {
                            
                            return window.lC.crawler.backOffice.crawlerCredentials.crawlerCredentials.getCrawlerProxyLookup;
            }
        });
    });
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "CrawlerCredentials/CreateModal",
        scriptUrl: "/Pages/CrawlerCredentials/createModal.js",
        modalClass: "crawlerCredentialCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "CrawlerCredentials/EditModal",
        scriptUrl: "/Pages/CrawlerCredentials/editModal.js",
        modalClass: "crawlerCredentialEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            dataSourceType: $("#DataSourceTypeFilter").val(),
			crawledAtMin: $("#CrawledAtFilterMin").data().datepicker.getFormattedDate('yyyy-mm-dd'),
			crawledAtMax: $("#CrawledAtFilterMax").data().datepicker.getFormattedDate('yyyy-mm-dd'),
            isAvailable: (function () {
                var value = $("#IsAvailableFilter").val();
                if (value === undefined || value === null || value === '') {
                    return '';
                }
                return value === 'true';
            })(),
			crawlerAccountId: $("#CrawlerAccountIdFilter").val(),			crawlerProxyId: $("#CrawlerProxyIdFilter").val()
        };
    };

    var dataTable = $("#CrawlerCredentialsTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(crawlerCredentialService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.CrawlerCredentials.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.crawlerCredential.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.CrawlerCredentials.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    crawlerCredentialService.delete(data.record.crawlerCredential.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{
                data: "crawlerCredential.dataSourceType",
                render: function (dataSourceType) {
                    if (dataSourceType === undefined ||
                        dataSourceType === null) {
                        return "";
                    }

                    var localizationKey = "Enum:DataSourceType." + dataSourceType;
                    var localized = l(localizationKey);

                    if (localized === localizationKey) {
                        abp.log.warn("No localization found for " + localizationKey);
                        return "";
                    }

                    return localized;
                }
            },
            {
                data: "crawlerCredential.crawledAt",
                render: function (crawledAt) {
                    if (!crawledAt) {
                        return "";
                    }
                    
					var date = Date.parse(crawledAt);
                    return (new Date(date)).toLocaleDateString(abp.localization.currentCulture.name);
                }
            },
            {
                data: "crawlerCredential.isAvailable",
                render: function (isAvailable) {
                    return isAvailable ? l("Yes") : l("No");
                }
            },
            {
                data: "crawlerAccount.username",
                defaultContent : "", 
                orderable: false
            },
            {
                data: "crawlerProxy.ip",
                defaultContent : "", 
                orderable: false
            }
        ]
    }));

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $("#NewCrawlerCredentialButton").click(function (e) {
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
