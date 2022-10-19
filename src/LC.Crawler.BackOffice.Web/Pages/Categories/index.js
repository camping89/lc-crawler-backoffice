$(function () {
    var l = abp.localization.getResource("BackOffice");
	
	var categoryService = window.lC.crawler.backOffice.categories.categories;
	
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
	    $('#Category1FilterLookupOpenButton').on('click', '', function () {
        lastNpDisplayNameId = 'Category1_Filter_Name';
        lastNpIdId = 'Category1IdFilter';
        _lookupModal.open({
            currentId: $('#Category1IdFilter').val(),
            currentDisplayName: $('#Category1_Filter_Name').val(),
            serviceMethod: function () {
                            
                            return window.lC.crawler.backOffice.categories.categories.getCategoryLookup;
            }
        });
    });
    var createModal = new abp.ModalManager({
        viewUrl: abp.appPath + "Categories/CreateModal",
        scriptUrl: "/Pages/Categories/createModal.js",
        modalClass: "categoryCreate"
    });

	var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + "Categories/EditModal",
        scriptUrl: "/Pages/Categories/editModal.js",
        modalClass: "categoryEdit"
    });

	var getFilter = function() {
        return {
            filterText: $("#FilterText").val(),
            name: $("#NameFilter").val(),
			slug: $("#SlugFilter").val(),
			description: $("#DescriptionFilter").val(),
			categoryType: $("#CategoryTypeFilter").val(),
			parentCategoryId: $("#ParentCategoryIdFilter").val()
        };
    };

    var dataTable = $("#CategoriesTable").DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,
        scrollX: true,
        autoWidth: false,
        scrollCollapse: true,
        order: [[1, "asc"]],
        ajax: abp.libs.datatables.createAjax(categoryService.getList, getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l("Edit"),
                                visible: abp.auth.isGranted('BackOffice.Categories.Edit'),
                                action: function (data) {
                                    editModal.open({
                                     id: data.record.category.id
                                     });
                                }
                            },
                            {
                                text: l("Delete"),
                                visible: abp.auth.isGranted('BackOffice.Categories.Delete'),
                                confirmMessage: function () {
                                    return l("DeleteConfirmationMessage");
                                },
                                action: function (data) {
                                    categoryService.delete(data.record.category.id)
                                        .then(function () {
                                            abp.notify.info(l("SuccessfullyDeleted"));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
			{ data: "category.name" },
			{ data: "category.slug" },
			{ data: "category.description" },
            {
                data: "category.categoryType",
                render: function (categoryType) {
                    if (categoryType === undefined ||
                        categoryType === null) {
                        return "";
                    }

                    var localizationKey = "Enum:CategoryType." + categoryType;
                    var localized = l(localizationKey);

                    if (localized === localizationKey) {
                        abp.log.warn("No localization found for " + localizationKey);
                        return "";
                    }

                    return localized;
                }
            },
            {
                data: "category1.name",
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

    $("#NewCategoryButton").click(function (e) {
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
