$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var productCommentService = window.lC.crawler.backOffice.productComments.productComments;
	
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
	    $('#ProductFilterLookupOpenButton').on('click', '', function () {
        lastNpDisplayNameId = 'Product_Filter_Name';
        lastNpIdId = 'ProductIdFilter';
        _lookupModal.open({
            currentId: $('#ProductIdFilter').val(),
            currentDisplayName: $('#Product_Filter_Name').val(),
            serviceMethod: function () {
                            
                            return window.lC.crawler.backOffice.productComments.productComments.getProductLookup;
            }
        });
    });
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "ProductComments/CreateModal",
        scriptUrl: "/Pages/ProductComments/createModal.js",
        modalClass: "productCommentCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "ProductComments/EditModal",
        scriptUrl: "/Pages/ProductComments/editModal.js",
        modalClass: "productCommentEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            name: $("#NameFilter").val(),
			content: $("#ContentFilter").val(),
			likesMin: $("#LikesFilterMin").val(),
			likesMax: $("#LikesFilterMax").val(),
			createdAtMin: $("#CreatedAtFilterMin").data().datepicker.getFormattedDate('yyyy-mm-dd'),
			createdAtMax: $("#CreatedAtFilterMax").data().datepicker.getFormattedDate('yyyy-mm-dd'),
			productId: $("#ProductIdFilter").val()
        };
    };

    var dataTable = $("#ProductCommentsTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(productCommentService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.ProductComments.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.productComment.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.ProductComments.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    productCommentService.delete(data.record.productComment.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "productComment.name" },
			{ data: "productComment.content" },
			{ data: "productComment.likes" },
            {
                data: "productComment.createdAt",
                render: function (createdAt) {
                    if (!createdAt) {
                        return "";
                    }
                    
					var date = Date.parse(createdAt);
                    return (new Date(date)).toLocaleDateString(abp.localization.currentCulture.name);
                }
            },
            {
                data: "product.name",
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

    $("#NewProductCommentButton").click(function (e) {
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
