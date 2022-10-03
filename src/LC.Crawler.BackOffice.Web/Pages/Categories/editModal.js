var abp = abp || {};

abp.modals.categoryEdit = function () {
    var initModal = function (publicApi, args) {
        var l = abp.localization.getResource("BackOffice");
        
        
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
        
        $('#Category1LookupOpenButton').on('click', '', function () {
            lastNpDisplayNameId = 'Category1_Name';
            lastNpIdId = 'Category1_Id';
            _lookupModal.open({
                currentId: $('#Category1_Id').val(),
                currentDisplayName: $('#Category1_Name').val(),
                serviceMethod: function() {
                    
                    return window.lC.crawler.backOffice.categories.categories.getCategoryLookup;
                }
            });
        });
        
        
    };

    return {
        initModal: initModal
    };
};
